using ProcGen.Settings;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProcGen.Generation
{
    [ExecuteInEditMode]
    public class VegetationDistributor : MonoBehaviour
    {
        // TODO: Add tree struct with (mesh, material, matrices) for multiple tree types
        [SerializeField]
        private Mesh treeMesh;
        [SerializeField]
        private Material treeMaterial;
        private List<Matrix4x4> treeMatrices = new List<Matrix4x4>();

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
                for (int z = 0; z < chunk.GridCellCount1D; z++)
                {
                    for (int x = 0; x < chunk.GridCellCount1D; x++)
                    {
                        // Reset model placement marker of grid cell
                        chunk.GetGridCellDataAtCoords(x, z).HasModelPlaced = false;
                    }
                }
            }
        }

        // Iterate over terrain and add vegetation
        public void DistributeVegetationOnTerrain()
        {
            // ... Setup ... //
            // Reset vegetation system state
            ResetVegetationState();

            // Get all chunks
            List<Transform> terrainChunks = terrainGO.transform.Cast<Transform>().ToList();


            // ...  Calculate distribution probabilities ... //
            // Iterate over each grid cell chunk by chunk
            foreach (Transform chunkTransform in terrainChunks)
            {
                Chunk chunk = chunkTransform.GetComponent<Chunk>();

                for (int i = 0; i < chunk.Grid.Length; i++)
                {
                    chunk.GetGridCellDataAtIndex(i).DistributionProbability = 1.0f;
                }
            }



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

                        EvaluateVegetation(
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

        private void EvaluateVegetation(float x, float z, GridCellData cellData)
        {
            // Return if a model is already placed in this grid cell
            if (cellData.HasModelPlaced) return;

            // Calculate height for given position (x, z)
            float y = SettingsManager.Instance.HeightfieldCompositor.GetComposedHeight(x, z);


            // ... Logic when to place what here ... //

            // Do not place vegetation below water level
            if (y < SettingsManager.Instance.MeshSettings.waterLevel) return; 

            // Decide if vegetation is placed based on the distribtution probability
            if (Random.value <= cellData.DistributionProbability)
            {
                // Offset each tree randomly inside it's cell to break the visible grid pattern
                float gridOffset = 0.5f * SettingsManager.Instance.ChunkSettings.gridCellSize;
                float randomOffsetX = Random.Range(0.0f, gridOffset);
                float randomOffsetZ = Random.Range(0.0f, gridOffset);

                // Spawn tree
                InstantiateTree(new Vector3(x + randomOffsetX, y, z + randomOffsetZ));
                cellData.HasModelPlaced = true;
            }
        }

        private void InstantiateTree(Vector3 pos)
        {
            // Add tree to instanced rendering queue
            treeMatrices.Add(
                Matrix4x4.TRS(
                    pos,                        // Translation
                    Quaternion.identity,        // Rotation
                    Vector3.one * 0.03f         // Scale
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
