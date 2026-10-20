using Assets.Project.Code.Runtime.Configuration;
using UnityEngine;
using Zenject;

namespace Assets.Project.Code.Runtime.Installers
{
    public sealed class GameAudioConfigInstaller : MonoInstaller
    {
        [SerializeField] private GameAudioConfig gameAudioConfig;

        public override void InstallBindings()
        {
            Container.BindInstance(gameAudioConfig).AsSingle();
        }
    }
}
