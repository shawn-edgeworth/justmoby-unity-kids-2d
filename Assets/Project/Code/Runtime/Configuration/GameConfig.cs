using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Configuration
{
    [CreateAssetMenu(fileName = nameof(GameConfig), menuName = "Project/" + nameof(GameConfig))]
    public sealed class GameConfig : ScriptableObject
    {
        [SerializeField] private List<CubeColorConfig> paletteCubes = new();

        public IReadOnlyList<CubeColorConfig> PaletteCubes => paletteCubes;
    }
}
