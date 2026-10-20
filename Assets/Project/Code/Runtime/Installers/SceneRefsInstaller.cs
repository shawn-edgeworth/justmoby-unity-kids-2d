using Assets.Project.Code.Runtime.Presentation;
using UnityEngine;
using Zenject;

namespace Assets.Project.Code.Runtime.Installers
{
    public sealed class SceneRefsInstaller : MonoInstaller
    {
        [SerializeField] private SceneRefs sceneRefs;

        public override void InstallBindings()
        {
            Container.BindInstance(sceneRefs).AsSingle();
            Container.BindInstance(sceneRefs.PaletteScrollGestureGate).AsSingle();
            Container.QueueForInject(sceneRefs.MessageView);
            Container.Bind<ITowerArea>().FromInstance(sceneRefs.TowerArea).AsSingle();
            Container.Bind<IHoleArea>().FromInstance(sceneRefs.Hole).AsSingle();
        }
    }
}
