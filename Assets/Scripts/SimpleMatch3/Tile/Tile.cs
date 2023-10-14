using UnityEngine;

namespace SimpleMatch3.Tile
{
    public class Tile : MonoBehaviour
    {
        public TileData Data;

        public Drop.Drop SetDrop(Drop.Drop newDrop)
        {
            var oldDrop = Data.CurrentDrop;
            Data.CurrentDrop = newDrop;
            return oldDrop;
        }
    }
}