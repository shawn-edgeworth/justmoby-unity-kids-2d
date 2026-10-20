using Assets.Project.Code.Runtime.Presentation;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Configuration
{
    [CreateAssetMenu(fileName = nameof(CubePrefabsConfig), menuName = "Project/" + nameof(CubePrefabsConfig))]
    public sealed class CubePrefabsConfig : ScriptableObject
    {
        [field: SerializeField] public CubeView PaletteCubePrefab { get; private set; }
        [field: SerializeField] public CubeView TowerCubePrefab { get; private set; }
        [field: SerializeField] public CubeView GhostCubePrefab { get; private set; }
    }
}
