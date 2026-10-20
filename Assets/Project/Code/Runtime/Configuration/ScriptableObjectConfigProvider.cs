using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Configuration
{
    public sealed class ScriptableObjectConfigProvider : IGameConfigProvider
    {
        private readonly GameConfig gameConfig;

        public ScriptableObjectConfigProvider(GameConfig gameConfig)
        {
            this.gameConfig = gameConfig;
        }

        public IReadOnlyList<CubeColorConfig> GetPaletteCubes()
        {
            return gameConfig.PaletteCubes;
        }

        public bool TryGetColor(string colorId, out Color color)
        {
            var palette = gameConfig.PaletteCubes;
            for (var i = 0; i < palette.Count; i++)
            {
                if (palette[i].ColorId == colorId)
                {
                    color = palette[i].Color;
                    return true;
                }
            }

            color = default;
            return false;
        }
    }
}
