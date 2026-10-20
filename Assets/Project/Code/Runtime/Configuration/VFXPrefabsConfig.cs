using Assets.Project.Code.Runtime.Presentation;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Configuration
{
    [CreateAssetMenu(fileName = nameof(VFXPrefabsConfig), menuName = "Project/" + nameof(VFXPrefabsConfig))]
    public sealed class VFXPrefabsConfig : ScriptableObject
    {
        [field: SerializeField] public VfxEffect DestroySmoke { get; private set; }
    }
}
