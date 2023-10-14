using System.Collections.Generic;
using SimpleMatch3.Board.Data;
using SimpleMatch3.Drop;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.BoardFactory
{
    public abstract class BoardCreationStrategyBase : IBoardCreationStrategy
    {
        private readonly GameObject _tilePrefab;
        protected readonly Dictionary<DropColor, GameObject> DropPrefabs;
        protected readonly IInstantiator instantiator;
        private readonly Transform _boardManager;

        public BoardCreationStrategyBase(IInstantiator instantiator, GameObject tilePrefab, Transform boardManager,
            Dictionary<DropColor, GameObject> dropPrefabs)
        {
            this.instantiator = instantiator;
            _tilePrefab = tilePrefab;
            _boardManager = boardManager;
            DropPrefabs = dropPrefabs;
        }
        
        public virtual Board.Board CreateBoard(BoardData boardData)
        {
            var board = new Board.Board(boardData);
            
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
            var tile = instantiator.InstantiatePrefab(_tilePrefab, _boardManager).GetComponent<Tile.Tile>();
            tile.transform.localPosition = new Vector3(tileData.Coordinates.x, tileData.Coordinates.y);
            tile.Data = tileData;
            return tile;
        }

        protected abstract Drop.Drop CreateDrop(Board.Board board, BoardData boardData, Tile.Tile tile);
    }
}