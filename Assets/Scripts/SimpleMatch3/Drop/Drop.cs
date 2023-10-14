using UnityEngine;

namespace SimpleMatch3.Drop
{
    public class Drop : MonoBehaviour
    {
        public DropColor Color;

        public Drop(DropColor color)
        {
            Color = color;
        }
    }
}