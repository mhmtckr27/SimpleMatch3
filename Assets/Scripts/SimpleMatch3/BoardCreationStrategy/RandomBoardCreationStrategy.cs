using System.Collections.Generic;
using SimpleMatch3.Board.Data;
using SimpleMatch3.Drop;
using SimpleMatch3.Extensions;
using SimpleMatch3.Util;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.BoardFactory
{
    public class RandomBoardCreationStrategy : BoardCreationStrategyBase
    {
        public RandomBoardCreationStrategy(IInstantiator instantiator, GameObject tilePrefab, Transform boardManager,
            Dictionary<DropColor, GameObject> dropPrefabs) : base(instantiator, tilePrefab, boardManager, dropPrefabs)
        {
        }

        protected override Drop.Drop CreateDrop(Board.Board board, BoardData boardData, Tile.Tile tile)
        {
            //We must only check left and down tiles because we start spawning from bottom left.
            var directions = new List<Vector2Int>()
            {
                Vector2Int.left,
                Vector2Int.down
            };
            
            var exclusionList = GetDropColorExclusionList(directions, board, tile);

            var color = Helpers.RandomEnum(exclusionList);
            var foundPrefab = DropPrefabs.TryGetValue(color, out var prefab);

            if (foundPrefab) 
                return instantiator.InstantiatePrefab(prefab).GetComponent<Drop.Drop>();
            
            Debug.LogError($"Could not find drop prefab with given color: {color}");
            return null;
        }

        private List<DropColor> GetDropColorExclusionList(List<Vector2Int> directions, Board.Board board, Tile.Tile currentTile)
        {
            var excludeList = new List<DropColor>() { DropColor.Blank };
            
            //Checks for each direction, next and 2 next tiles to see if they are same color.
            //If they are same, we cannot spawn the same color drop on current tile.
            //We remove that color from random color pool.
            foreach (var direction in directions)
            {
                if(!board.TileExists(currentTile.Data.Coordinates + direction, out var nextTile))
                    continue;

                var shouldExcludeColor = true;
                var color = nextTile.Data.CurrentDrop.Color;
                
                //We already checked the adjacent tile so we start from 2
                for (var i = 2; i < board.BoardData.MinimumMatchAmount; i++)
                {
                    if (board.TileExists(currentTile.Data.Coordinates + direction * i, out nextTile) &&
                        nextTile.Data.CurrentDrop.Color == color) 
                    {
                        continue;
                    }
                    
                    shouldExcludeColor = false;
                    break;
                }
                
                if(shouldExcludeColor)
                    excludeList.AddUnique(color);
            }

            return excludeList;
        }
    }
}