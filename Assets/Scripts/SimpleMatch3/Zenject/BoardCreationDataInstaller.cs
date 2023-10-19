using SimpleMatch3.Board.Data;
using SimpleMatch3.Board.Manager;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.Zenject
{
    public class BoardCreationDataInstaller : MonoInstaller
    {
        [SerializeField] private BoardManager boardManager;
        
        public override void InstallBindings()
        {
            Container.Bind<BoardCreationData>().FromInstance(boardManager.BoardCreationData).AsSingle().NonLazy();
        }
    }
}