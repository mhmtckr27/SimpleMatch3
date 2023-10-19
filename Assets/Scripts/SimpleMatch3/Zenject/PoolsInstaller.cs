using SimpleMatch3.Pool;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.Zenject
{
    public class PoolsInstaller : MonoInstaller
    {
        [SerializeField] private DropPools dropPools;
        [SerializeField] private VFXPools vfxPools;
        
        public override void InstallBindings()
        {
            Container.BindInstance(dropPools).AsSingle().NonLazy();
            Container.BindInstance(vfxPools).AsSingle().NonLazy();
        }
    }
}