using UnityEngine;
using ProcGen.Settings;

namespace ProcGen.Generation
{
    public class PerlinNoiseGenerator : HeightfieldGenerator
    {
        // Settings
        private float Amplitude 
        {
            get => (float)Settings.Find(s => s.Name == "Amplitude").Value;
            set
            {
                Settings.Find(s => s.Name == "Amplitude").Value = value;
            }
        }

        private float Frequency
        {
            get => (float)Settings.Find(s => s.Name == "Frequency").Value;
            set
            {
                Settings.Find(s => s.Name == "Frequency").Value = value;
            }
        }

        // Constructor
        public PerlinNoiseGenerator(int seed = 42) : base(seed)
        {
            Settings.Add(new Setting("Amplitude", 1.0f));
            Settings.Add(new Setting("Frequency", 1.0f));
        }

        public override float GetHeight(int x, int z)
        {
            

            return Mathf.PerlinNoise(
                ((float) x / SettingsManager.Instance.resolution) * Frequency, 
                ((float) z / SettingsManager.Instance.resolution) * Frequency
            ) * Amplitude;
        } 
    }
}
