using UnityEngine;

namespace ProcGen.Generation
{
    public class DiamondSquareGenerator : IHeightfieldGenerator
    {
        public float GetHeight(int x, int z)
        {
            return Random.Range(0.0f, 10.0f); // TODO
        }
    }
}
