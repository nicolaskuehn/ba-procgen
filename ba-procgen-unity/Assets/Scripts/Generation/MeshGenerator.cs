using UnityEngine;
using ProcGen.Settings;

namespace ProcGen.Generation
{
    public class MeshGenerator : MonoBehaviour
    {
        // ... Components ... //

        // Terrain
        private GameObject terrainGO;

        private Mesh terrainMesh;
        private Material _terrainMeshMaterial;
        private Material terrainMeshMaterial 
            => _terrainMeshMaterial == null 
            ? (_terrainMeshMaterial = new Material(Shader.Find("Standard"))) 
            : _terrainMeshMaterial;
       
        private MeshFilter _terrainMeshFilter;
        private MeshFilter terrainMeshFilter 
            => _terrainMeshFilter == null
            ? (_terrainMeshFilter = terrainGO.GetComponent<MeshFilter>())
            : _terrainMeshFilter;

        private MeshRenderer _terrainMeshRenderer;
        private MeshRenderer terrainMeshRenderer
            => _terrainMeshRenderer == null
            ? (_terrainMeshRenderer = terrainGO.GetComponent<MeshRenderer>())
            : _terrainMeshRenderer;


        // Water
        private GameObject waterGO;

        private Mesh waterMesh;
        [SerializeField]
        private Material waterMeshMaterial;

        private MeshFilter _waterMeshFilter;
        private MeshFilter waterMeshFilter
            => _waterMeshFilter == null
            ? (_waterMeshFilter = waterGO.GetComponent<MeshFilter>())
            : _waterMeshFilter;

        private MeshRenderer _waterMeshRenderer;
        private MeshRenderer waterMeshRenderer
            => _waterMeshRenderer == null
            ? (_waterMeshRenderer = waterGO.GetComponent<MeshRenderer>())
            : _waterMeshRenderer;

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            // ... Init child game objects that hold the different meshes ... //
            // Terrain
            if (terrainGO == null)
            {
                terrainGO = new GameObject("Terrain");

                terrainGO.AddComponent<MeshFilter>();
                terrainGO.AddComponent<MeshRenderer>();
                
                terrainGO.transform.parent = transform;
            }

            // Water
            if (waterGO == null)
            {
                waterGO = new GameObject("Water");

                waterGO.AddComponent<MeshFilter>();
                waterGO.AddComponent<MeshRenderer>();

                waterGO.transform.parent = transform;
            }
        }

        // Generates a 2D grid mesh of a plane
        public void GenerateTerrainMesh()
        {
            // Get mesh resolution from SettingsManager
            int size = SettingsManager.Instance.MeshSettings.size;
            int subdivisions = SettingsManager.Instance.MeshSettings.subdivisions;

            // Mesh data
            int vertexCount1D = 1 << subdivisions + 1;
            Vector3[] vertices = new Vector3[vertexCount1D * vertexCount1D];
            int faceCount = vertexCount1D - 1;
            int[] triangleIndices = new int[2 * faceCount * faceCount * 3];

            // Factor used to position the vertices to match the given size
            float sizeScalingFactor = (float)size / vertexCount1D;

            // Create vertices
            Vector3 vertexPos = Vector3.zero;
            for(int j = 0; j < vertexCount1D; j++)
            {
                for(int i = 0; i < vertexCount1D; i++)
                {
                    float x = i * sizeScalingFactor;
                    float z = j * sizeScalingFactor;

                    vertexPos.x = x;
                    vertexPos.z = z;
                    // Assign value of heightfield to y-position
                    vertexPos.y = SettingsManager.Instance.HeightfieldCompositor.GetComposedHeight(x, z);
                    
                    vertices[j * vertexCount1D + i] = vertexPos;
                }
            }

            // Create triangles (indices)
            int triangleIndex = 0;

            for (int z = 0; z < faceCount; z++)
            {
                for (int x = 0; x < faceCount; x++)
                {
                    // Vertices of the current square 
                    int bl = z * vertexCount1D + x;                 // bottom left
                    int tl = (z + 1) * vertexCount1D + x;           // top left
                    int tr = (z + 1) * vertexCount1D + x + 1;       // top right
                    int br = z * vertexCount1D + x + 1;             // bottom right

                    // Lower left triangle
                    triangleIndices[triangleIndex] = bl;
                    triangleIndices[triangleIndex + 1] = tl;
                    triangleIndices[triangleIndex + 2] = br;

                    // Upper right triangle
                    triangleIndices[triangleIndex + 3] = br;
                    triangleIndices[triangleIndex + 4] = tl;
                    triangleIndices[triangleIndex + 5] = tr;

                    triangleIndex += 6;
                }
            }

            // Create new mesh
            terrainMesh = new Mesh
            {
                // Assign vertices and triangles data to mesh
                vertices = vertices,
                triangles = triangleIndices
            };

            // Recalculate normals and tangents to ensure correct shading
            terrainMesh.RecalculateNormals();
            terrainMesh.RecalculateTangents();

            // Assign mesh to mesh filter
            terrainMeshFilter.sharedMesh = terrainMesh;

            // Assign material to mesh renderer
            terrainMeshRenderer.material = terrainMeshMaterial;
        }

        public void GenerateWaterMesh(int level)
        {
            // Create vertices
            Vector3[] vertices = new Vector3[4];
            vertices[0] = new Vector3(0, level, 0);
            vertices[1] = new Vector3(0, level, SettingsManager.Instance.MeshSettings.size);
            vertices[2] = new Vector3(SettingsManager.Instance.MeshSettings.size, level, SettingsManager.Instance.MeshSettings.size);
            vertices[3] = new Vector3(SettingsManager.Instance.MeshSettings.size, level, 0);

            // Create triangles (indices)
            int[] triangleIndices = new int[6];
            triangleIndices[0] = 0;
            triangleIndices[1] = 1;
            triangleIndices[2] = 2;
            triangleIndices[3] = 3;
            triangleIndices[4] = 0;
            triangleIndices[5] = 2;

            // Create new mesh
            waterMesh = new Mesh
            {
                // Assign vertices and triangles data to mesh
                vertices = vertices,
                triangles = triangleIndices
            };

            // Recalculate normals and tangents to ensure correct shading
            waterMesh.RecalculateNormals();
            waterMesh.RecalculateTangents();

            // Assign mesh to mesh filter
            waterMeshFilter.sharedMesh = waterMesh;

            // Assign material to mesh renderer
            waterMeshRenderer.material = waterMeshMaterial;
        }

        public void DisplayWaterMesh(bool display)
        {
            waterGO.SetActive(display);
        }
    }
}
