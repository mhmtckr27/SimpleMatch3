using System.Collections.Generic;
using SimpleMatch3.Board.Data;
using SimpleMatch3.Drop;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.BoardFactory
{
    public abstract class BoardCreationStrategyBase : IBoardCreationStrategy
    {
        protected readonly BoardCreationStrategyData Data;

        protected BoardCreationStrategyBase(BoardCreationStrategyData data)
        {
            Data = data;
        }
        
        public virtual Board.Board CreateBoard(BoardData boardData)
        {
            var board = Data.Instantiator.Instantiate<Board.Board>(new object[] {boardData, Data.SignalBus});
            
            for (var colIndex = 0; colIndex < boardData.ColumnCount; colIndex++)
            {
                for (var rowIndex = 0; rowIndex < boardData.RowCount; rowIndex++)
                {
                    var coords = new Vector2Int(colIndex, rowIndex);
                    
                    if(boardData.TilesToSkip.Contains(coords))
                        continue;
                    
                    var isGeneratorTile = boardData.GeneratorTiles.Contains(coords);
                    var tile = CreateTile(boardData, new TileData(coords, isGeneratorTile));
                    var drop = CreateDrop(board, boardData, tile);
                    drop.transform.position = tile.transform.position;
                    tile.SetDrop(drop);
                    board.AddTile(coords, tile);
                }
            }

            return board;
        }

        private Tile.Tile CreateTile(BoardData boardData, TileData tileData)
        {
            var tile = Data.Instantiator.InstantiatePrefab(Data.TilePrefab, Data.TilesParent).GetComponent<Tile.Tile>();
            tile.name = $"Tile_{tileData.Coordinates}";
            tile.transform.localPosition = new Vector3(tileData.Coordinates.x, tileData.Coordinates.y);
            tile.Data = tileData;
            return tile;
        }

        protected abstract Drop.Drop CreateDrop(Board.Board board, BoardData boardData, Tile.Tile tile);
    }
    
    public class BoardCreationStrategyData
    {
        public IInstantiator Instantiator;
        public SignalBus SignalBus;
        public GameObject TilePrefab;
        public Transform TilesParent;
        public Transform DropsParent;
        public Dictionary<DropColor, GameObject> DropPrefabs;
    }
}