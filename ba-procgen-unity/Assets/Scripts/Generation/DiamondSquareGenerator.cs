using ProcGen.Settings;
using ProcGen.Utils;
using UnityEngine;

namespace ProcGen.Generation
{
    public class DiamondSquareGenerator : HeightfieldGenerator
    {
        // Constants
        private static int MAX_ITERATIONS = 1000;


        // ... Settings ... //
        public float Roughness
        {
            get => (float)Settings.Find(s => s.Name == "Roughness").Value;
            private set
            {
                Settings.Find(s => s.Name == "Roughness").Value = value;
            }
        }

        // Determines the minimum height which can be generated
        public float MinHeight
        {
            get => (float)Settings.Find(s => s.Name == "MinHeight").Value;
            private set
            {
                Settings.Find(s => s.Name == "MinHeight").Value = value;
            }
        }

        // Determines the maximum height which can be generated
        public float MaxHeight
        {
            get => (float)Settings.Find(s => s.Name == "MaxHeight").Value;
            private set
            {
                Settings.Find(s => s.Name == "MaxHeight").Value = value;
            }
        }

        // Determines the change of the random range each iteration; this influences the roughness of the terrain
        private float currentRoughness;

        // Stores the heightfield
        private float[,] heightfield;

        // Calculate random values constrained by their min and max (see settings)
        private float RandomHeight => MathUtils.Map(0.0f, 1.0f, MinHeight, MaxHeight, (float) randomGenerator.NextDouble());
        private float RandomValue => MathUtils.Map(0.0f, 1.0f, -currentRoughness, currentRoughness, (float) randomGenerator.NextDouble());

        // Constructor
        public DiamondSquareGenerator(int seed = 42) : base(seed)
        {
            // Add default settings
            Settings.Add(new Setting("Roughness", 2.0f, 1.0f, 16.0f));
            Settings.Add(new Setting("MinHeight", -5.0f, -20.0f, 20.0f));
            Settings.Add(new Setting("MaxHeight", 5.0f, -20.0f, 20.0f));
        }

        public override float GetHeight(float x, float z)
        {
            // Generate heightfield if it was not generated already
            if (heightfield == null)
                GenerateHeightfield();



            // Next four valid coordinates (in heightfield space) describing the square in which the given point lays
            
            int vertexCount1D = 1 << SettingsManager.Instance.MeshSettings.subdivisions + 1;
            int normalizingFactor = SettingsManager.Instance.MeshSettings.size / vertexCount1D;

            int fx = Mathf.FloorToInt(x / normalizingFactor);
            int fz = Mathf.FloorToInt(z / normalizingFactor);
            int cx = Mathf.CeilToInt(x / normalizingFactor);
            int cz = Mathf.CeilToInt(z / normalizingFactor);

            /*
            Debug.Log($"fx: {fx}");
            Debug.Log($"fz: {fz}");
            Debug.Log($"cx: {cx}");
            Debug.Log($"cz: {cz}");
            */

            // Top left
            float tl = heightfield[fx, fz];

            // Top right
            float tr = heightfield[cx, fz];

            // Bottom left
            float bl = heightfield[fx, cz];

            // Bottom right
            float br = heightfield[cx, cz];

            // Interpolate between the heightfield values bilinearly
            return Mathf.Lerp(
                Mathf.Lerp(tl, tr, x), 
                Mathf.Lerp(bl, br, x), 
                z
            );
        }

        // Generate heightfield with diamond-square algorithm
        private void GenerateHeightfield()
        {
            Debug.Log("DiamondSquareGenerator: Heightfield generated!");

            // Create new 2D heightfield array with current dimensions
            int size1D = 1 << SettingsManager.Instance.MeshSettings.subdivisions + 1;
            heightfield = new float[size1D,size1D];

            // Pick inital random values for the corners
            int maxInd = size1D - 1;

            heightfield[0,0] = RandomHeight;
            heightfield[0, maxInd] = RandomHeight;
            heightfield[maxInd, maxInd] = RandomHeight;
            heightfield[maxInd, 0] = RandomHeight;

            // Describes the current size of a square and/or a diamond
            int unitSize = size1D;

            // Make copy of roughness that can be changed
            currentRoughness = Roughness;

            // Perform square steps and diamond steps alternately until no subdivision can be made
            int iterations = 0;
            while (unitSize > 1)
            {
                // Just for safety
                if (iterations > MAX_ITERATIONS)
                {
                    Debug.LogError($"DiamondSquareGenerator with hash code {GetHashCode()} exceeded the maximum iterations. Abort.");
                    break;
                }

                InterpolateSquare(unitSize);
                InterpolateDiamond(unitSize); // TODO: uncomment

                // Subdivide units each iteration
                unitSize /= 2;

                // Modulate roughness each iteration (effectively smoothing the terrain)
                currentRoughness /= 2.0f;

                iterations++;
            }
        }

        private void InterpolateSquare(int unitSize)
        {
            int halfUnitSize = unitSize / 2;

            // Loop through all units in heightfield
            int maxX = heightfield.GetLength(0);
            int maxZ = heightfield.GetLength(1);

            // (i, j) describes the unit's coordinates in unit space
            for (int j = 0; j < maxZ / unitSize - 1; j++)
            {
                for(int i = 0; i < maxX / unitSize - 1; i++)
                {
                    // Map coordinates from unit space to heightfield space, (x, z) describing the top left vertex of the square
                    int x = i * unitSize;
                    int z = j * unitSize;

                    // Calculate average of the values of the four corner vertices describing this square
                    float v = 0;

                    v += heightfield[x, z];
                    v += heightfield[x + unitSize, z];
                    v += heightfield[x, z + unitSize];
                    v += heightfield[x + unitSize, z + unitSize];
                    v /= 4.0f;

                    // Add random vale to the average value
                    v += RandomValue;

                    // Assign this weighted sum to the center vertex of the square
                    heightfield[x + halfUnitSize, z + halfUnitSize] = v;
                }
            }
        }

        private void InterpolateDiamond(int unitSize)
        {
            int halfUnitSize = unitSize / 2;

            // Loop through all units in heightfield
            int maxX = heightfield.GetLength(0);
            int maxZ = heightfield.GetLength(1);

            // (i, j) describes the unit's coordinates in unit space
            for (int j = 0; j < maxZ / unitSize; j++)
            {
                for (int i = 0; i < maxX / unitSize; i++)
                {
                    // Map coordinates from unit space to heightfield space, (x, z) describing the center vertex of the diamond
                    int x = i * unitSize + halfUnitSize;
                    int z = j * unitSize + halfUnitSize;

                    // Calculate average of the values of the four (on the edges only three) diamond tips vertices to the middle vertex
                    float v = 0;
                    int numSamples = 0;

                    // Left
                    if (x - halfUnitSize > 0)
                    {
                        v += heightfield[x - halfUnitSize, z];
                        numSamples++;
                    }
                    
                    // Top
                    if (z - halfUnitSize > 0)
                    {
                        v += heightfield[x, z - halfUnitSize];
                        numSamples++;
                    }
                    
                    // Right
                    if (x + halfUnitSize < maxX)
                    {
                        v += heightfield[x + halfUnitSize, z];
                        numSamples++;
                    }

                    // Bottom
                    if (z + halfUnitSize < maxZ)
                    {
                        v += heightfield[x, z + halfUnitSize];
                        numSamples++;
                    }

                    v /= numSamples;

                    // Add random vale to the average value
                    v += RandomValue;

                    // Assign this weighted sum to the center vertex of the square
                    heightfield[x, z] = v;
                }
            }
        }
    }
}
