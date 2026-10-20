using Assets.Project.Code.Runtime.Configuration;
using UnityEngine;
using Zenject;

namespace Assets.Project.Code.Runtime.Installers
{
    public sealed class GameAnimationConfigInstaller : MonoInstaller
    {
        [SerializeField] private GameAnimationConfig gameAnimationConfig;

        public override void InstallBindings()
        {
            Container.BindInstance(gameAnimationConfig).AsSingle();
        }
    }
}
