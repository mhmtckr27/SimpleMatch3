using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleMatch3.Util;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.Gravity
{
    public class GravityProcessor
    {
        private readonly CoroutineRunner _coroutineRunner;
        private readonly Board.Board _board;
        private readonly SignalBus _signalBus;

        public GravityProcessor(Board.Board board, CoroutineRunner coroutineRunner, SignalBus signalBus)
        {
            _board = board;
            _coroutineRunner = coroutineRunner;
            _signalBus = signalBus;
        }

        public IEnumerator ProcessGravityForDrops(List<Drop.Drop> drops)
        {
            foreach (var drop in drops)
            {
                if(!_board.GetNextAvailableLowerTile(drop.CurrentTileCoords, out var tileToDrop))
                    continue;

                if (_board.TileExists(drop.CurrentTileCoords, out var currentTile))
                    currentTile.SetDrop(null);
                
                _coroutineRunner.StartCoroutine(DropToTile(drop, tileToDrop));
                yield return new WaitForSeconds(0.03f);
            }
        }

        private bool GetTileToDrop(Vector2Int coords, out Tile.Tile tile)
        {
            //we must get current tile for topmost tile.
            _board.TileExists(coords, out tile);
            
            while (_board.TileExists(coords + Vector2Int.down, out var tileCache) && tileCache.IsEmpty())
            {
                coords += Vector2Int.down;
                tile = tileCache;
            }

            return tile != null;
        }

        private IEnumerator DropToTile(Drop.Drop drop, Tile.Tile nextTile)
        {
            nextTile.SetDrop(drop);
            nextTile.SetBusy(true);
            
            var targetPosition = nextTile.transform.position;
            
            drop.ResetSpeed();
            drop.SetFalling(true);

            while (true)
            {
                yield return _coroutineRunner.StartCoroutine(drop.DropTo(targetPosition));
                if (drop.IsExploded)
                {
                    yield break;
                }
                
                drop.transform.position = targetPosition;

                yield return _coroutineRunner.StartCoroutine(drop.SquashAndStretch());
                yield return new WaitForSeconds(0.05f);
                nextTile.SetBusy(false);
                yield break;
            }
        }
        
        public bool AreDropsStable(List<Drop.Drop> drops)
        {
            return drops == null || drops.All(d => !d.IsFalling);
        }
    }
}