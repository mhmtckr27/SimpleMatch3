using UnityEngine;

namespace SimpleMatch3.EventInterfaces
{
    public interface ITileExploded
    {
        public class OnExplode
        {
            public Vector2Int Coords;
            public Vector3 Position;
        }
    }
}