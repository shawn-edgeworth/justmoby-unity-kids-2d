using Assets.Project.Code.Runtime.Configuration;
using Assets.Project.Code.Runtime.Infrastructure.Pooling;
using Assets.Project.Code.Runtime.Model;
using Assets.Project.Code.Runtime.Presentation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Project.Code.Runtime.Gameplay
{
    public sealed class PalettePlaceFlow
    {
        private readonly TowerModel tower;
        private readonly PlacementRules placementRules;
        private readonly CubePrefabsConfig cubePrefabsConfig;
        private readonly GameAnimationConfig animationConfig;
        private readonly SceneRefs sceneRefs;
        private readonly IPrefabPool prefabPool;
        private readonly ITowerArea towerArea;
        private readonly GameStateMachine gameStateMachine;
        private readonly TowerView towerView;
        private readonly GameplayFeedback feedback;
        private readonly GamePersistence persistence;
        private readonly CubeDestroyFlow destroyFlow;

        private CubeView ghostCube;
        private bool isPaletteScrollLocked;
        private bool paletteScrollWasEnabled;

        public PalettePlaceFlow(
            TowerModel tower,
            PlacementRules placementRules,
            CubePrefabsConfig cubePrefabsConfig,
            GameAnimationConfig animationConfig,
            SceneRefs sceneRefs,
            IPrefabPool prefabPool,
            ITowerArea towerArea,
            GameStateMachine gameStateMachine,
            TowerView towerView,
            GameplayFeedback feedback,
            GamePersistence persistence,
            CubeDestroyFlow destroyFlow)
        {
            this.tower = tower;
            this.placementRules = placementRules;
            this.cubePrefabsConfig = cubePrefabsConfig;
            this.animationConfig = animationConfig;
            this.sceneRefs = sceneRefs;
            this.prefabPool = prefabPool;
            this.towerArea = towerArea;
            this.gameStateMachine = gameStateMachine;
            this.towerView = towerView;
            this.feedback = feedback;
            this.persistence = persistence;
            this.destroyFlow = destroyFlow;
        }

        public void OnBeginDrag(CubeView source, Vector2 position)
        {
            if (!gameStateMachine.CanStartDrag)
            {
                return;
            }

            gameStateMachine.Set(GameState.DraggingFromPalette);
            LockPaletteScroll();
            DestroyGhost();
            ghostCube = prefabPool.Spawn(cubePrefabsConfig.CubePrefab.gameObject).GetComponent<CubeView>();
            ghostCube.SetParent(sceneRefs.DragLayer, false);
            ghostCube.ApplyVisual(source.ColorId, source.Color);
            ghostCube.SetWorldSize(source.Size);
            ghostCube.SetFollowLerpSpeed(animationConfig.FollowLerpSpeed);
            ghostCube.BeginFollow(position);
            feedback.PlayPick();
            feedback.ShowPickedFromPalette();
        }

        public void OnDrag(CubeView source, Vector2 position)
        {
            if (gameStateMachine.Current != GameState.DraggingFromPalette)
            {
                return;
            }

            ghostCube.UpdateFollowTarget(position);
        }

        public void OnEndDrag(CubeView source, Vector2 position)
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
                feedback.Show(failMessage);

                UnlockPaletteScroll();
                if (failMessage == GameMessageId.HeightLimitReached)
                {
                    destroyFlow.BeginHeightReject(ghostCube, ClearGhostReference);
                    return;
                }

                destroyFlow.Begin(ghostCube, CubeDestroyFlow.DestroyKind.GhostRejected, ClearGhostReference);
                return;
            }

            var positionX = ResolvePlacementPositionX(dropCenter.x, size);
            var cube = new CubeData(colorId, positionX, size);
            tower.Add(cube);
            DestroyGhost();
            UnlockPaletteScroll();

            gameStateMachine.Set(GameState.AnimatingToTower);
            towerView.Spawn(cube, dropCenter, true, OnPlaceAnimationComplete);

            feedback.ShowCubePlaced();
            persistence.Save();
        }

        public void Cancel()
        {
            UnlockPaletteScroll();
            DestroyGhost();
            destroyFlow.Cancel();
        }

        private void OnPlaceAnimationComplete()
        {
            feedback.PlayPlaceOrSettle();
            gameStateMachine.Set(GameState.Idle);
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

        private void DestroyGhost()
        {
            if (ghostCube == null)
            {
                return;
            }

            ghostCube.EndFollow();
            prefabPool.Despawn(ghostCube.gameObject);
            ghostCube = null;
        }

        private void ClearGhostReference()
        {
            ghostCube = null;
        }
    }
}
