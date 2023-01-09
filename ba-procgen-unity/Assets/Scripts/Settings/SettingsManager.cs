using UnityEngine;
using System.Collections.Generic;
using ProcGen.Generation;

namespace ProcGen.Settings
{   
    public sealed class SettingsManager : MonoBehaviour
    {
        // Singleton setup (not thread-safe)
        public static SettingsManager Instance { get; private set; }
        private SettingsManager() { }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                Debug.LogError("Destroyed duplicate instance of SettingsManager (Singleton)!");
            }
            else
                Instance = this;
        }

        // ... User Settings ... //
        // Determines the resolution (number of tiles/squares) of the mesh of the plane in x- and z-direction
        [SerializeField, Range(2, 250), Tooltip("Resolution of the mesh in x- and z-direction")]
        public int resolution = 10; // TODO: split current resolution functionality in: resolution and size (currently size scales with resolution)

        // Let's the user select a generation method for the heightfield
        public enum HeightfieldGeneratorType
        {
            DiamondSquare = 0,
            PerlinNoise = 1
        }

        private readonly HeightfieldGenerator[] heightfieldGenerators = 
        { 
            new DiamondSquareGenerator(), 
            new PerlinNoiseGenerator() 
        };

        // Editor property
        [SerializeField]
        private HeightfieldGeneratorType heightfieldGenerator = HeightfieldGeneratorType.DiamondSquare;  // Default: Diamond-square algorithm

        public HeightfieldGenerator HeightfieldGenerator
        {
            get => heightfieldGenerators[(int)heightfieldGenerator];
            private set { }
        }
    }
}
