using System;
using System.Collections.Generic;
using Assets.Project.Code.Runtime.Model;
using Assets.Project.Code.Runtime.Presentation;
using UnityEngine;
using Zenject;

namespace Assets.Project.Code.Runtime.Persistence
{
    public sealed class GameSaveMapper
    {
        private readonly ITowerArea towerArea;

        public GameSaveMapper(ITowerArea towerArea)
        {
            this.towerArea = towerArea;
        }

        public GameSaveData ToSaveData(TowerModel tower)
        {
            var areaWidth = GetAreaWidth();
            var areaHeight = GetAreaHeight();

            var data = new GameSaveData
            {
                Cubes = new List<CubeSaveData>(tower.Count)
            };

            var cubes = tower.Cubes;
            for (var i = 0; i < cubes.Count; i++)
            {
                var cube = cubes[i];
                data.Cubes.Add(new CubeSaveData
                {
                    ColorId = cube.ColorId,
                    NormalizedCenterX = Mathf.Clamp01((cube.PositionX - towerArea.MinX) / areaWidth),
                    NormalizedSize = cube.Size / areaHeight
                });
            }

            return data;
        }

        public bool ApplyToTower(GameSaveData data, TowerModel tower)
        {
            var areaWidth = GetAreaWidth();
            var areaHeight = GetAreaHeight();
            var buffer = ListPool<CubeData>.Instance.Spawn();
            var wasTruncated = false;

            try
            {
                for (var i = 0; i < data.Cubes.Count; i++)
                {
                    var entry = data.Cubes[i];
                    var worldSize = entry.NormalizedSize * areaHeight;
                    var worldX = towerArea.MinX + entry.NormalizedCenterX * areaWidth;
                    worldX = towerArea.ClampBaseCenterX(worldX, worldSize);

                    if (!CanFitAdditionalHeight(buffer, worldSize))
                    {
                        wasTruncated = true;
                        break;
                    }

                    buffer.Add(new CubeData(entry.ColorId, worldX, worldSize));
                }

                tower.ReplaceAll(buffer);
            }
            finally
            {
                ListPool<CubeData>.Instance.Despawn(buffer);
            }

            return wasTruncated;
        }

        private bool CanFitAdditionalHeight(List<CubeData> currentCubes, float additionalSize)
        {
            var topY = towerArea.BottomY;
            for (var i = 0; i < currentCubes.Count; i++)
            {
                topY += currentCubes[i].Size;
            }

            return topY + additionalSize <= towerArea.TopY;
        }

        private float GetAreaWidth()
        {
            var width = towerArea.MaxX - towerArea.MinX;
            if (width <= 0f)
            {
                throw new InvalidOperationException("TowerArea width must be positive.");
            }

            return width;
        }

        private float GetAreaHeight()
        {
            var height = towerArea.TopY - towerArea.BottomY;
            if (height <= 0f)
            {
                throw new InvalidOperationException("TowerArea height must be positive.");
            }

            return height;
        }
    }
}
