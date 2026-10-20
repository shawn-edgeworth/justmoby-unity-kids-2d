using System;
using System.Collections.Generic;
using Assets.Project.Code.Runtime.Configuration;
using Assets.Project.Code.Runtime.Model;
using Assets.Project.Code.Runtime.Presentation;
using UnityEngine;
using Zenject;

namespace Assets.Project.Code.Runtime.Persistence
{
    public sealed class GameSaveMapper
    {
        private readonly ITowerArea towerArea;
        private readonly CubePrefabsConfig cubePrefabsConfig;
        private readonly SceneRefs sceneRefs;
        private readonly IGameConfigProvider configProvider;

        public GameSaveMapper(
            ITowerArea towerArea,
            CubePrefabsConfig cubePrefabsConfig,
            SceneRefs sceneRefs,
            IGameConfigProvider configProvider)
        {
            this.towerArea = towerArea;
            this.cubePrefabsConfig = cubePrefabsConfig;
            this.sceneRefs = sceneRefs;
            this.configProvider = configProvider;
        }

        public GameSaveData ToSaveData(TowerModel tower)
        {
            var areaWidth = GetAreaWidth();

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
                    NormalizedCenterX = Mathf.Clamp01((cube.PositionX - towerArea.MinX) / areaWidth)
                });
            }

            return data;
        }

        /// <summary>
        /// Applies save data to the tower. Skips cubes with ColorId missing from config
        /// and truncates by height limit. Returns true when the applied state differs from save
        /// and should be re-persisted.
        /// </summary>
        public bool ApplyToTower(GameSaveData data, TowerModel tower)
        {
            var areaWidth = GetAreaWidth();
            var worldSize = ResolveCubeWorldSize();
            var buffer = ListPool<CubeData>.Instance.Spawn();
            var needsResave = false;

            try
            {
                for (var i = 0; i < data.Cubes.Count; i++)
                {
                    var entry = data.Cubes[i];
                    if (!configProvider.TryGetColor(entry.ColorId, out _))
                    {
                        Debug.LogWarning(
                            $"Save cube at index {i} has unknown ColorId '{entry.ColorId}' and was skipped.");
                        needsResave = true;
                        continue;
                    }

                    var worldX = towerArea.MinX + entry.NormalizedCenterX * areaWidth;
                    worldX = towerArea.ClampBaseCenterX(worldX, worldSize);

                    if (!CanFitAdditionalHeight(buffer, worldSize))
                    {
                        needsResave = true;
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

            return needsResave;
        }

        private float ResolveCubeWorldSize()
        {
            var prefabRect = cubePrefabsConfig.CubePrefab.GetComponent<RectTransform>();
            var parentScale = sceneRefs.TowerRoot.lossyScale.x;
            if (parentScale <= 0f)
            {
                throw new InvalidOperationException("TowerRoot lossyScale.x must be positive.");
            }

            var worldSize = prefabRect.sizeDelta.x * parentScale;
            if (worldSize <= 0f)
            {
                throw new InvalidOperationException("Tower cube prefab world size must be positive.");
            }

            return worldSize;
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
    }
}
