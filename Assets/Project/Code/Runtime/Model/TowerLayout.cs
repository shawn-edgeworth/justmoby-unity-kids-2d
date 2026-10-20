using System.Collections.Generic;
using Assets.Project.Code.Runtime.Presentation;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Model
{
    public sealed class TowerLayout
    {
        public void CalculatePositions(TowerModel tower, ITowerArea towerArea, List<Vector2> results)
        {
            results.Clear();

            if (tower.IsEmpty)
            {
                return;
            }

            var y = towerArea.BottomY;
            var cubes = tower.Cubes;

            for (var i = 0; i < cubes.Count; i++)
            {
                var size = cubes[i].Size;
                var centerY = y + size * 0.5f;
                results.Add(new Vector2(cubes[i].PositionX, centerY));
                y += size;
            }
        }

        public float GetTopY(TowerModel tower, ITowerArea towerArea)
        {
            if (tower.IsEmpty)
            {
                return towerArea.BottomY;
            }

            var y = towerArea.BottomY;
            var cubes = tower.Cubes;
            for (var i = 0; i < cubes.Count; i++)
            {
                y += cubes[i].Size;
            }

            return y;
        }
    }
}
