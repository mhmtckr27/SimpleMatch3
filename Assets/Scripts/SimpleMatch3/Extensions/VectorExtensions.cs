using UnityEngine;

namespace SimpleMatch3.Extensions
{
    public static class VectorExtensions
    {
        public static Vector2 Abs(this Vector2 vector2)
        {
            return new Vector2(Mathf.Abs(vector2.x), Mathf.Abs(vector2.y));
        }
        
        public static Vector3 Abs(this Vector3 vector3)
        {
            return new Vector3(Mathf.Abs(vector3.x), Mathf.Abs(vector3.y), Mathf.Abs(vector3.z));
        }

        public static Vector2Int ToVec2Int(this Vector2 vector2)
        {
            return new Vector2Int((int) vector2.x, (int) vector2.y);
        }

        /// <summary>
        /// Ceils positive numbers and floors negative numbers. e.g. 0.35 becomes 1 and -0.35 becomes -1 etc.
        /// </summary>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static Vector2Int CeilToVec2Int(this Vector2 vector2)
        {
            return new Vector2Int((int) Mathf.Sign(vector2.x) * Mathf.CeilToInt(Mathf.Abs(vector2.x)),
                ((int)Mathf.Sign(vector2.y)) * Mathf.CeilToInt(Mathf.Abs(vector2.y)));
        }

        public static Vector3 ToVec3(this Vector2Int vector2Int)
        {
            return new Vector3(vector2Int.x, vector2Int.y);
        }

        /// <summary>
        /// Chooses the bigger (in absolute) axis and returns a single axis vector2.
        /// </summary>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static Vector2 SelectBiggerAxis(this Vector2 vector2)
        {
            var vector2Abs = vector2.Abs();
            return vector2Abs.x > vector2Abs.y ? new Vector2(vector2.x, 0) : new Vector2(0, vector2.y);
        }

        /// <summary>
        /// Chooses the bigger (in absolute) axis and returns a single axis vector3.
        /// </summary>
        /// <param name="vector3"></param>
        /// <returns></returns>
        public static Vector2 SelectBiggerAxis(this Vector3 vector3)
        {
            var vector3Abs = vector3.Abs();
            return vector3Abs.x > vector3Abs.y ? new Vector2(vector3.x, 0) : new Vector2(0, vector3.y);
        }

        /// <summary>
        /// Returns the sign of given Vector2Int assuming one axis is 0.
        /// </summary>
        /// <param name="vector2Int"></param>
        /// <returns></returns>
        public static float Sign(this Vector2Int vector2Int)
        {
            return vector2Int.x != 0 ? Mathf.Sign(vector2Int.x) : Mathf.Sign(vector2Int.y);
        }

        public static bool Approximately(this Vector3 vector3, Vector3 toCompare)
        {
            return Mathf.Approximately(vector3.x, toCompare.x) && 
                   Mathf.Approximately(vector3.y, toCompare.y) &&
                   Mathf.Approximately(vector3.z, toCompare.z);
        }
    }
}