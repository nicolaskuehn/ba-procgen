using ProcGen.Settings;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProcGen.Generation
{
    [ExecuteInEditMode]
    public class VegetationDistributor : MonoBehaviour
    {
        // Constants for approximations of the standard normal integral
        private const float BETA_1 = -0.0004406f;
        private const float BETA_2 = 0.0418198f;
        private const float BETA_3 = 0.9000000f;
        private const float SQRT_PI = 1.7724539f;

        private const int MAX_PLANTS_IN_GRID_CELL = 10;

        // TODO: Add tree struct with (mesh, material, matrices) for multiple tree types
        [SerializeField]
        private Mesh treeMesh;
        [SerializeField]
        private Material treeMaterial;
        private List<Matrix4x4> treeMatrices = new List<Matrix4x4>();

        [SerializeField]
        private Texture2D biomesTexture;


        private GameObject terrainGO;

        private void Awake()
        {
            Init();
        }

        private void Update()
        {
            // Render vegetation
            if (treeMatrices.Count > 0)
                RenderTrees();
        }

        public void Init()
        {
            // Get reference to game object that holds all terrain chunks (meshes)
            terrainGO = FindFirstObjectByType<MeshGenerator>().terrainGO;

            if (terrainGO == null)
                Debug.LogWarning("Could not find terrain game object in VegetationDistributor. Is there a MeshGenerator in the scene and the terrain is generated?");
        }

        public void ResetVegetationState()
        {
            // Clear instanced rendering queue (of model matrices)
            treeMatrices.Clear();

            // Reset grid cell state (especially HasModelPlaced property)
            List<Transform> terrainChunks = terrainGO.transform.Cast<Transform>().ToList();
            foreach (Transform chunkTransform in terrainChunks)
            {
                Chunk chunk = chunkTransform.GetComponent<Chunk>();

                // Iterate over all grid cells in tile
                /*
                for (int z = 0; z < chunk.GridCellCount1D; z++)
                {
                    for (int x = 0; x < chunk.GridCellCount1D; x++)
                    {
                        // Reset model placement marker of grid cell
                        chunk.GetGridCellDataAtCoords(x, z).HasModelPlaced = false;
                    }
                }
                */
            }
        }

        private void LoadBiomeData()    // TODO: Call this method at the "right" place/moment
        {
            // Load biome data from texture (for each grid cell of each chunk)
            List<Transform> terrainChunks = terrainGO.transform.Cast<Transform>().ToList();
            foreach (Transform chunkTransform in terrainChunks)
            {
                Chunk chunk = chunkTransform.GetComponent<Chunk>();

                // Iterate over all grid cells in tile
                for (int z = 0; z < chunk.GridCellCount1D; z++)
                {
                    for (int x = 0; x < chunk.GridCellCount1D; x++)
                    {
                        // ... Sample pixel of biomes texture that corresponds to the current grid cell ... //
                        Vector2 worldPos = chunk.GridCoordsToWorld(x, z);

                        float relativeX = worldPos.x / SettingsManager.Instance.MeshSettings.size;
                        float relativeZ = worldPos.y / SettingsManager.Instance.MeshSettings.size;

                        int pixelX = Mathf.FloorToInt(Mathf.Lerp(0.0f, biomesTexture.width, relativeX));
                        int pixelZ = Mathf.FloorToInt(Mathf.Lerp(0.0f, biomesTexture.height, relativeZ));

                        Color sampledPixel = biomesTexture.GetPixel(pixelX, pixelZ);     // TODO: Maybe use GetPixelData<>() for better performance?


                        // ... Read and store data from sampled pixel ... //
                        GridCellData data = chunk.GetGridCellDataAtCoords(x, z);

                        // Get soil fertility value (Red channel)
                        data.SoilFertility = sampledPixel.r;

                        // Get climate value (Green channel)
                        data.Climate = sampledPixel.g;

                        // Debug.Log($"SF: {data.SoilFertility}, C: {data.Climate}");
                    }
                }
            }
        }
        

        private float RunNormalDistributionRandomExperiment(float expVal, float stdDev)
        {
            // ... VERSION 1 (PAPER) ... //
            //float z = (Random.value - expVal) / stdDev;
            // z = Mathf.Clamp(z, -8.0f, 8.0f);        // See paper "A Sigmoid Approximation of the Standard Normal Integral"
            // TODO: Use the INVERSE function of the original CDF below (currently NOT working!)
            // return 1.0f / ( 1.0f + Mathf.Exp(-SQRT_PI * (BETA_1 * z*z*z*z*z + BETA_2 * z*z*z + BETA_3 * z) ) );

            // ... VERSION 2 (OWN) ... //
            float x = Random.value;
            const float c_s = 1.6f;
            return Mathf.Log(1.0f/x - 1.0f) * (stdDev/c_s) + expVal;
        }

        // Iterate over terrain and add vegetation
        public void DistributeVegetationOnTerrain()
        {
            // ... Setup ... //
            // Reset vegetation system state
            ResetVegetationState();

            // Load biome data
            LoadBiomeData();


            // ...  Calculate distribution probabilities ... //
            // Get all chunks
            List<Transform> terrainChunks = terrainGO.transform.Cast<Transform>().ToList();


            // ... Evaluate vegetation ... //
            // Iterate over each chunk to handle them separately
            foreach (Transform chunkTransform in terrainChunks)
            {
                Chunk chunk = chunkTransform.GetComponent<Chunk>();

                Vector3 center = chunkTransform.position;    // origin of each tile is in the center
                int size = SettingsManager.Instance.MeshSettings.size;

                // Iterate over all grid cells in tile
                for (int z = 0; z < chunk.GridCellCount1D; z++)
                {
                    for (int x = 0; x < chunk.GridCellCount1D; x++)
                    {
                        Vector2 worldPos2D = chunk.GridCoordsToWorld(x, z);
                        float gridOffset = 0.5f * SettingsManager.Instance.ChunkSettings.gridCellSize;  // Used to offset position to cell center

                        DistributeVegetationInGridCell(
                            worldPos2D.x + gridOffset,
                            worldPos2D.y + gridOffset,
                            chunk.GetGridCellDataAtCoords(x, z)
                        );
                    }
                }
            }

            // Trigger rendering of scene view
            UnityEditor.SceneView.RepaintAll();
        }

        private void DistributeVegetationInGridCell(float centerX, float centerZ, GridCellData cellData)
        {
            // ... Calculate vegetation based on biome data ... //
            // Calculate height for given position (x, z)
            float y = SettingsManager.Instance.HeightfieldCompositor.GetComposedHeight(centerX, centerZ);

            float PlantCount_ExpVal = Mathf.Lerp(0, MAX_PLANTS_IN_GRID_CELL, cellData.SoilFertility); // TODO: Add influence of height
            float PlantCount_StdDev = Mathf.Exp( -24.0f * ((cellData.Climate - 0.5f) * (cellData.Climate - 0.5f)) );

            // Run random experiment with expected value and standard deviation to determine plant count
            float plantCount = Mathf.RoundToInt(
                Mathf.Clamp(
                    RunNormalDistributionRandomExperiment(PlantCount_ExpVal, PlantCount_StdDev), 
                    0.0f, 
                    MAX_PLANTS_IN_GRID_CELL
                )
            );
            // Debug.Log($"count: {plantCount} | exp: {PlantCount_ExpVal}, sd: {PlantCount_StdDev}");


            // ... Determine spawn locations for plants using Poisson Disc Sampling ... //
            // Create grid
            const float r = 0.1f;
            const int k = 30;
            float w = r / Mathf.Sqrt(2);

            int d = Mathf.FloorToInt(SettingsManager.Instance.ChunkSettings.gridCellSize / w);

            int[,] grid = new int[d, d];

            // Init grid
            for (int z = 0; z < d; z++)
            {
                for (int x = 0; x < d; x++)
                {
                    grid[x, z] = -1;
                }
            }

            // Select initial sample randomly
            int sampleIndex = 0;
            List<int> active = new List<int>();

            float gridCellHalfSize = 0.5f * SettingsManager.Instance.ChunkSettings.gridCellSize;
            float randomX = Random.Range(centerX - gridCellHalfSize, centerX + gridCellHalfSize);
            float randomZ = Random.Range(centerZ - gridCellHalfSize, centerX + gridCellHalfSize);

            int iX = Mathf.FloorToInt(randomX / w);
            int iZ = Mathf.FloorToInt(randomZ / w);

            grid[iX, iZ] = sampleIndex;
            active.Add(sampleIndex);
            sampleIndex++;

            // Choose random sample from active list while not empty
            while (active.Count > 0)
            {
                int randomIndex = Random.Range(0, active.Count);

                for (int i = 0; i < k; i++)
                {
                    float randomAngle = Random.Range(0.0f, 2.0f*Mathf.PI);
                    float randomLength = Random.Range(r, 2.0f*r);
                    float offX = Mathf.Cos(randomAngle) * randomLength;
                    float offZ = Mathf.Sin(randomAngle) * randomLength;

                    // TODO: Continue
                }

            }


            // Offset each tree randomly inside it's cell to break the visible grid pattern
            /*
            float gridOffset = 0.5f * SettingsManager.Instance.ChunkSettings.gridCellSize;
            float randomOffsetX = Random.Range(0.0f, gridOffset);
            float randomOffsetZ = Random.Range(0.0f, gridOffset);
            */


            // ... Do plausability check for each plant separately ... //
            // TODO
            // Do not place vegetation below water level
            if (y < SettingsManager.Instance.MeshSettings.waterLevel) return;
            // Do not place vegetation above certain height
            // TODO

                
            // InstantiateTree(new Vector3(centerX + randomOffsetX, y, centerZ + randomOffsetZ));
        }

        private void InstantiateTree(Vector3 pos)
        {
            // Add tree to instanced rendering queue
            treeMatrices.Add(
                Matrix4x4.TRS(
                    pos,                                                        // Translation
                    Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f),   // Rotation
                    Vector3.one * 0.03f                                         // Scale
                )
            );
        }

        public void RenderTrees()
        {
            // Check if trees can be rendered in a single instanced draw call
            if (treeMatrices.Count <= 1023)
            {
                Graphics.DrawMeshInstanced(
                    treeMesh,                   // mesh
                    0,                          // submeshIndex
                    treeMaterial,               // material
                    treeMatrices.ToArray(),     // matrices
                    treeMatrices.Count          // count
                );

                return;
            }

            // Otherwise draw meshes instanced in batches of 1023 (which is the max count)
            for (int i = 0; i < Mathf.CeilToInt((float) treeMatrices.Count / 1023); i++)
            {
                int startIndex = i * 1023;
                int count = Mathf.Min(treeMatrices.Count - startIndex, 1023);

                //Debug.Log($"{treeMatrices.Count} - {startIndex} = {count}");

                Matrix4x4[] currentBatchTreeMatrices = treeMatrices.GetRange(startIndex, count).ToArray();

                Graphics.DrawMeshInstanced(
                    treeMesh,                   // mesh
                    0,                          // submeshIndex
                    treeMaterial,               // material
                    currentBatchTreeMatrices,   // matrices
                    count                       // count
                );
            }
        }
    }
}
