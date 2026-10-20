using Assets.Project.Code.Runtime.Model;
using Assets.Project.Code.Runtime.Presentation;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Gameplay
{
    public sealed class TowerRemoveFlow
    {
        private readonly TowerModel tower;
        private readonly PlacementRules placementRules;
        private readonly GameStateMachine gameStateMachine;
        private readonly TowerView towerView;
        private readonly GameplayFeedback feedback;
        private readonly GamePersistence persistence;
        private readonly HoleAbsorbFlow holeAbsorbFlow;
        private readonly CubeDestroyFlow destroyFlow;

        private CubeView draggedTowerCube;

        public TowerRemoveFlow(
            TowerModel tower,
            PlacementRules placementRules,
            GameStateMachine gameStateMachine,
            TowerView towerView,
            GameplayFeedback feedback,
            GamePersistence persistence,
            HoleAbsorbFlow holeAbsorbFlow,
            CubeDestroyFlow destroyFlow)
        {
            this.tower = tower;
            this.placementRules = placementRules;
            this.gameStateMachine = gameStateMachine;
            this.towerView = towerView;
            this.feedback = feedback;
            this.persistence = persistence;
            this.holeAbsorbFlow = holeAbsorbFlow;
            this.destroyFlow = destroyFlow;
        }

        public void OnBeginDrag(CubeView view, Vector2 position)
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
            feedback.PlayPick();
            feedback.ShowPickedFromTower();
            persistence.Save();
            feedback.UpdateHoleHoverFeedback(position);
            towerView.ApplySettle(true, null);
        }

        public void OnDrag(CubeView view, Vector2 position)
        {
            if (gameStateMachine.Current != GameState.DraggingFromTower)
            {
                return;
            }

            draggedTowerCube.UpdateFollowTarget(position);
            feedback.UpdateHoleHoverFeedback(position);
        }

        public void OnEndDrag(CubeView view, Vector2 position)
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
            feedback.StopHoleHoverFeedback();

            if (placementRules.IsInsideHole(cube.Position))
            {
                holeAbsorbFlow.Begin(cube);
                return;
            }

            destroyFlow.Begin(cube, CubeDestroyFlow.DestroyKind.TowerMissed);
        }

        public void OnCubeSettled()
        {
            feedback.PlayPlaceOrSettle();
        }

        public void Cancel()
        {
            feedback.StopHoleHoverFeedback();
            draggedTowerCube = null;
            holeAbsorbFlow.Cancel();
            destroyFlow.Cancel();
        }
    }
}
