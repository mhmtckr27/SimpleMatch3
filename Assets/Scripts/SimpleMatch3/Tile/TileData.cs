using UnityEngine;

namespace SimpleMatch3
{
    public class TileData
    {
        /// <summary>
        /// x => columnIndex, y => rowIndex
        /// </summary>
        public Vector2Int Coordinates;
        public bool IsGeneratorTile;
        public Drop.Drop CurrentDrop;

        public TileData(Vector2Int coordinates, bool isGeneratorTile)
        {
            Coordinates = coordinates;
            IsGeneratorTile = isGeneratorTile;
        }
    }
}