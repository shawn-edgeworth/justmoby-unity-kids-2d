using Assets.Project.Code.Runtime.Configuration;
using UnityEngine;
using Zenject;

namespace Assets.Project.Code.Runtime.Installers
{
    public sealed class GameMessagesConfigInstaller : MonoInstaller
    {
        [SerializeField] private GameMessagesConfig gameMessagesConfig;

        public override void InstallBindings()
        {
            Container.BindInstance(gameMessagesConfig).AsSingle();
        }
    }
}
