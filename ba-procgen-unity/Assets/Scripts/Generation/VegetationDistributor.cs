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

        [SerializeField]
        private int tileGridResolution = 4;     // density has to be 

        private GameObject terrainGO;

        private void Awake()
        {
            Init();
        }

        private void Update()
        {
            return; // TODO: remove return
            // Render vegetation
            if (treeMatrices.Count > 0)
                RenderTrees();
        }

        public void Init()
        {
            // Get reference to game object that holds all terrain chunks (meshes)
            terrainGO = FindFirstObjectByType<MeshGenerator>().terrainGO;

            if (terrainGO == null)
                Debug.LogWarning("Could not find terrain game object in VegetationDistributor. Is there a MeshGenerator in the scene?");
        }

        // Iterate over terrain and add vegetation
        public void DistributeVegetationOnTerrain()
        {
            // Iterate over each chunk to handle them separately
            List<Transform> terrainChunks = transform.Cast<Transform>().ToList();
            foreach (Transform chunk in terrainChunks)
            {
                Vector3 center = chunk.position;    // origin of each tile is in the center
                int size = SettingsManager.Instance.MeshSettings.size;

                // Iterate over all grid cells in tile
                for (int z = Mathf.RoundToInt(center.z - size / 2.0f); z < Mathf.RoundToInt(center.z + size / 2.0f); z+= size / tileGridResolution)
                {
                    for (int x = Mathf.RoundToInt(center.x - size / 2.0f); x < Mathf.RoundToInt(center.x + size / 2.0f); x += size / tileGridResolution)
                    {
                        CalculateVegetation(x, z);
                    }
                }
            }
        }

        private void CalculateVegetation(float x, float z)   // TODO: Find better method name
        {
            // Logic when to place what here:
            if (Random.value < 0.1)     // DEBUG, TODO: Change
                InstantiateTree(x, z);
        }

        private void InstantiateTree(float x, float z)
        {
            float y = 10.0f; // TODO: calculate y pos

            // Add tree to instanced rendering queue
            treeMatrices.Add(
                Matrix4x4.TRS(
                    new Vector3(x, y, z),       // Translation
                    Quaternion.identity,        // Rotation
                    Vector3.one                 // Scale
                )
            );
        }

        public void RenderTrees()
        {
            // TODO: Render all instanced trees in queue
            Graphics.DrawMeshInstanced(
                    treeMesh,                   // mesh
                    0,                          // submeshIndex
                    treeMaterial,               // material
                    treeMatrices.ToArray(),     // matrices
                    treeMatrices.Count          // count
            );
        }
    }
}
