using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Plugins.Core.PathCore;
using SimpleMatch3.EventInterfaces;
using SimpleMatch3.Extensions;
using SimpleMatch3.Util;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.Gravity
{
    public class GravityProcessor : IDisposable
    {
        private readonly CoroutineRunner _coroutineRunner;
        private readonly Board.Board _board;
        private readonly SignalBus _signalBus;

        public GravityProcessor(Board.Board board, CoroutineRunner coroutineRunner, SignalBus signalBus)
        {
            _board = board;
            _coroutineRunner = coroutineRunner;
            _signalBus = signalBus;
            
            // _signalBus.Subscribe<IProcessGravity.ProcessGravityForDrops>(OnProcessGravityForDrops);
        }
        
        public void Dispose()
        {
            // _signalBus.Unsubscribe<IProcessGravity.ProcessGravityForDrops>(OnProcessGravityForDrops);
        }
        
        // private void OnProcessGravityForDrops(IProcessGravity.ProcessGravityForDrops data)
        // {
        //     if(!_board.TileExists(, out var tile))
        //         return;
        //
        //     _coroutineRunner.StartCoroutine(ProcessGravityFor(tile));
        // }

        public async Task ProcessGravityFor(List<Tile.Tile> tiles)
        {
            var gravityTasks = new List<Task>();
            foreach (var tile in tiles)
            {
                gravityTasks.Add(ProcessGravityFor(tile));
                await Task.Delay(30);
                
                // if (tile.Data.IsGeneratorTile)
                // {
                    // Debug.LogError(tiles.Count);
                    // for (int i = 0; i < tiles.Count - 1; i++)
                    // {
                        // GenerateDropAndProcessGravity(tile);
                        // yield return new WaitForSeconds(0.03f);
                    // }
                // }
            }

            foreach (var gravityTask in gravityTasks)
            {
                await gravityTask;
            }
            return;
        }

        // private void GenerateDropAndProcessGravity(Tile.Tile tile)
        // {
        //     var drop = tile.Data.Generator.GenerateDrop(tile.transform.position + Vector3.up * Tile.Tile.TileSize);
        //     
        //     if(drop == null)
        //         return;
        //
        //     _coroutineRunner.StartCoroutine(DropContinuously(drop, tile));
        //     
        //     return;
        // }

        public async Task ProcessGravityFor(Tile.Tile tile)
        {
            if(tile.IsEmpty())
                return;            
            
            if(!_board.TileExists(tile.Data.Coordinates + Vector2Int.down, out var nextTile) || !nextTile.IsEmpty())
                return;       
            
            var drop = tile.SetDrop(null);
            
            if(drop == null)
                return;
            
            await DropContinuously(drop, nextTile);
        }

        private async Task DropContinuously(Drop.Drop drop, Tile.Tile nextTile)
        {
            nextTile.SetDrop(drop);
            nextTile.SetBusy(true);
            var targetPosition = nextTile.transform.position;
            drop.ResetSpeed();
            drop.SetFalling(true);
            Tile.Tile tile = null;
            while (true)
            {
                await drop.DropTo(targetPosition);
                
                tile = nextTile;

                if (_board.TileExists(tile.Data.Coordinates + Vector2Int.down, out nextTile) && nextTile.IsEmpty())
                {
                    tile.SetDrop(null);
                    tile.SetBusy(false);
                    nextTile.SetDrop(drop);
                    nextTile.SetBusy(true);
                    targetPosition = nextTile.transform.position;
                    continue;
                }

                drop.transform.position = targetPosition;

                tile.SetBusy(false);
                await drop.SquashAndStretch();
                
                return;
            }
        }
    }
}