using Assets.Project.Code.Runtime.Configuration;
using Assets.Project.Code.Runtime.Model;
using Assets.Project.Code.Runtime.Presentation;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Gameplay
{
    public sealed class PlacementRules
    {
        private readonly TowerModel tower;
        private readonly ICubePlacementRule[] cubePlacementRules;
        private readonly ITowerArea towerArea;
        private readonly IHoleArea holeArea;

        public PlacementRules(
            TowerModel tower,
            ICubePlacementRule[] cubePlacementRules,
            ITowerArea towerArea,
            IHoleArea holeArea)
        {
            this.tower = tower;
            this.cubePlacementRules = cubePlacementRules;
            this.towerArea = towerArea;
            this.holeArea = holeArea;
        }

        public bool CanPlaceAt(in PlacementContext context, out GameMessageId failMessage)
        {
            failMessage = GameMessageId.None;

            if (!towerArea.Contains(context.DropCenter))
            {
                failMessage = GameMessageId.CubeMissed;
                return false;
            }

            for (var i = 0; i < cubePlacementRules.Length; i++)
            {
                if (!cubePlacementRules[i].CanPlace(tower, in context, out failMessage))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInsideHole(Vector2 cubeCenter)
        {
            return holeArea.Contains(cubeCenter);
        }
    }
}
