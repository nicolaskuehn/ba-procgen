using ProcGen.Settings;
using ProcGen.Utils;
using UnityEngine;

namespace ProcGen.Generation
{
    public static class TextureGenerator
    {
        public static Texture2D GenerateTextureMap(int width, int height, Octave octave)
        {
            // Create texture
            Texture2D tex = new Texture2D(width, height);

            // Loop over all pixels
            Color col = new Color(0.0f, 0.0f, 0.0f, 1.0f);  // values between 0-1!

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Normalize height value to be in range [0, 1]
                    
                    float minHeight = 0.0f;
                    float maxHeight = 0.0f;

                    // Calculate min and max height dependant on the heightfield generation method
                    switch(octave.GenerationMethod)
                    {
                        case Octave.EGenerationMethod.DiamondSquare:
                            minHeight = ((DiamondSquareGenerator)octave.HeightfieldGenerator).MinHeight;
                            maxHeight = ((DiamondSquareGenerator)octave.HeightfieldGenerator).MaxHeight;
                            break;

                        case Octave.EGenerationMethod.FractionalBrownianMotion:
                            minHeight = 0.0f;   // TODO: Calculate real min height!
                            maxHeight = 10.0f;  // TODO: Calculate real max height!
                            break;

                        case Octave.EGenerationMethod.PerlinNoise:
                            float absMinMaxHeight = Mathf.Sqrt(0.5f); // Generally [-sqrt(N/4), sqrt(N/4)], N beeing the number of dimensions
                            float amplitude = ((PerlinNoiseGenerator)octave.HeightfieldGenerator).Amplitude;
                            minHeight = (-absMinMaxHeight + SettingsManager.Instance.MeshSettings.offset) * amplitude;
                            maxHeight = (absMinMaxHeight + SettingsManager.Instance.MeshSettings.offset) * amplitude;
                            break;

                        default:
                            Debug.LogError("2D texture generation is not implemented for this generation method!");
                            break;
                    }

                    float normalizedHeight = MathUtils.Map(
                        minHeight, 
                        maxHeight, 
                        0.0f, 
                        1.0f, 
                        octave.HeightfieldGenerator.GetHeight(
                            x * SettingsManager.Instance.MeshSettings.size / width, 
                            y * SettingsManager.Instance.MeshSettings.size / height
                        )
                    );

                    // Write normalized height field (range [0, 1]) value into the texture as rgba value 
                    col.r = col.g = col.b = normalizedHeight;
                    tex.SetPixel(x, y, col);
                }
            }

            // Apply the changes (setting of pixel color's)
            tex.Apply();

            return tex;
        }
    }
}
