using UnityEngine;
using System.Collections.Generic;

using ProcGen.Settings;

namespace ProcGen.Generation
{
    public class DiamondSquareGenerator : HeightfieldGenerator
    {
        [SerializeField, Range(1,5)]
        int gridSize = 3; // Grid is always a square with a side length of gridSize; gridSize has to be of form 2^n+1!
        [SerializeField]
        int iterations = 4; // TODO: Research best default value + Getter&Setter

        // Constructor
        public DiamondSquareGenerator()
        {
            // DEBUG: TODO: REMOVE!
            Settings.Add(new Setting("Test Int", 2));
            Settings.Add(new Setting("Test Float", 1.234f));
            Settings.Add(new Setting("Test String", "Test Value"));
            Settings.Add(new Setting("Test String 2", "Test Value 2"));
        }

        public override float GetHeight(int x, int z)
        {
            return Random.Range(0.0f, 10.0f); // TODO
        }
    }
}
