using Assets.Project.Code.Runtime.Configuration;
using Assets.Project.Code.Runtime.Presentation;
using DG.Tweening;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Gameplay
{
    public sealed class HoleAbsorbFlow
    {
        private readonly GameStateMachine gameStateMachine;
        private readonly GameAnimationConfig animationConfig;
        private readonly GameAudioConfig audioConfig;
        private readonly SceneRefs sceneRefs;
        private readonly TowerView towerView;
        private readonly GameplayFeedback feedback;
        private readonly GamePersistence persistence;

        private CubeView holeAbsorbCube;
        private Tween consumeVfxDelayTween;

        public HoleAbsorbFlow(
            GameStateMachine gameStateMachine,
            GameAnimationConfig animationConfig,
            GameAudioConfig audioConfig,
            SceneRefs sceneRefs,
            TowerView towerView,
            GameplayFeedback feedback,
            GamePersistence persistence)
        {
            this.gameStateMachine = gameStateMachine;
            this.animationConfig = animationConfig;
            this.audioConfig = audioConfig;
            this.sceneRefs = sceneRefs;
            this.towerView = towerView;
            this.feedback = feedback;
            this.persistence = persistence;
        }

        public void Begin(CubeView cube)
        {
            holeAbsorbCube = cube;
            gameStateMachine.Set(GameState.ConsumingByHole);
            var halfDuration = animationConfig.HoleAbsorbDuration * 0.5f;
            cube.MoveTo(sceneRefs.HoleAbovePoint.position, halfDuration, Ease.OutQuad, OnHoleAbsorbReachedAbove);
        }

        public void Cancel()
        {
            holeAbsorbCube = null;
            KillConsumeVfxDelay();
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
            feedback.PlayHoleFlight();
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
            feedback.ShowCubeAbsorbedByHole();
            gameStateMachine.Set(GameState.Idle);
            persistence.Save();
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
    }
}
