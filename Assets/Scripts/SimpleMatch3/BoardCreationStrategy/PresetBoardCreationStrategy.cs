using SimpleMatch3.Board.Data;
using SimpleMatch3.BoardFactory;
using SimpleMatch3.Drop;
using SimpleMatch3.Util;
using UnityEngine;

namespace SimpleMatch3.BoardCreationStrategy
{
    public class PresetBoardCreationStrategy : BoardCreationStrategyBase
    {
        public PresetBoardCreationStrategy(BoardCreationStrategyData data) : base(data)
        {
        }
        
        protected override Drop.Drop CreateDrop(Board.Board board, BoardData boardData, Tile.Tile tile)
        {
            var foundColor = boardData.BoardColors.TryGetValue(tile.Data.Coordinates, out var color);

            if (!foundColor)
            {
                Debug.LogError(
                    $"Could not find drop color from board data with given coordinates : {tile.Data.Coordinates}, setting to a random color!");
                color = Helpers.RandomEnum<DropColor>();
            }
            
            var foundPrefab = Data.DropPrefabs.TryGetValue(color, out var prefab);

            if (foundPrefab)
            {
                var drop = Data.Instantiator.InstantiatePrefab(prefab, Data.DropsParent).GetComponent<Drop.Drop>();
                drop.name = $"Drop_{color}";
                return drop;
            }            
            Debug.LogError($"Could not find drop prefab with given color: {color}");
            return null;
        }
    }
}