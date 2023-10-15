using AYellowpaper.SerializedCollections;
using SimpleMatch3.Board.Data;
using SimpleMatch3.BoardFactory;
using SimpleMatch3.Drop;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.Board.Manager
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private BoardData boardData;

        [SerializedDictionary("Color", "Prefab")]
        public SerializedDictionary<DropColor, GameObject> dropPrefabs;

        private Board _board;
        private IInstantiator _instantiator;
        private SignalBus _signalBus;

        [Inject]
        private void Construct(IInstantiator instantiator, SignalBus signalBus)
        {
            _instantiator = instantiator;
            _signalBus = signalBus;
        }
        
        private void Awake()
        {
            AdjustBoardPosition();
            FitBoardInCamera();
            var tilesParent = CreateTilesParent();
            var dropsParent = CreateDropsParent();

            var strategyData = new BoardCreationStrategyData()
            {
                Instantiator = _instantiator,
                SignalBus = _signalBus,
                DropPrefabs = dropPrefabs,
                DropsParent = dropsParent,
                TilePrefab = tilePrefab,
                TilesParent = tilesParent
            };
            
            IBoardCreationStrategy boardCreationStrategy = new RandomBoardCreationStrategy(strategyData);
            _board = boardCreationStrategy.CreateBoard(boardData);
        }

        private Transform CreateDropsParent()
        {
            var dropsParent = new GameObject("Drops").transform;
            dropsParent.SetParent(transform, false);
            return dropsParent;
        }

        private Transform CreateTilesParent()
        {
            var tilesParent = new GameObject("Tiles").transform;
            tilesParent.SetParent(transform, false);
            return tilesParent;
        }

        private void AdjustBoardPosition()
        {
            var tileBounds = tilePrefab.GetComponent<SpriteRenderer>().bounds;
            transform.position = new Vector3(-(float)boardData.ColumnCount / 2 + tileBounds.extents.x, -(float)boardData.RowCount / 2 + tileBounds.extents.y);
        }

        private void FitBoardInCamera()
        {
            Camera.main.orthographicSize = boardData.ColumnCount;
        }
    }
}