using Assets.Project.Code.Runtime.Configuration;
using Assets.Project.Code.Runtime.Model;

namespace Assets.Project.Code.Runtime.Gameplay
{
    public sealed class MatchingTopColorPlacementRule : ICubePlacementRule
    {
        public bool CanPlace(TowerModel tower, in PlacementContext context, out GameMessageId failMessage)
        {
            if (!tower.TryGetTop(out var top))
            {
                failMessage = GameMessageId.None;
                return true;
            }

            if (top.ColorId == context.ColorId)
            {
                failMessage = GameMessageId.None;
                return true;
            }

            failMessage = GameMessageId.ColorMismatch;
            return false;
        }
    }
}
