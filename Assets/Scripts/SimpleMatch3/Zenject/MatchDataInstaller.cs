using SimpleMatch3.Matching.Data;
using SimpleMatch3.Matching.Matches;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.Zenject
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Installers/MatchDataInstaller", fileName = "MatchDataInstaller")]
    public class MatchDataInstaller : ScriptableObjectInstaller<MatchDataInstaller>
    {
        [SerializeField] private MatchData singleLineMatch3Data;
        [SerializeField] private MatchData singleLineMatch4Data;
        [SerializeField] private MatchData singleLineMatch5Data;
        
        public override void InstallBindings()
        {
            Container.Bind<MatchData>().FromInstance(singleLineMatch3Data).WhenInjectedInto<SingleLineMatch3>();
            Container.Bind<MatchData>().FromInstance(singleLineMatch4Data).WhenInjectedInto<SingleLineMatch4>();
            Container.Bind<MatchData>().FromInstance(singleLineMatch5Data).WhenInjectedInto<SingleLineMatch5>();
        }
    }
}