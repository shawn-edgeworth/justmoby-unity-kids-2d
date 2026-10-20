using Assets.Project.Code.Runtime.Configuration;
using Assets.Project.Code.Runtime.Model;
using Assets.Project.Code.Runtime.Presentation;

namespace Assets.Project.Code.Runtime.Gameplay
{
    /// <summary>
    /// Ghost may stack only when its bottom is at/above the top cube midline
    /// and at least one of its side edges is strictly inside the top cube sides.
    /// </summary>
    public sealed class TopCubeStackPlacementRule : ICubePlacementRule
    {
        private readonly TowerLayout layout;
        private readonly ITowerArea towerArea;

        public TopCubeStackPlacementRule(TowerLayout layout, ITowerArea towerArea)
        {
            this.layout = layout;
            this.towerArea = towerArea;
        }

        public bool CanPlace(TowerModel tower, in PlacementContext context, out GameMessageId failMessage)
        {
            if (!tower.TryGetTop(out var top))
            {
                failMessage = GameMessageId.None;
                return true;
            }

            var topTopY = layout.GetTopY(tower, towerArea);
            var topHalf = top.Size * 0.5f;
            var topMidY = topTopY - topHalf;
            var topLeft = top.PositionX - topHalf;
            var topRight = top.PositionX + topHalf;

            var ghostHalf = context.CubeSize * 0.5f;
            var ghostBottom = context.DropCenter.y - ghostHalf;
            var ghostLeft = context.DropCenter.x - ghostHalf;
            var ghostRight = context.DropCenter.x + ghostHalf;

            var isVerticalOk = ghostBottom >= topMidY;
            var isLeftEdgeInside = ghostLeft > topLeft && ghostLeft < topRight;
            var isRightEdgeInside = ghostRight > topLeft && ghostRight < topRight;
            var isHorizontalOk = isLeftEdgeInside || isRightEdgeInside;

            if (isVerticalOk && isHorizontalOk)
            {
                failMessage = GameMessageId.None;
                return true;
            }

            failMessage = GameMessageId.CubeMissed;
            return false;
        }
    }
}
