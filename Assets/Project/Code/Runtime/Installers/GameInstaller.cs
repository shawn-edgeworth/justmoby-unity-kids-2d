using Assets.Project.Code.Runtime.Audio;
using Assets.Project.Code.Runtime.Gameplay;
using Assets.Project.Code.Runtime.Infrastructure.Pooling;
using Assets.Project.Code.Runtime.Model;
using Assets.Project.Code.Runtime.Persistence;
using Assets.Project.Code.Runtime.Presentation;
using Zenject;

namespace Assets.Project.Code.Runtime.Installers
{
    public sealed class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PrefabPool>().AsSingle();
            Container.Bind<GameStateMachine>().AsSingle();
            Container.Bind<TowerModel>().AsSingle();
            Container.Bind<TowerLayout>().AsSingle();

            Container.Bind<ICubePlacementRule>().To<HeightLimitPlacementRule>().AsCached();
            //Container.Bind<ICubePlacementRule>().To<MatchingTopColorPlacementRule>().AsCached();
            Container.Bind<PlacementRules>().AsSingle();

            Container.Bind<ISaveService>().To<JsonSaveService>().AsSingle();
            Container.Bind<GameSaveMapper>().AsSingle();
            Container.Bind<AudioService>().AsSingle();

            Container.Bind<PaletteView>().AsSingle();
            Container.Bind<TowerView>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameController>().AsSingle().NonLazy();
        }
    }
}
