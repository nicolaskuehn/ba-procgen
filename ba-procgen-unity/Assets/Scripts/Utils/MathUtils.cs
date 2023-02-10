using UnityEngine;

namespace ProcGen.Utils
{
    public static class MathUtils
    {
        public static float Map(float aMin, float aMax, float bMin, float bMax, float t)
        {
            return Mathf.Min(Mathf.Max((t - aMin) * (bMax - bMin) / (aMax - aMin) + bMin, bMin), bMax);
        }
    }
}
