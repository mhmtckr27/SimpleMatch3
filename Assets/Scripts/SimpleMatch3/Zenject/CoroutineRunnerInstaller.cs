using SimpleMatch3.Util;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.Zenject
{
    public class CoroutineRunnerInstaller : MonoInstaller
    {
        [SerializeField] private CoroutineRunner coroutineRunner;
        
        public override void InstallBindings()
        {
            Container.BindInstance(coroutineRunner).AsSingle().NonLazy();
        }
    }
}