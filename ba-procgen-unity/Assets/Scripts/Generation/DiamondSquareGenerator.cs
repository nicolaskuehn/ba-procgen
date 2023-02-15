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
        private float GetRandomHeight() => MathUtils.Map(0.0f, 1.0f, MinHeight, MaxHeight, (float) randomGenerator.NextDouble());
        private float GetRandomValue() => MathUtils.Map(0.0f, 1.0f, -currentRoughness, currentRoughness, (float) randomGenerator.NextDouble());

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
            float sizeScalingFactor = MeshGenerator.SizeScalingFactor;

            float i = x / sizeScalingFactor;
            float j = z / sizeScalingFactor;

            int leftI = Mathf.FloorToInt(i);
            int topJ = Mathf.FloorToInt(j);

            int rightI = leftI + 1;
            int bottomJ = topJ + 1;

            // ... Edge treatment ... //
            // Calculate local t values for interpolation
            float ti = Mathf.InverseLerp(leftI, rightI, i);
            float tj = Mathf.InverseLerp(topJ, bottomJ, j);

            int maxI = heightfield.GetLength(0) - 1;
            int maxJ = heightfield.GetLength(1) - 1;

            leftI = Mathf.Clamp(leftI, 0, maxI);
            topJ = Mathf.Clamp(topJ, 0, maxJ);

            rightI = Mathf.Clamp(rightI, 0, maxI);
            bottomJ = Mathf.Clamp(bottomJ, 0, maxJ);

            // Top left
            float tl = heightfield[leftI, topJ];

            // Top right
            float tr = heightfield[rightI, topJ];

            // Bottom left
            float bl = heightfield[leftI, bottomJ];

            // Bottom right
            float br = heightfield[rightI, bottomJ];

            // Interpolate between the heightfield values bilinearly
            return Mathf.Lerp(
                Mathf.LerpUnclamped(tl, tr, ti),
                Mathf.LerpUnclamped(bl, br, ti),
                tj
            );
        }

        // Generate heightfield with diamond-square algorithm
        private void GenerateHeightfield()
        {
            // Create new 2D heightfield array with current dimensions
            int size1D = MeshGenerator.VertexCount1D;
            heightfield = new float[size1D,size1D];

            // DEBUG
            /*
            for (int j = 0; j < size1D; j++)
            {
                for (int i = 0; i < size1D; i++)
                {
                    heightfield[i, j] = 0.5f * (i + j);
                }
            }

            Debug.Log("DiamondSquareGenerator: Heightfield generated!");
            return;
            */
            // DEBUG - END

            // Describes the current size of a square and/or a diamond
            int unitSize = MeshGenerator.FaceCount1D;

            /*
            heightfield[0,0] = GetRandomHeight();
            heightfield[0, unitSize] = GetRandomHeight();
            heightfield[unitSize, unitSize] = GetRandomHeight();
            heightfield[unitSize, 0] = GetRandomHeight();
            */

            heightfield[0, 0] = 0.0f;
            heightfield[0, unitSize] = 5.0f;
            heightfield[unitSize, unitSize] = 10.0f;
            heightfield[unitSize, 0] = 5.0f;

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
                InterpolateDiamond(unitSize);

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
            int maxX = heightfield.GetLength(0) - 1;
            int maxZ = heightfield.GetLength(1) - 1;

            // (i, j) describes the unit's coordinates in unit space
            for (int j = 0; j < maxZ / unitSize; j++)
            {
                for(int i = 0; i < maxX / unitSize; i++)
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
                    //v += GetRandomValue();

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
            for (int j = 0; j < maxZ / halfUnitSize + 1; j++)
            {
                for (int i = 0; i < maxX / halfUnitSize + 1; i++)
                {
                    // Only interpolate points on a diamond grid
                    if ((j % 2 == 0 && i % 2 == 0) || j % 2 == 1 && i % 2 == 1)
                        continue;

                    // Map coordinates from unit space to heightfield space, (x, z) describing the center vertex of the diamond
                    int x = i * halfUnitSize;
                    int z = j * halfUnitSize;

                    // Debug.Log($"[{unitSize}] (x,z): ({x},{z})");

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
                    //v += GetRandomValue();

                    // Assign this weighted sum to the center vertex of the square
                    heightfield[x, z] = v;
                }
            }
        }
    }
}
