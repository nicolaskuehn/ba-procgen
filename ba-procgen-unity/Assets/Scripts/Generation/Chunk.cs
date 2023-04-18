using ProcGen.Settings;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProcGen.Generation
{
    public class Chunk : MonoBehaviour
    {
        // ... Attributes ... //
        private int chunkSize;

        private GridCellData[] grid;
        private int gridCellSize;
        private int gridCellCount1D;

        public void Init()
        {
            // Init grid
            gridCellSize = SettingsManager.Instance.ChunkSettings.gridCellSize;

            const int MAX_SUBDIVISIONS_PER_MESH = 7;
            int subdivisions = SettingsManager.Instance.MeshSettings.subdivisions;
            int size = SettingsManager.Instance.MeshSettings.size;

            chunkSize = subdivisions < MAX_SUBDIVISIONS_PER_MESH ? size 
                : size / (1 << (subdivisions - MAX_SUBDIVISIONS_PER_MESH));   // size / chunkCount1D


            gridCellCount1D = chunkSize / gridCellSize;
            grid = new GridCellData[gridCellCount1D * gridCellCount1D];
        }

        public GridCellData GetGridCellDataAtIndex(int index) => grid[index];

        public GridCellData GetGridCellDataAtCoords(int x, int y) => grid[y * gridCellCount1D + x];

        // Gizmo renderer
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawGizmosForChunk(Chunk chunk, GizmoType gizmoType)
        {
            if (SettingsManager.Instance.ChunkSettings.drawChunkBounds)
                chunk.DrawBounds();

            if (SettingsManager.Instance.ChunkSettings.drawChunkGrid)
                chunk.DrawGrid();
        }

        // TODO: Set height of wired cube to average of the chunk
        private void DrawBounds()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + new Vector3(chunkSize / 2.0f, 0.0f, chunkSize / 2.0f), new Vector3(chunkSize, chunkSize, chunkSize));  // TODO: Optimize by caching vector
            Gizmos.color = Color.white;
        }

        // TODO: Set height of wired cube to: 1. average of cell (4 corners average) or 2. height of center of the cell
        private void DrawGrid()
        {
            Gizmos.color = Color.cyan;

            for (int y = 0; y < gridCellCount1D; y++)
            {
                for (int x = 0; x < gridCellCount1D; x++)
                {
                    float offset = gridCellSize;
                    float halfOffset = offset / 2.0f;
                    Vector3 cubeDimensions = new Vector3(gridCellSize, gridCellSize, gridCellSize);
                    Gizmos.DrawWireCube(transform.position + new Vector3(x * offset + halfOffset, 1.0f, y * offset + halfOffset), cubeDimensions);  // TODO: Optimize by caching vector
                }
            }

            Gizmos.color = Color.white;
        }

    }

    public struct GridCellData
    {
        public float DistributionProbability { get; } // TODO: Revise
        public bool HasModelPlaced { get; set; }

        public GridCellData (float distributionProbability = 0.0f, bool hasModelPlaced = false)
        {
            DistributionProbability = distributionProbability;
            HasModelPlaced = hasModelPlaced;
        }

        public override string ToString() => $"(DP: {DistributionProbability}, MP: {HasModelPlaced})";
    }
}
