using System;

namespace Assets.Project.Code.Runtime.Gameplay
{
    public sealed class GameStateMachine
    {
        public GameState Current { get; private set; } = GameState.Idle;

        public bool CanStartDrag => Current == GameState.Idle;

        public void Set(GameState state)
        {
            Current = state;
        }

        public void Ensure(GameState expected)
        {
            if (Current != expected)
            {
                throw new InvalidOperationException(
                    $"Expected game state {expected}, but was {Current}.");
            }
        }
    }
}
