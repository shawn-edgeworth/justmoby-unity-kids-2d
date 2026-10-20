using Assets.Project.Code.Runtime.Configuration;
using UnityEngine;
using Zenject;

namespace Assets.Project.Code.Runtime.Installers
{
    public sealed class VFXPrefabsConfigInstaller : MonoInstaller
    {
        [SerializeField] private VFXPrefabsConfig vfxPrefabsConfig;

        public override void InstallBindings()
        {
            Container.BindInstance(vfxPrefabsConfig).AsSingle();
        }
    }
}
