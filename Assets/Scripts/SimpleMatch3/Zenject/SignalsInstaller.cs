using SimpleMatch3.EventInterfaces;
using Zenject;

namespace SimpleMatch3.Zenject
{
    public class SignalsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<ISwiped.OnSwiped>();
        }
    }
}