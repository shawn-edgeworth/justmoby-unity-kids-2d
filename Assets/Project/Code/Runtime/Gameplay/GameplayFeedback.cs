using Assets.Project.Code.Runtime.Audio;
using Assets.Project.Code.Runtime.Configuration;
using Assets.Project.Code.Runtime.Presentation;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Gameplay
{
    public sealed class GameplayFeedback
    {
        private readonly AudioService audioService;
        private readonly SceneRefs sceneRefs;
        private readonly PlacementRules placementRules;

        private bool isHoveringHole;

        public GameplayFeedback(
            AudioService audioService,
            SceneRefs sceneRefs,
            PlacementRules placementRules)
        {
            this.audioService = audioService;
            this.sceneRefs = sceneRefs;
            this.placementRules = placementRules;
        }

        public void Show(GameMessageId messageId)
        {
            if (messageId == GameMessageId.None)
            {
                return;
            }

            sceneRefs.MessageView.Show(messageId);
        }

        public void ShowPickedFromPalette()
        {
            sceneRefs.MessageView.Show(GameMessageId.CubePickedFromPalette);
        }

        public void ShowCubePlaced()
        {
            sceneRefs.MessageView.Show(GameMessageId.CubePlaced);
        }

        public void ShowPickedFromTower()
        {
            sceneRefs.MessageView.Show(GameMessageId.CubePickedFromTower);
        }

        public void ShowCubeDestroyed()
        {
            sceneRefs.MessageView.Show(GameMessageId.CubeDestroyed);
        }

        public void ShowCubeAbsorbedByHole()
        {
            sceneRefs.MessageView.Show(GameMessageId.CubeAbsorbedByHole);
        }

        public void PlayPick()
        {
            audioService.PlayPick();
        }

        public void PlayPlaceOrSettle()
        {
            audioService.PlayPlaceOrSettle();
        }

        public void PlayInvalidPlace()
        {
            audioService.PlayInvalidPlace();
        }

        public void PlayDestroyWhoosh()
        {
            audioService.PlayDestroyWhoosh();
        }

        public void PlayHoleFlight()
        {
            audioService.PlayHoleFlight();
        }

        public void UpdateHoleHoverFeedback(Vector2 cubeCenter)
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

        public void StopHoleHoverFeedback()
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
    }
}
