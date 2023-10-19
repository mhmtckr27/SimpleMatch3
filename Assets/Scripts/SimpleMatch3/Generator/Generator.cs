using System;
using System.Collections.Generic;
using SimpleMatch3.Drop;
using SimpleMatch3.EventInterfaces;
using SimpleMatch3.Pool;
using SimpleMatch3.Util;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.Generator
{
    public class Generator
    {
        public readonly GeneratorData Data;
        private readonly CoroutineRunner _coroutineRunner;

        public Generator(GeneratorData data, CoroutineRunner coroutineRunner)
        {
            Data = data;
            _coroutineRunner = coroutineRunner;
        }

        public Drop.Drop GenerateDrop(Vector3 position)
        {
            var rand = Helpers.RandomEnum(new List<DropColor>
            {
                DropColor.Blank
            });

            var drop = Data.DropPools.GetPool(rand)?.Get();
            
            if (drop == null)
                return null;
            
            drop.transform.position = position;
            return drop;
        }
    }

    public class GeneratorData
    {
        public IInstantiator Instantiator;
        public DropPools DropPools;
        public Transform DropsParent;
        public int ColumnIndex;
        public Vector2Int Coords;
        public Vector3 Position;
    }
}