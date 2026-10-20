using Assets.Project.Code.Runtime.Configuration;
using UnityEngine;
using Zenject;

namespace Assets.Project.Code.Runtime.Installers
{
    public sealed class CubePrefabsConfigInstaller : MonoInstaller
    {
        [SerializeField] private CubePrefabsConfig cubePrefabsConfig;

        public override void InstallBindings()
        {
            Container.BindInstance(cubePrefabsConfig).AsSingle();
        }
    }
}
