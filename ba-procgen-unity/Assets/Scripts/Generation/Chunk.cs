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

        public GridCellData[] Grid { get; private set; }
        public float GridCellSize => SettingsManager.Instance.ChunkSettings.gridCellSize;
        public int GridCellCount1D { get; private set; }


        private void Reset()
        {
            Init();
        }

        public void Init()
        {
            // Only initialize grid if it has not been initialized yet
            if (Grid != null) return;

            // Init grid
            const int MAX_SUBDIVISIONS_PER_MESH = 7;
            int subdivisions = SettingsManager.Instance.MeshSettings.subdivisions;
            int size = SettingsManager.Instance.MeshSettings.size;

            chunkSize = subdivisions < MAX_SUBDIVISIONS_PER_MESH ? size 
                : size / (1 << (subdivisions - MAX_SUBDIVISIONS_PER_MESH));   // size / chunkCount1D


            GridCellCount1D = Mathf.FloorToInt(chunkSize / GridCellSize);
            int gridCellsCount = GridCellCount1D * GridCellCount1D;
            Grid = new GridCellData[gridCellsCount];

            for (int i = 0; i < gridCellsCount; i++)
            {
                Grid[i] = new GridCellData();
            }
        }

        public GridCellData GetGridCellDataAtIndex(int index) => Grid[index];

        public GridCellData GetGridCellDataAtCoords(int x, int z) => Grid[z * (GridCellCount1D - 1) + x];

        public Vector2 GridCoordsToWorld(int x, int z) => new Vector2(transform.position.x + x * GridCellSize, transform.position.z + z * GridCellSize);

        // Render gizmos
        private void OnDrawGizmos()
        {
            if (SettingsManager.Instance.ChunkSettings.drawChunkBounds)
                DrawBounds();

            if (SettingsManager.Instance.ChunkSettings.drawChunkGrid)
                DrawGrid();
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
            // Only draw gizmos if the chunk's grid is initialized
            if (Grid == null) return;

            Gizmos.color = Color.cyan;

            for (int y = 0; y < GridCellCount1D; y++)
            {
                for (int x = 0; x < GridCellCount1D; x++)
                {
                    float offset = GridCellSize;
                    float halfOffset = offset / 2.0f;
                    Vector3 cubeDimensions = new Vector3(GridCellSize, GridCellSize, GridCellSize);
                    Gizmos.DrawWireCube(transform.position + new Vector3(x * offset + halfOffset, 1.0f, y * offset + halfOffset), cubeDimensions);  // TODO: Optimize by caching vector
                }
            }

            Gizmos.color = Color.white;
        }

    }

    public class GridCellData
    {
        public float SoilFertility { get; set; }
        public float Climate { get; set; }
        public bool HasModelPlaced { get; set; }

        public float ExpectedValue { get; set; }
        public float StandardDeviation { get; set; }

        public GridCellData (float soilFertility = 0.0f, float climate = 0.0f, bool hasModelPlaced = false)
        {
            // Biom data
            SoilFertility = soilFertility;
            Climate = climate;

            // Stochastic data
            ExpectedValue = 0.0f;
            StandardDeviation = 0.0f;

            // Miscellaneous data
            HasModelPlaced = hasModelPlaced;
        }

        public override string ToString() => $"Biom(SF: {SoilFertility}, C: {Climate}) - Stochastic(E: {ExpectedValue}, s: {StandardDeviation}) - Misc(MP: {HasModelPlaced})";
    }
}
