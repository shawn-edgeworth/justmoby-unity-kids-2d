using System;
using Assets.Project.Code.Runtime.Presentation;
using Zenject;

namespace Assets.Project.Code.Runtime.Gameplay
{
    public sealed class GameController : IInitializable, IDisposable
    {
        private readonly GamePersistence persistence;
        private readonly PalettePlaceFlow paletteFlow;
        private readonly TowerRemoveFlow towerFlow;
        private readonly PaletteView paletteView;
        private readonly TowerView towerView;
        private readonly GameStateMachine gameStateMachine;

        public GameController(
            GamePersistence persistence,
            PalettePlaceFlow paletteFlow,
            TowerRemoveFlow towerFlow,
            PaletteView paletteView,
            TowerView towerView,
            GameStateMachine gameStateMachine)
        {
            this.persistence = persistence;
            this.paletteFlow = paletteFlow;
            this.towerFlow = towerFlow;
            this.paletteView = paletteView;
            this.towerView = towerView;
            this.gameStateMachine = gameStateMachine;
        }

        public void Initialize()
        {
            persistence.LoadOrStartNew();
            paletteView.Build();
            towerView.Rebuild();

            paletteView.CubeBeginDrag += paletteFlow.OnBeginDrag;
            paletteView.CubeDrag += paletteFlow.OnDrag;
            paletteView.CubeEndDrag += paletteFlow.OnEndDrag;

            towerView.CubeBeginDrag += towerFlow.OnBeginDrag;
            towerView.CubeDrag += towerFlow.OnDrag;
            towerView.CubeEndDrag += towerFlow.OnEndDrag;
            towerView.CubeSettled += towerFlow.OnCubeSettled;
        }

        public void Dispose()
        {
            paletteView.CubeBeginDrag -= paletteFlow.OnBeginDrag;
            paletteView.CubeDrag -= paletteFlow.OnDrag;
            paletteView.CubeEndDrag -= paletteFlow.OnEndDrag;

            towerView.CubeBeginDrag -= towerFlow.OnBeginDrag;
            towerView.CubeDrag -= towerFlow.OnDrag;
            towerView.CubeEndDrag -= towerFlow.OnEndDrag;
            towerView.CubeSettled -= towerFlow.OnCubeSettled;

            paletteFlow.Cancel();
            towerFlow.Cancel();
            paletteView.Clear();
            towerView.ClearViews();
            gameStateMachine.Set(GameState.Idle);
        }
    }
}
