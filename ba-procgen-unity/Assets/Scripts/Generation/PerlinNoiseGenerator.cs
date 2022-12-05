using UnityEngine;

namespace ProcGen.Generation
{
    public class PerlinNoiseGenerator : IHeightfieldGenerator
    {
        public float GetHeight(int x, int z)
        {
            return Random.Range(0.0f, 2.0f); // TODO
        }
    }
}
