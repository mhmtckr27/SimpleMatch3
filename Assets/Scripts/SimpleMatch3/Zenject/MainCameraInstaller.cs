using UnityEngine;
using Zenject;

namespace SimpleMatch3.Zenject
{
    public class MainCameraInstaller : MonoInstaller
    {
        [SerializeField] private Camera mainCamera;
        
        public override void InstallBindings()
        {
            Container.BindInstance(mainCamera).AsSingle().NonLazy();
        }
    }
}