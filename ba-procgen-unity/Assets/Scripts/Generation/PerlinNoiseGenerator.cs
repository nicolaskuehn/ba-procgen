using UnityEngine;

namespace ProcGen.Generation
{
    public class PerlinNoiseGenerator : HeightfieldGenerator
    {
        public override float GetHeight(int x, int z)
        {
            return Random.Range(0.0f, 2.0f);
        }
    }
}
