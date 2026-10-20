using Assets.Project.Code.Runtime.Configuration;
using UnityEngine;
using Zenject;

namespace Assets.Project.Code.Runtime.Installers
{
    [CreateAssetMenu(fileName = nameof(DataInstaller), menuName = "Project/Installers/" + nameof(DataInstaller))]
    public sealed class DataInstaller : ScriptableObjectInstaller<DataInstaller>
    {
        [SerializeField] private GameConfig gameConfig;

        public override void InstallBindings()
        {
            //You can use a configurator based on JSON
            //Container.Bind<IGameConfigProvider>().To<JsonConfigurationProvider>().AsSingle();

            //Or you can use a configurator from a ScriptableObject
            Container.Bind<GameConfig>().FromInstance(gameConfig).AsSingle();
            Container.Bind<IGameConfigProvider>().To<ScriptableObjectConfigProvider>().AsSingle();
        }
    }
}
