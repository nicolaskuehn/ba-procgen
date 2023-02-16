using UnityEngine;
using ProcGen.Settings;

namespace ProcGen.Generation
{
    public class FractionalBrownianMotionGenerator : HeightfieldGenerator
    {
        // Settings
        public float FractalIncrement   // also called Hurst parameter
        {
            get => (float)Settings.Find(s => s.Name == "Fractal Increment").Value;
            private set
            {
                Settings.Find(s => s.Name == "Fractal Increment").Value = value;
            }
        }

        public float Lacunarity
        {
            get => (float)Settings.Find(s => s.Name == "Lacunarity").Value;
            private set
            {
                Settings.Find(s => s.Name == "Lacunarity").Value = value;
            }
        }

        public int NumOctaves
        {
            get => (int)Settings.Find(s => s.Name == "Number of Octaves").Value;
            private set
            {
                Settings.Find(s => s.Name == "Number of Octaves").Value = value;
            }
        }

        public float Amplitude
        {
            get => (float)Settings.Find(s => s.Name == "Amplitude").Value;
            set
            {
                Settings.Find(s => s.Name == "Amplitude").Value = value;
            }
        }

        public float Frequency
        {
            get => (float)Settings.Find(s => s.Name == "Frequency").Value;
            set
            {
                Settings.Find(s => s.Name == "Frequency").Value = value;
            }
        }

        // Noise Generator
        private PerlinNoiseGenerator noiseGen;

        // Constructor
        public FractionalBrownianMotionGenerator (int seed = 42) : base(seed)
        {
            Settings.Add(new Setting("Fractal Increment", 0.2f, 0.0f, 1.0f));
            Settings.Add(new Setting("Lacunarity", 2.0f, 0.0f, 16.0f));
            Settings.Add(new Setting("Number of Octaves", 4, 1, 10));

            Settings.Add(new Setting("Amplitude", 2.0f, 0.0f, 20.0f));
            Settings.Add(new Setting("Frequency", 0.2f, 0.0f, 1.0f));

            // Initialize noise generator with given seed and neutral standard settings
            noiseGen = new PerlinNoiseGenerator(seed);
            noiseGen.Amplitude = Amplitude;
            noiseGen.Frequency = Frequency;
        }

        public override float GetHeight(float x, float z)
        {
            float h = FractalIncrement;
            float l = Lacunarity;

            float pX = x;
            float pZ = z;

            float height = 0.0f;

            // Set amplitude and frequency for noise gen
            noiseGen.Amplitude = Amplitude;
            noiseGen.Frequency = Frequency;

            // Calculate combination of noise of all octaves iteratively
            for (int i = 0; i < NumOctaves; i++)
            {
                height += noiseGen.GetHeight(pX, pZ) * Mathf.Pow(l, -h * i);
                pX *= l;
                pZ *= l;
            }

            return height;
        }
    }
}