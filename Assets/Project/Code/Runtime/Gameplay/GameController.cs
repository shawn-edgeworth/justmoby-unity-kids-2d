using System;
using Assets.Project.Code.Runtime.Audio;
using Assets.Project.Code.Runtime.Configuration;
using Assets.Project.Code.Runtime.Infrastructure.Pooling;
using Assets.Project.Code.Runtime.Model;
using Assets.Project.Code.Runtime.Persistence;
using Assets.Project.Code.Runtime.Presentation;
using DG.Tweening;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Assets.Project.Code.Runtime.Gameplay
{
    public sealed class GameController : IInitializable, IDisposable
    {
        private enum DestroyKind
        {
            GhostRejected = 0,
            TowerMissed = 1
        }

        private readonly TowerModel tower;
        private readonly PlacementRules placementRules;
        private readonly ISaveService saveService;
        private readonly GameSaveMapper saveMapper;
        private readonly CubePrefabsConfig cubePrefabsConfig;
        private readonly VFXPrefabsConfig vfxPrefabsConfig;
        private readonly GameAnimationConfig animationConfig;
        private readonly GameAudioConfig audioConfig;
        private readonly AudioService audioService;
        private readonly SceneRefs sceneRefs;
        private readonly IPrefabPool prefabPool;
        private readonly ITowerArea towerArea;
        private readonly GameStateMachine gameStateMachine;
        private readonly PaletteView paletteView;
        private readonly TowerView towerView;

        private CubeView ghostCube;
        private CubeView draggedTowerCube;
        private CubeView pendingDestroyCube;
        private CubeView holeAbsorbCube;
        private DestroyKind pendingDestroyKind;
        private bool isPaletteScrollLocked;
        private bool paletteScrollWasEnabled;
        private bool isHoveringHole;
        private Tween consumeVfxDelayTween;

        public GameController(
            TowerModel tower,
            PlacementRules placementRules,
            ISaveService saveService,
            GameSaveMapper saveMapper,
            CubePrefabsConfig cubePrefabsConfig,
            VFXPrefabsConfig vfxPrefabsConfig,
            GameAnimationConfig animationConfig,
            GameAudioConfig audioConfig,
            AudioService audioService,
            SceneRefs sceneRefs,
            IPrefabPool prefabPool,
            ITowerArea towerArea,
            GameStateMachine gameStateMachine,
            PaletteView paletteView,
            TowerView towerView)
        {
            this.tower = tower;
            this.placementRules = placementRules;
            this.saveService = saveService;
            this.saveMapper = saveMapper;
            this.cubePrefabsConfig = cubePrefabsConfig;
            this.vfxPrefabsConfig = vfxPrefabsConfig;
            this.animationConfig = animationConfig;
            this.audioConfig = audioConfig;
            this.audioService = audioService;
            this.sceneRefs = sceneRefs;
            this.prefabPool = prefabPool;
            this.towerArea = towerArea;
            this.gameStateMachine = gameStateMachine;
            this.paletteView = paletteView;
            this.towerView = towerView;
        }

        public void Initialize()
        {
            LoadOrStartNew();
            paletteView.Build();
            towerView.Rebuild();

            paletteView.CubeBeginDrag += OnPaletteBeginDrag;
            paletteView.CubeDrag += OnPaletteDrag;
            paletteView.CubeEndDrag += OnPaletteEndDrag;

            towerView.CubeBeginDrag += OnTowerBeginDrag;
            towerView.CubeDrag += OnTowerDrag;
            towerView.CubeEndDrag += OnTowerEndDrag;
            towerView.CubeSettled += OnCubeSettled;
        }

        public void Dispose()
        {
            paletteView.CubeBeginDrag -= OnPaletteBeginDrag;
            paletteView.CubeDrag -= OnPaletteDrag;
            paletteView.CubeEndDrag -= OnPaletteEndDrag;

            towerView.CubeBeginDrag -= OnTowerBeginDrag;
            towerView.CubeDrag -= OnTowerDrag;
            towerView.CubeEndDrag -= OnTowerEndDrag;
            towerView.CubeSettled -= OnCubeSettled;

            UnlockPaletteScroll();
            DestroyGhost();
            StopHoleHoverFeedback();
            paletteView.Clear();
            towerView.ClearViews();
            ClearPendingAnimations();
            gameStateMachine.Set(GameState.Idle);
        }

        private void LoadOrStartNew()
        {
            if (!saveService.TryLoad(out GameSaveData data))
            {
                tower.Clear();
                return;
            }

            try
            {
                var wasTruncated = saveMapper.ApplyToTower(data, tower);
                if (wasTruncated)
                {
                    Save();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                tower.Clear();
            }
        }

        private void OnPaletteBeginDrag(CubeView source, Vector2 position)
        {
            if (!gameStateMachine.CanStartDrag)
            {
                return;
            }

            gameStateMachine.Set(GameState.DraggingFromPalette);
            LockPaletteScroll();
            DestroyGhost();
            ghostCube = prefabPool.Spawn(cubePrefabsConfig.GhostCubePrefab.gameObject).GetComponent<CubeView>();
            ghostCube.SetParent(sceneRefs.DragLayer, false);
            ghostCube.ApplyVisual(source.ColorId, source.Color);
            ghostCube.SetWorldSize(source.Size);
            ghostCube.SetFollowLerpSpeed(animationConfig.FollowLerpSpeed);
            ghostCube.BeginFollow(position);
            audioService.PlayPick();
            sceneRefs.MessageView.Show(GameMessageId.CubePickedFromPalette);
        }

        private void OnPaletteDrag(CubeView source, Vector2 position)
        {
            if (gameStateMachine.Current != GameState.DraggingFromPalette)
            {
                return;
            }

            ghostCube.UpdateFollowTarget(position);
        }

        private void OnPaletteEndDrag(CubeView source, Vector2 position)
        {
            if (gameStateMachine.Current != GameState.DraggingFromPalette)
            {
                return;
            }

            ghostCube.EndFollow();
            ghostCube.UpdateFollowTarget(position);
            ghostCube.SetPosition(position);

            var colorId = ghostCube.ColorId;
            var size = ghostCube.Size;
            var dropCenter = ghostCube.Position;
            var context = new PlacementContext(colorId, size, dropCenter);

            if (!placementRules.CanPlaceAt(in context, out var failMessage))
            {
                if (failMessage != GameMessageId.None)
                {
                    sceneRefs.MessageView.Show(failMessage);
                }

                if (failMessage == GameMessageId.HeightLimitReached)
                {
                    audioService.PlayInvalidPlace();
                }

                UnlockPaletteScroll();
                BeginDestroy(ghostCube, DestroyKind.GhostRejected);
                return;
            }

            var positionX = ResolvePlacementPositionX(dropCenter.x, size);
            var cube = new CubeData(colorId, positionX, size);
            tower.Add(cube);
            DestroyGhost();
            UnlockPaletteScroll();

            gameStateMachine.Set(GameState.AnimatingToTower);
            towerView.Spawn(cube, dropCenter, true, OnPlaceAnimationComplete);

            sceneRefs.MessageView.Show(GameMessageId.CubePlaced);
            Save();
        }

        private float ResolvePlacementPositionX(float dropX, float cubeSize)
        {
            if (tower.IsEmpty)
            {
                return towerArea.ClampBaseCenterX(dropX, cubeSize);
            }

            var topCube = tower.GetAt(tower.Count - 1);
            var randomOffset = Random.Range(-topCube.Size * 0.5f, topCube.Size * 0.5f);
            return towerArea.ResolveStackedCenterX(topCube.PositionX, randomOffset, cubeSize);
        }

        private void OnTowerBeginDrag(CubeView view, Vector2 position)
        {
            if (!gameStateMachine.CanStartDrag)
            {
                return;
            }

            gameStateMachine.Set(GameState.DraggingFromTower);
            var index = view.Index;
            tower.RemoveAt(index);
            draggedTowerCube = towerView.DetachAt(index);
            draggedTowerCube.BeginFollow(position);
            audioService.PlayPick();
            sceneRefs.MessageView.Show(GameMessageId.CubePickedFromTower);
            Save();
            UpdateHoleHoverFeedback(position);
            towerView.ApplySettle(true, null);
        }

        private void OnTowerDrag(CubeView view, Vector2 position)
        {
            if (gameStateMachine.Current != GameState.DraggingFromTower)
            {
                return;
            }

            draggedTowerCube.UpdateFollowTarget(position);
            UpdateHoleHoverFeedback(position);
        }

        private void OnTowerEndDrag(CubeView view, Vector2 position)
        {
            if (gameStateMachine.Current != GameState.DraggingFromTower)
            {
                return;
            }

            var cube = draggedTowerCube;
            draggedTowerCube = null;
            cube.EndFollow();
            cube.UpdateFollowTarget(position);
            cube.SetPosition(position);
            StopHoleHoverFeedback();

            if (placementRules.IsInsideHole(cube.Position))
            {
                gameStateMachine.Set(GameState.ConsumingByHole);
                BeginHoleAbsorb(cube);
                return;
            }

            BeginDestroy(cube, DestroyKind.TowerMissed);
        }

        private void OnPlaceAnimationComplete()
        {
            audioService.PlayPlaceOrSettle();
            gameStateMachine.Set(GameState.Idle);
        }

        private void OnCubeSettled()
        {
            audioService.PlayPlaceOrSettle();
        }

        private void BeginDestroy(CubeView cube, DestroyKind kind)
        {
            pendingDestroyCube = cube;
            pendingDestroyKind = kind;
            gameStateMachine.Set(GameState.Destroying);

            PlayDestroySmoke(cube.Position);
            audioService.PlayDestroyWhoosh();
            cube.PlayDestroy(animationConfig.DestroyDuration, OnDestroyAnimationComplete);
        }

        private void PlayDestroySmoke(Vector2 worldPosition)
        {
            var effect = prefabPool.Spawn(vfxPrefabsConfig.DestroySmoke.gameObject).GetComponent<VfxEffect>();
            effect.transform.SetParent(sceneRefs.DragLayer, false);
            effect.Play(worldPosition, () => prefabPool.Despawn(effect.gameObject));
        }

        private void OnDestroyAnimationComplete()
        {
            var cube = pendingDestroyCube;
            if (cube == null || gameStateMachine.Current != GameState.Destroying)
            {
                return;
            }

            pendingDestroyCube = null;

            if (cube == ghostCube)
            {
                prefabPool.Despawn(cube.gameObject);
                ghostCube = null;
            }
            else
            {
                towerView.Despawn(cube);
            }

            if (pendingDestroyKind == DestroyKind.TowerMissed)
            {
                sceneRefs.MessageView.Show(GameMessageId.CubeDestroyed);
                Save();
            }

            gameStateMachine.Set(GameState.Idle);
        }

        private void BeginHoleAbsorb(CubeView cube)
        {
            holeAbsorbCube = cube;
            var halfDuration = animationConfig.HoleAbsorbDuration * 0.5f;
            cube.MoveTo(sceneRefs.HoleAbovePoint.position, halfDuration, Ease.OutQuad, OnHoleAbsorbReachedAbove);
        }

        private void OnHoleAbsorbReachedAbove()
        {
            var cube = holeAbsorbCube;
            if (cube == null || gameStateMachine.Current != GameState.ConsumingByHole)
            {
                return;
            }

            cube.SetParent(sceneRefs.HoleClipRoot, true);
            cube.SetMaterial(sceneRefs.HoleAbsorbMaterial);
            var halfDuration = animationConfig.HoleAbsorbDuration * 0.5f;
            audioService.PlayHoleFlight();
            ScheduleConsumeVfx(halfDuration);
            cube.MoveTo(sceneRefs.HoleInsidePoint.position, halfDuration, Ease.OutQuad, OnHoleAbsorbReachedInside);
        }

        private void OnHoleAbsorbReachedInside()
        {
            var cube = holeAbsorbCube;
            if (cube == null || gameStateMachine.Current != GameState.ConsumingByHole)
            {
                return;
            }

            KillConsumeVfxDelay();
            holeAbsorbCube = null;
            towerView.Despawn(cube);
            sceneRefs.MessageView.Show(GameMessageId.CubeAbsorbedByHole);
            gameStateMachine.Set(GameState.Idle);
            Save();
        }

        private void ScheduleConsumeVfx(float halfDuration)
        {
            KillConsumeVfxDelay();
            var delay = halfDuration * audioConfig.ConsumeVfxProgress;
            consumeVfxDelayTween = DOVirtual.DelayedCall(delay, PlayConsumeVfx);
        }

        private void PlayConsumeVfx()
        {
            consumeVfxDelayTween = null;
            if (gameStateMachine.Current != GameState.ConsumingByHole)
            {
                return;
            }

            sceneRefs.Hole.PlayConsume();
        }

        private void KillConsumeVfxDelay()
        {
            if (consumeVfxDelayTween == null)
            {
                return;
            }

            consumeVfxDelayTween.Kill();
            consumeVfxDelayTween = null;
        }

        private void UpdateHoleHoverFeedback(Vector2 cubeCenter)
        {
            var insideHole = placementRules.IsInsideHole(cubeCenter);
            if (insideHole == isHoveringHole)
            {
                return;
            }

            isHoveringHole = insideHole;
            sceneRefs.Hole.SetHoleHoverActive(insideHole);
            if (insideHole)
            {
                audioService.StartHoleHoverLoop();
            }
            else
            {
                audioService.StopHoleHoverLoop();
            }
        }

        private void StopHoleHoverFeedback()
        {
            if (!isHoveringHole)
            {
                sceneRefs.Hole.SetHoleHoverActive(false);
                audioService.StopHoleHoverLoop();
                return;
            }

            isHoveringHole = false;
            sceneRefs.Hole.SetHoleHoverActive(false);
            audioService.StopHoleHoverLoop();
        }

        private void LockPaletteScroll()
        {
            var scrollRect = sceneRefs.PaletteScrollRect;
            paletteScrollWasEnabled = scrollRect.enabled;
            scrollRect.enabled = false;
            isPaletteScrollLocked = true;
        }

        private void UnlockPaletteScroll()
        {
            if (!isPaletteScrollLocked)
            {
                return;
            }

            sceneRefs.PaletteScrollRect.enabled = paletteScrollWasEnabled;
            isPaletteScrollLocked = false;
        }

        private void Save()
        {
            saveService.Save(saveMapper.ToSaveData(tower));
        }

        private void DestroyGhost()
        {
            if (ghostCube == null)
            {
                ghostCube = null;
                return;
            }

            ghostCube.EndFollow();
            prefabPool.Despawn(ghostCube.gameObject);
            ghostCube = null;
        }

        private void ClearPendingAnimations()
        {
            pendingDestroyCube = null;
            holeAbsorbCube = null;
            KillConsumeVfxDelay();
        }
    }
}
