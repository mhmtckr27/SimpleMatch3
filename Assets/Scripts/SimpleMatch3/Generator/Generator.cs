using System;
using System.Collections.Generic;
using SimpleMatch3.Drop;
using SimpleMatch3.EventInterfaces;
using SimpleMatch3.Util;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.Generator
{
    public class Generator : IDisposable
    {
        public readonly GeneratorData Data;
        private readonly CoroutineRunner _coroutineRunner;

        public Generator(GeneratorData data, CoroutineRunner coroutineRunner)
        {
            Data = data;
            _coroutineRunner = coroutineRunner;
            
            // _data.SignalBus.SubscribeId<ITileExploded.OnExplode>(_data.Coordinates, OnTilesExploded);
        }

        public void Dispose()
        {
            // _data.SignalBus.UnsubscribeId<ITileExploded.OnExplode>(_data.Coordinates, OnTilesExploded);
        }
        
        // private void OnTilesExploded(ITileExploded.OnExplode data)
        // {
        //     GenerateDrop(data.Position);
        // }

        // public void Activate()
        // {
        //     CheckTile();
        // }

        // private async void CheckTile()
        // {
        //     while (Application.isPlaying)
        //     {
        //         if (_tile.IsEmpty())
        //         {
        //             _coroutineRunner.StartCoroutine(Generate());
        //         }
        //
        //         await Task.Delay(20);
        //     }
        // }

        // private IEnumerator Generate()
        // {
        //     var rand = Helpers.RandomEnum(new List<DropColor>
        //     {
        //         DropColor.Blank
        //     });
        //
        //     if (!_data.DropPrefabs.TryGetValue(rand, out var prefab))
        //         yield break;
        //
        //     var drop = _data.Instantiator.InstantiatePrefab(prefab,
        //             _tile.transform.position + Vector3.up * Tile.Tile.TileSize, Quaternion.identity, _data.DropsParent)
        //         .GetComponent<Drop.Drop>();
        //
        //     drop.SetFalling(true);
        //     _tile.SetDrop(drop);
        //     yield return _coroutineRunner.StartCoroutine(drop.DropTo(_tile.transform.position));
        //     
        //     if (_board.TileExists(_tile.Data.Coordinates + Vector2Int.down, out var downTile) && downTile.IsEmpty())
        //     {
        //         _data.SignalBus.Fire(new IProcessGravity.ProcessGravityFor()
        //         {
        //             TileCoords = _tile.Data.Coordinates
        //         });
        //     }
        //     else
        //     {
        //         _coroutineRunner.StartCoroutine(drop.SquashAndStretch());
        //     }        
        // }

        public Drop.Drop GenerateDrop(Vector3 position)
        {
            var rand = Helpers.RandomEnum(new List<DropColor>
            {
                DropColor.Blank
            });

            if (!Data.DropPrefabs.TryGetValue(rand, out var prefab))
                return null;

            return Data.Instantiator.InstantiatePrefab(prefab,
                    position, Quaternion.identity, Data.DropsParent)
                .GetComponent<Drop.Drop>();
        }
    }

    public class GeneratorData
    {
        public IInstantiator Instantiator;
        public Dictionary<DropColor, GameObject> DropPrefabs;
        public Transform DropsParent;
        public int ColumnIndex;
        public Vector2Int Coords;
        public Vector3 Position;
    }
}