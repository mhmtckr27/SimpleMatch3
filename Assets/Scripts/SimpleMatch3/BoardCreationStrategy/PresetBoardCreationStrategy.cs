using System.Collections.Generic;
using SimpleMatch3.Board.Data;
using SimpleMatch3.Drop;
using SimpleMatch3.Util;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.BoardFactory
{
    public class PresetBoardCreationStrategy : BoardCreationStrategyBase
    {
        protected override Drop.Drop CreateDrop(Board.Board board, BoardData boardData, Tile.Tile tile)
        {
            var foundColor = boardData.BoardColors.TryGetValue(tile.Data.Coordinates, out var color);

            if (!foundColor)
            {
                Debug.LogError(
                    $"Could not find drop color from board data with given coordinates : {tile.Data.Coordinates}, setting to a random color!");
                color = Helpers.RandomEnum<DropColor>();
            }
            
            var foundPrefab = DropPrefabs.TryGetValue(color, out var prefab);

            if (foundPrefab) 
                return instantiator.InstantiatePrefab(prefab).GetComponent<Drop.Drop>();
            
            Debug.LogError($"Could not find drop prefab with given color: {color}");
            return null;
        }

        public PresetBoardCreationStrategy(IInstantiator instantiator, GameObject tilePrefab, Transform boardManager,
            Dictionary<DropColor, GameObject> dropPrefabs) : base(instantiator, tilePrefab, boardManager, dropPrefabs)
        {
        }
    }
}