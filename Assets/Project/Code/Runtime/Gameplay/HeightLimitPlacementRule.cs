using Assets.Project.Code.Runtime.Configuration;
using Assets.Project.Code.Runtime.Model;
using Assets.Project.Code.Runtime.Presentation;

namespace Assets.Project.Code.Runtime.Gameplay
{
    public sealed class HeightLimitPlacementRule : ICubePlacementRule
    {
        private readonly TowerLayout layout;
        private readonly ITowerArea towerArea;

        public HeightLimitPlacementRule(TowerLayout layout, ITowerArea towerArea)
        {
            this.layout = layout;
            this.towerArea = towerArea;
        }

        public bool CanPlace(TowerModel tower, in PlacementContext context, out GameMessageId failMessage)
        {
            var projectedTop = layout.GetTopY(tower, towerArea) + context.CubeSize;
            if (projectedTop <= towerArea.TopY)
            {
                failMessage = GameMessageId.None;
                return true;
            }

            failMessage = GameMessageId.HeightLimitReached;
            return false;
        }
    }
}
