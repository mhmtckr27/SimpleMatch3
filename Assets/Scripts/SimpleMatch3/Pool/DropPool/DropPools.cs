using System;
using System.Collections.Generic;
using SimpleMatch3.Board.Data;
using SimpleMatch3.Drop;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

namespace SimpleMatch3.Pool
{
    public class DropPools : MonoBehaviour
    {
        private Dictionary<Drop.DropColor, ObjectPool<Drop.Drop>> _dropPools;
        private IInstantiator _instantiator;
        private BoardCreationData _boardCreationData;

        [Inject]
        private void Construct(IInstantiator instantiator, BoardCreationData boardCreationData)
        {
            _instantiator = instantiator;
            _boardCreationData = boardCreationData;
            _dropPools = new Dictionary<DropColor, ObjectPool<Drop.Drop>>();

            var enumValues = Enum.GetValues(typeof(DropColor));
            var defaultCapacity = boardCreationData.rowCount * boardCreationData.columnCount / enumValues.Length;
            
            foreach (DropColor dropColor in enumValues)
            {
                if(dropColor == DropColor.Blank)
                    continue;

                _dropPools.Add(dropColor, new ObjectPool<Drop.Drop>(() => OnCreateItem(dropColor), OnGetItem, OnReleaseItem,
                    OnDestroyItem,
                    defaultCapacity: defaultCapacity));
            }
        }

        private void OnDestroyItem(Drop.Drop drop)
        {
            Destroy(drop.gameObject);
        }

        private void OnReleaseItem(Drop.Drop drop)
        {
            drop.gameObject.SetActive(false);
            drop.SetFalling(false);
        }

        private void OnGetItem(Drop.Drop drop)
        {
            drop.gameObject.SetActive(true);
        }

        private Drop.Drop OnCreateItem(DropColor dropColor)
        {
            if (!_boardCreationData.dropPrefabs.TryGetValue(dropColor, out var prefab))
                return null;

            var drop = _instantiator.InstantiatePrefab(prefab, _boardCreationData.dropsParent).GetComponent<Drop.Drop>();
            drop.name = $"Drop_{dropColor}";
            drop.SetParentPool(_dropPools[dropColor]);
            return drop;
        }

        public ObjectPool<Drop.Drop> GetPool(DropColor dropColor)
        {
            _dropPools.TryGetValue(dropColor, out var pool);
            return pool;
        }
    }
}