using ProcGen.Settings;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProcGen.Generation
{
    public class Octave
    {
        public readonly Guid id;

        public enum EGenerationMethod
        {
            DiamondSquare = 0,
            FractionalBrownianMotion = 1,
            PerlinNoise = 2
        }

        private EGenerationMethod generationMethod;
        public EGenerationMethod GenerationMethod 
        {
            get => generationMethod;
            set 
            {
                // Safe previous settings if they exist and only if the generation method is the same
                List<Setting> settings = null;
                if (HeightfieldGenerator != null && value == GenerationMethod)
                    settings = HeightfieldGenerator.Settings;

                // Set noise generator according to generation method
                switch(value)
                {
                    case EGenerationMethod.DiamondSquare:
                        HeightfieldGenerator = new DiamondSquareGenerator();
                        generationMethod = EGenerationMethod.DiamondSquare;
                        break;

                    case EGenerationMethod.FractionalBrownianMotion:
                        HeightfieldGenerator = new FractionalBrownianMotionGenerator();
                        generationMethod = EGenerationMethod.FractionalBrownianMotion;
                        break;

                    case EGenerationMethod.PerlinNoise:
                        HeightfieldGenerator = new PerlinNoiseGenerator();
                        generationMethod = EGenerationMethod.PerlinNoise;
                        break;
                    
                    default:
                        HeightfieldGenerator = new DiamondSquareGenerator();
                        generationMethod = EGenerationMethod.DiamondSquare;
                        break;
                }

                // Apply previous settings (if they exist)
                if (settings != null)
                    HeightfieldGenerator.Settings = settings;

            } 
        }

        // Heightfield generator used for this octave
        public HeightfieldGenerator HeightfieldGenerator { get; private set; }

        // Constructor
        public Octave(EGenerationMethod method = EGenerationMethod.DiamondSquare)
        {
            id = Guid.NewGuid();
            GenerationMethod = method;
        }
    }
}