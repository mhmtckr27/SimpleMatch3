using System;
using System.Collections.Generic;
using SimpleMatch3.Board.Data;
using SimpleMatch3.EventInterfaces;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.Board
{
    [Serializable]
    public class Board
    {
        [field: SerializeField] public BoardData BoardData { get; private set; }
        private Dictionary<Vector2Int, Tile.Tile> _tiles = new();
        
        private readonly SignalBus _signalBus;

        public Board(BoardData boardData, SignalBus signalBus)
        {
            BoardData = boardData;
            _signalBus = signalBus;
            
            _signalBus.Subscribe<ISwiped.OnSwiped>(OnSwiped);
        }

        private void OnSwiped(ISwiped.OnSwiped data)
        {
            if(!TileExists(data.InputDownTileCoords, out var swipedTile))
                return;
            
            if(swipedTile.IsBusy)
                return;
            
            if(!TileExists(data.InputDownTileCoords + data.SwipeDirection, out var tile))
            {
                swipedTile.PlaySwipeNotAllowedAnim(data.SwipeDirection);
                return;
            }        
            
            if(tile.IsBusy)
                return;
            
            swipedTile.PlaySwipeWithoutExplosionAnim(data.SwipeDirection, tile);
            tile.PlaySwipeWithoutExplosionAnim(-data.SwipeDirection, swipedTile);
            
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