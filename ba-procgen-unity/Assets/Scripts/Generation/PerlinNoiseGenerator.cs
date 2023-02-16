using UnityEngine;
using ProcGen.Settings;

namespace ProcGen.Generation
{
    public class PerlinNoiseGenerator : HeightfieldGenerator
    {
        // Settings
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

        // Constructor
        public PerlinNoiseGenerator(int seed = 42) : base(seed)
        {
            Settings.Add(new Setting("Amplitude", 2.0f, 0.0f, 20.0f));
            Settings.Add(new Setting("Frequency", 0.2f, 0.0f, 1.0f));
        }

        public override float GetHeight(float x, float z)
        {
            float offsetY = (SettingsManager.Instance.MeshSettings.offset - 1) / 2.0f;

            return (Mathf.PerlinNoise(
                x * Frequency, 
                z * Frequency
            ) + offsetY)  * Amplitude;
        } 
    }
}
