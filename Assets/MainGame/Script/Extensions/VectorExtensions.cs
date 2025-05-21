using UnityEngine;

namespace Tag.Block
{
    public static class VectorExtensions
    {
        public static Vector3 With(this Vector3 v, float? x = null, float? y = null, float? z = null)
        {
            v.x = x != null ? (float)x : v.x;
            v.y = y != null ? (float)y : v.y;
            v.z = z != null ? (float)z : v.z;

            return v;
        }
    }
}
