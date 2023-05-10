using ProcGen.Settings;
using ProcGen.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProcGen.Generation
{
    [ExecuteInEditMode]
    public class VegetationDistributor : MonoBehaviour
    {
        // Constants for approximations of the standard normal integral
        // TODO: REMOVE
        private const float BETA_1 = -0.0004406f;
        private const float BETA_2 = 0.0418198f;
        private const float BETA_3 = 0.9000000f;
        private const float SQRT_PI = 1.7724539f;

        const float BASE_MODEL_SCALE = 0.03f;

        // TODO: Add tree struct with (mesh, material, matrices) for multiple tree types
        [SerializeField]
        private Mesh treeMesh;
        [SerializeField]
        private Material broadleafTreeMaterial;
        [SerializeField]
        private Material pineTreeMaterial;

        private List<Matrix4x4> broadleafTreeMatrices = new List<Matrix4x4>();
        private List<Matrix4x4> pineTreeMatrices = new List<Matrix4x4>();

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
            if (broadleafTreeMatrices.Count > 0 || pineTreeMatrices.Count > 0)
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
            broadleafTreeMatrices.Clear();

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
            int MAX_PLANTS_IN_GRID_CELL = SettingsManager.Instance.VegetationSettings.maxPlantsInGridCell;

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


            // ... Determine spawn locations for plants ... //
            int d = Mathf.CeilToInt(Mathf.Sqrt(MAX_PLANTS_IN_GRID_CELL));

            Vector2[,] backgroundGrid = new Vector2[d, d];
            float w = SettingsManager.Instance.ChunkSettings.gridCellSize / d;

            for (int j = 0; j < d; j++)
            {
                for (int i = 0; i < d; i++)
                {
                    backgroundGrid[i, j] = new Vector2(-99.0f, -99.0f);    // this value is viewed as invalid
                }
            }

            const int MAX_ITERATIONS = 100;
            List<Vector2> foundPositionsWorld = new List<Vector2>();

            for (int i = 0; i < MAX_ITERATIONS; i++)
            {
                // Break if all plant positions are found
                if (foundPositionsWorld.Count >= plantCount) break;

                // Pick random cell of background grid
                int randIndX = Random.Range(0, d);
                int randIndZ = Random.Range(0, d);

                // Check if this cell is already occupied
                if (!(backgroundGrid[randIndX, randIndZ].x < -1.0f || backgroundGrid[randIndX, randIndZ].y < -1.0f)) continue;

                // Generate random position in background grid cell as relative offset in range [-1.0, 1.0]
                float randOffX = Random.Range(-1.0f, 1.0f);
                float randOffZ = Random.Range(-1.0f, 1.0f);

                // Calculate relative position to world position
                Vector2 relativePos = new Vector2(randOffX, randOffZ);

                float gridCellHalfSize = 0.5f * SettingsManager.Instance.ChunkSettings.gridCellSize;
                Vector2 worldPos = new Vector2(
                    (centerX - gridCellHalfSize) + randIndX * w + 0.5f * w + relativePos.x * w,
                    (centerZ - gridCellHalfSize) + randIndZ * w + 0.5f * w + relativePos.y * w
                );

                // ... Check plausability rules ... //
                float height = SettingsManager.Instance.HeightfieldCompositor.GetComposedHeight(worldPos.x, worldPos.y);
                
                // Do not place vegetation below water level
                if (height < SettingsManager.Instance.MeshSettings.waterLevel) continue;

                // Do not place vegetation above max growing height
                if (height > SettingsManager.Instance.VegetationSettings.maxPlantGrowHeight) continue;


                // ... Position is valid and plausible ... //
                // Save relative position in background grid
                backgroundGrid[randIndX, randIndZ] = relativePos;

                // Save world position in found positions list
                foundPositionsWorld.Add(worldPos);
            }

            // Spawn plants at the found positions
            for (int i = 0; i < foundPositionsWorld.Count; i++)
            {
                Vector2 worldPos = foundPositionsWorld[i];
                float terrainHeight = SettingsManager.Instance.HeightfieldCompositor.GetComposedHeight(worldPos.x, worldPos.y);

                // Calculate scale of plant based on height (and small random factor)
                float randomHeightOff = Random.Range(0.0f, 0.01f);

                float baseScale = Mathf.InverseLerp(
                    SettingsManager.Instance.VegetationSettings.maxPlantGrowHeight,
                    SettingsManager.Instance.MeshSettings.waterLevel,
                    terrainHeight
                );

                baseScale = MathUtils.Map(0.0f, 1.0f, 0.8f, 1.0f, baseScale) * BASE_MODEL_SCALE;

                Vector3 scale = new Vector3(baseScale, baseScale + randomHeightOff, baseScale);

                float randomTypeFactor = Random.value;

                InstantiateTree(
                    new Vector3(
                        worldPos.x,
                        terrainHeight, 
                        worldPos.y
                    ),
                    scale,
                    randomTypeFactor < 0.5f ? "broadleaf" : "pine"
                );
            }


            /* // ... OLD VERSION ... //
            // ... Determine spawn locations for plants using Poisson Disc Sampling ... //
            // Create grid
            const float r = 0.1f;
            const int k = 30;
            float w = r / Mathf.Sqrt(2);

            int d = Mathf.FloorToInt(SettingsManager.Instance.ChunkSettings.gridCellSize / w);

            Vector2[,] grid = new Vector2[d, d];

            // Init grid
            for (int z = 0; z < d; z++)
            {
                for (int x = 0; x < d; x++)
                {
                    grid[x, z] = Vector2.negativeInfinity;  // TODO: Check if there is a better way of initializing
                }
            }

            // Select initial sample randomly
            List<Vector2> active = new List<Vector2>();

            float gridCellHalfSize = 0.5f * SettingsManager.Instance.ChunkSettings.gridCellSize;
            float randomX = Random.Range(centerX - gridCellHalfSize, centerX + gridCellHalfSize);
            float randomZ = Random.Range(centerZ - gridCellHalfSize, centerX + gridCellHalfSize);

            int iX = Mathf.FloorToInt(randomX / w);
            int iZ = Mathf.FloorToInt(randomZ / w);

            Debug.Log($"[initialSample] iX: {iX}, iZ: {iZ}");

            Vector2 randomPos = new Vector2(randomX, randomZ);
            grid[iX, iZ] = randomPos;
            active.Add(randomPos);

            // Choose random sample from active list while not empty
            while (active.Count > 0)
            {
                int randomIndex = Random.Range(0, active.Count);
                bool foundSample = false;

                for (int i = 0; i < k; i++)
                {
                    float randomAngle = Random.Range(0.0f, 2.0f*Mathf.PI);
                    float randomLength = Random.Range(r, 2.0f*r);
                    float offX = Mathf.Cos(randomAngle) * randomLength;
                    float offZ = Mathf.Sin(randomAngle) * randomLength;


                    Vector2 newSample = active[randomIndex] + (new Vector2(offX, offZ));

                    int newSampleIX = Mathf.FloorToInt(newSample.x / w);
                    int newSampleIZ = Mathf.FloorToInt(newSample.y / w);

                    // Only continue if new sample lays in grid
                    if (newSampleIX < 0 || newSampleIX > d - 1 || newSampleIZ < 0 || newSampleIZ > d - 1) continue;

                    // Debug.Log($"[newSample] d: {d}, ({newSampleIX},{newSampleIZ})");

                    // Check if newly generated sample is within distance r of existing samples
                    bool isValid = true;
                    for (int z = -2; z < 2; z++)
                    {
                        for (int x = -2; x < 2; x++)
                        {
                            // Check if neighbor lays in grid
                            int neighborIX = newSampleIX + x;
                            int neighborIZ = newSampleIZ + z;

                            if (neighborIX < 0 || neighborIX > d - 1 || neighborIZ < 0 || neighborIZ > d - 1) continue;

                            Vector2 neighborSample = grid[neighborIX, neighborIZ];

                            // Check if neighbor is a valid sample
                            if (neighborSample == Vector2.negativeInfinity) continue;

                            float distance = Vector2.Distance(newSample, neighborSample);
                            if (distance < r)
                            {
                                isValid = false;
                            }
                        }
                    }

                    // Accept new sample if it is valid
                    if (isValid)
                    {
                        foundSample = true;
                        grid[newSampleIX, newSampleIZ] = newSample;
                        active.Add(newSample);

                        // break; // TODO: Check if we can break here
                    }
                }

                // Remove sample from active list if no valid sample is found
                if (!foundSample)
                    active.RemoveAt(randomIndex);

            
        }
        */

            // Offset each tree randomly inside it's cell to break the visible grid pattern
            /*
            float gridOffset = 0.5f * SettingsManager.Instance.ChunkSettings.gridCellSize;
            float randomOffsetX = Random.Range(0.0f, gridOffset);
            float randomOffsetZ = Random.Range(0.0f, gridOffset);
            */


            // ... Do plausability check for each plant separately ... //
            // TODO
            // Do not place vegetation below water level
            // if (y < SettingsManager.Instance.MeshSettings.waterLevel) return;
            // Do not place vegetation above certain height
            // TODO

                
            // InstantiateTree(new Vector3(centerX + randomOffsetX, y, centerZ + randomOffsetZ));
        }

        private void InstantiateTree(Vector3 pos, Vector3 scale, string type)
        {
            // Add tree to instanced rendering queue
            if (type == "broadleaf")
            {
                broadleafTreeMatrices.Add(
                                Matrix4x4.TRS(
                                    pos,                                                        // Translation
                                    Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f),   // Rotation
                                    scale                                                       // Scale
                                )
                            );
            }
            else if (type == "pine")
            {
                pineTreeMatrices.Add(
                                Matrix4x4.TRS(
                                    pos,                                                        // Translation
                                    Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f),   // Rotation
                                    scale                                                       // Scale
                                )
                            );
            }
        }

        public void RenderTrees()
        {
            RenderTreeType(broadleafTreeMaterial, broadleafTreeMatrices);
            RenderTreeType(pineTreeMaterial, pineTreeMatrices);
        }

        public void RenderTreeType(Material material, List<Matrix4x4> matrices)
        {
            // Check if trees can be rendered in a single instanced draw call
            if (matrices.Count <= 1023)
            {
                Graphics.DrawMeshInstanced(
                    treeMesh,                   // mesh
                    0,                          // submeshIndex
                    material,               // material
                    matrices.ToArray(),     // matrices
                    matrices.Count          // count
                );

                return;
            }

            // Otherwise draw meshes instanced in batches of 1023 (which is the max count)
            for (int i = 0; i < Mathf.CeilToInt((float)matrices.Count / 1023); i++)
            {
                int startIndex = i * 1023;
                int count = Mathf.Min(broadleafTreeMatrices.Count - startIndex, 1023);

                //Debug.Log($"{treeMatrices.Count} - {startIndex} = {count}");

                Matrix4x4[] currentBatchTreeMatrices = matrices.GetRange(startIndex, count).ToArray();

                Graphics.DrawMeshInstanced(
                    treeMesh,                   // mesh
                    0,                          // submeshIndex
                    material,                   // material
                    currentBatchTreeMatrices,   // matrices
                    count                       // count
                );
            }
        }
    }
}
