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

        [Inject]
        private void Construct(IInstantiator instantiator)
        {
            _instantiator = instantiator;
        }
        
        private void Awake()
        {
            AdjustBoardPosition();
            FitBoardInCamera();
            
            IBoardCreationStrategy boardCreationStrategy = new RandomBoardCreationStrategy(_instantiator, tilePrefab, transform, dropPrefabs);
            _board = boardCreationStrategy.CreateBoard(boardData);
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