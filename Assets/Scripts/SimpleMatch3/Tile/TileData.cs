using System;
using UnityEngine;

namespace SimpleMatch3.Tile
{
    [Serializable]
    public class TileData
    {
        /// <summary>
        /// x => columnIndex, y => rowIndex
        /// </summary>
        public Vector2Int Coordinates;
        public bool IsGeneratorTile;
        public Generator.Generator Generator;
        public Drop.Drop CurrentDrop;

        public TileData(Vector2Int coordinates, bool isGeneratorTile, Generator.Generator generator)
        {
            Coordinates = coordinates;
            IsGeneratorTile = isGeneratorTile;
            Generator = generator;
        }
    }
}