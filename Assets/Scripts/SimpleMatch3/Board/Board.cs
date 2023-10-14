using System;
using System.Collections.Generic;
using SimpleMatch3.Board.Data;
using UnityEngine;

namespace SimpleMatch3.Board
{
    [Serializable]
    public class Board
    {
        [field: SerializeField] public BoardData BoardData { get; private set; }
        private Dictionary<Vector2Int, Tile.Tile> _tiles = new();

        public Board(BoardData boardData)
        {
            BoardData = boardData;
        }
        
        public void AddTile(Vector2Int coords, Tile.Tile tile)
        {
            if(_tiles.ContainsKey(coords))
                return;
            
            _tiles.Add(coords, tile);
        }

        public bool TileExists(Vector2Int coords, out Tile.Tile tile)
        {
            return _tiles.TryGetValue(coords, out tile);
        }
    }
}