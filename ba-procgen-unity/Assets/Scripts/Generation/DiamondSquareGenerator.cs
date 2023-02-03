using UnityEngine;

using ProcGen.Settings;

namespace ProcGen.Generation
{
    public class DiamondSquareGenerator : HeightfieldGenerator
    {
        //[SerializeField, Range(1,5)]
        int gridSize = 3; // Grid is always a square with a side length of gridSize; gridSize has to be of form 2^n+1!
        //[SerializeField]
        int iterations = 4; // TODO: Research best default value + Getter&Setter

        // Constructor
        public DiamondSquareGenerator(int seed = 42) : base(seed)
        {
            // Add default settings
            Settings.Add(new Setting("Grid size", gridSize));
            Settings.Add(new Setting("Iterations", iterations));
        }

        public override float GetHeight(int x, int z)
        {
            return randomGenerator.Next(100) / 100.0f * 2.0f;
            // return Random.Range(0.0f, 10.0f); // TODO
        }
    }
}
