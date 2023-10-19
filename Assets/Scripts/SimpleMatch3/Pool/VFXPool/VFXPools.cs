using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using SimpleMatch3.Board.Data;
using SimpleMatch3.Drop;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

namespace SimpleMatch3.Pool
{
    public class VFXPools : MonoBehaviour
    {
        [SerializedDictionary("Color", "VFX")] 
        public SerializedDictionary<DropColor, GameObject> VFXPrefabs;
        private Dictionary<Drop.DropColor, ObjectPool<PooledVFX>> _vfxPools;
        private IInstantiator _instantiator;
        private BoardCreationData _boardCreationData;

        [Inject]
        private void Construct(IInstantiator instantiator, BoardCreationData boardCreationData)
        {
            _instantiator = instantiator;
            _boardCreationData = boardCreationData;
            _vfxPools = new Dictionary<DropColor, ObjectPool<PooledVFX>>();

            var enumValues = Enum.GetValues(typeof(DropColor));
            var defaultCapacity = 10;
            
            foreach (DropColor dropColor in enumValues)
            {
                if(dropColor == DropColor.Blank)
                    continue;

                _vfxPools.Add(dropColor, new ObjectPool<PooledVFX>(() => OnCreateItem(dropColor), OnGetItem, OnReleaseItem,
                    OnDestroyItem,
                    defaultCapacity: defaultCapacity));
            }
        }

        private void OnDestroyItem(PooledVFX vfx)
        {
            Destroy(vfx.gameObject);
        }

        private void OnReleaseItem(PooledVFX vfx)
        {
            vfx.gameObject.SetActive(false);
        }

        private void OnGetItem(PooledVFX vfx)
        {
            vfx.gameObject.SetActive(true);
        }

        private PooledVFX OnCreateItem(DropColor dropColor)
        {
            if (!VFXPrefabs.TryGetValue(dropColor, out var prefab))
                return null;

            var vfx = _instantiator.InstantiatePrefab(prefab, transform).GetComponent<PooledVFX>();
            vfx.name = $"VFX_{dropColor}";
            vfx.SetParentPool(_vfxPools[dropColor]);
            return vfx;
        }

        public ObjectPool<PooledVFX> GetPool(DropColor dropColor)
        {
            _vfxPools.TryGetValue(dropColor, out var pool);
            return pool;
        }
    }
}