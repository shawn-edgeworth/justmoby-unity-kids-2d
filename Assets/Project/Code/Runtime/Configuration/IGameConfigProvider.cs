using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Configuration
{
    public interface IGameConfigProvider
    {
        IReadOnlyList<CubeColorConfig> GetPaletteCubes();
        bool TryGetColor(string colorId, out Color color);
    }
}
