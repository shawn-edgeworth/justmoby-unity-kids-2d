using System;
using Assets.Project.Code.Runtime.Configuration;
using Assets.Project.Code.Runtime.Infrastructure.Pooling;
using Assets.Project.Code.Runtime.Presentation;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Gameplay
{
    public sealed class CubeDestroyFlow
    {
        public enum DestroyKind
        {
            GhostRejected = 0,
            TowerMissed = 1
        }

        private readonly GameStateMachine gameStateMachine;
        private readonly GameAnimationConfig animationConfig;
        private readonly VFXPrefabsConfig vfxPrefabsConfig;
        private readonly IPrefabPool prefabPool;
        private readonly SceneRefs sceneRefs;
        private readonly TowerView towerView;
        private readonly GameplayFeedback feedback;
        private readonly GamePersistence persistence;

        private CubeView pendingDestroyCube;
        private DestroyKind pendingDestroyKind;
        private Action onGhostReleased;

        public CubeDestroyFlow(
            GameStateMachine gameStateMachine,
            GameAnimationConfig animationConfig,
            VFXPrefabsConfig vfxPrefabsConfig,
            IPrefabPool prefabPool,
            SceneRefs sceneRefs,
            TowerView towerView,
            GameplayFeedback feedback,
            GamePersistence persistence)
        {
            this.gameStateMachine = gameStateMachine;
            this.animationConfig = animationConfig;
            this.vfxPrefabsConfig = vfxPrefabsConfig;
            this.prefabPool = prefabPool;
            this.sceneRefs = sceneRefs;
            this.towerView = towerView;
            this.feedback = feedback;
            this.persistence = persistence;
        }

        public void BeginHeightReject(CubeView cube, Action onGhostReleased)
        {
            pendingDestroyCube = cube;
            pendingDestroyKind = DestroyKind.GhostRejected;
            this.onGhostReleased = onGhostReleased;
            gameStateMachine.Set(GameState.Destroying);
            feedback.PlayInvalidPlace();
            cube.PlayRejectShake(
                animationConfig.RejectShakeDuration,
                animationConfig.RejectShakeRotationStrength,
                animationConfig.RejectShakeScaleStrength,
                animationConfig.RejectShakeVibrato,
                OnHeightRejectShakeComplete);
        }

        public void Begin(CubeView cube, DestroyKind kind, Action onGhostReleased = null)
        {
            pendingDestroyCube = cube;
            pendingDestroyKind = kind;
            this.onGhostReleased = onGhostReleased;
            gameStateMachine.Set(GameState.Destroying);
            StartDestroyAnimation(cube);
        }

        public void Cancel()
        {
            pendingDestroyCube = null;
            onGhostReleased = null;
        }

        private void OnHeightRejectShakeComplete()
        {
            var cube = pendingDestroyCube;
            if (cube == null || gameStateMachine.Current != GameState.Destroying)
            {
                return;
            }

            StartDestroyAnimation(cube);
        }

        private void StartDestroyAnimation(CubeView cube)
        {
            PlayDestroySmoke(cube.Position);
            feedback.PlayDestroyWhoosh();
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

            var kind = pendingDestroyKind;
            pendingDestroyCube = null;
            var ghostReleased = onGhostReleased;
            onGhostReleased = null;

            if (kind == DestroyKind.GhostRejected)
            {
                prefabPool.Despawn(cube.gameObject);
                ghostReleased?.Invoke();
            }
            else
            {
                towerView.Despawn(cube);
            }

            if (kind == DestroyKind.TowerMissed)
            {
                feedback.ShowCubeDestroyed();
                persistence.Save();
            }

            gameStateMachine.Set(GameState.Idle);
        }
    }
}
