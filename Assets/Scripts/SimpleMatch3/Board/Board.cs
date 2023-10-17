﻿using System.Collections.Generic;
using System.Linq;
using SimpleMatch3.Board.Data;
using UnityEngine;

namespace SimpleMatch3.Board
{
    public class Board
    {
        private Dictionary<Vector2Int, Tile.Tile> _tiles = new();
        public readonly BoardCreationData BoardData;

        public Board(BoardCreationData boardData)
        {
            BoardData = boardData;
        }
        
        public void AddTile(Vector2Int coords, Tile.Tile tile)
        {
            if(!_tiles.ContainsKey(coords))
                _tiles.Add(coords, tile);
        }

        public bool TileExists(Vector2Int coords, out Tile.Tile tile)
        {
            return _tiles.TryGetValue(coords, out tile);
        }

        public bool GetUpperTiles(Vector2Int startFrom, out  List<Tile.Tile> upperTiles)
        {
            upperTiles = new List<Tile.Tile>();
            
            if (!TileExists(startFrom, out var startTile))
                return false;
            
            for (var i = 1; i < BoardData.RowCount; i++)
            {
                if(!TileExists(startFrom + Vector2Int.up * i, out var tile))
                    continue;

                upperTiles.Add(tile);
            }

            return true;
        }

        public List<List<Tile.Tile>> GetAllUpperTilesGroupedByColumns(List<Tile.Tile> tiles)
        {
            var allUpperTiles = new List<List<Tile.Tile>>();

            var groupedByColumnIndices = tiles.Select(t => t.Data.Coordinates).GroupBy(coord => coord.x);

            foreach (var group in groupedByColumnIndices)
            {
                var bottomMostCoords = new Vector2Int(group.Key, group.Min(coord => coord.y));

                if(!GetUpperTiles(bottomMostCoords, out var upperTiles))
                    continue;
                
                allUpperTiles.Add(upperTiles);
            }

            return allUpperTiles;
        }
    }
}