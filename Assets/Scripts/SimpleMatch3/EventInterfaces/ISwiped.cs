using UnityEngine;

namespace SimpleMatch3.EventInterfaces
{
    public interface ISwiped
    {
        public class OnSwiped
        {
            public Vector2Int InputDownTileCoords;
            public Vector2Int SwipeDirection;
        }
    }
}