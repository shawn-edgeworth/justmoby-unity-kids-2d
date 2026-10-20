using Assets.Project.Code.Runtime.Configuration;
using UnityEngine;
using Zenject;

namespace Assets.Project.Code.Runtime.Installers
{
    [CreateAssetMenu(fileName = nameof(DataInstaller), menuName = "Project/Installers/" + nameof(DataInstaller))]
    public sealed class DataInstaller : ScriptableObjectInstaller<DataInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IGameConfigProvider>().To<JsonConfigurationProvider>().AsSingle();
        }
    }
}
