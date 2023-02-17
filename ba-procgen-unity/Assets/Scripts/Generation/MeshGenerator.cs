using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProcGen.Settings;

namespace ProcGen.Generation
{
    public class MeshGenerator : MonoBehaviour
    {
        // ... Components ... //

        // Terrain
        private GameObject terrainGO;

        // private Mesh terrainMesh;
        [SerializeField]
        private Material terrainMeshMaterial;
       
        /*
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
        */


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


        // ... Properties ... //
        // Number of faces
        public static int FaceCount1D => 1 << SettingsManager.Instance.MeshSettings.subdivisions;

        // Number of vertices in one direction/dimension (mesh is a square, so all directions have the same length)
        public static int VertexCount1D => FaceCount1D + 1;

        // Factor used to position the vertices to match the given size
        public static float SizeScalingFactor => (float) SettingsManager.Instance.MeshSettings.size / FaceCount1D;


        // ... Constants ... //
        private const int MAX_SUBDIVISIONS_PER_MESH = 7;

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            // ... Init child game objects that hold the different meshes ... //

            // Only initialize if references do not exist
            if (transform.childCount > 0 && terrainGO != null && waterGO != null)
                return;

            // Delete old children first (to avoid duplicates)
            if (transform.childCount > 0)
            {
                List<Transform> tempChildList = transform.Cast<Transform>().ToList();
                foreach (Transform child in tempChildList)
                {
                    if (Application.isPlaying)
                        Destroy(child.gameObject);
                    else
                        DestroyImmediate(child.gameObject);
                }
            }
            
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

        public void GenerateTerrainMesh()
        {
            // Delete all mesh instances first
            List<Transform> tempChildList = terrainGO.transform.Cast<Transform>().ToList();
            foreach (Transform child in tempChildList)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }

            int size = SettingsManager.Instance.MeshSettings.size;
            int subdivisions = SettingsManager.Instance.MeshSettings.subdivisions;

            if (subdivisions > MAX_SUBDIVISIONS_PER_MESH)
            {
                int submeshCount1D = 1 << (subdivisions - MAX_SUBDIVISIONS_PER_MESH);
                int submeshSubdivisions = MAX_SUBDIVISIONS_PER_MESH;

                int scaledSize = size / submeshCount1D;

                Vector3 submeshOrigin = Vector3.zero;
                for (int j = 0; j < submeshCount1D; j++)
                {
                    for (int i = 0; i < submeshCount1D; i++)
                    {
                        // Add new game object for each submesh
                        submeshOrigin.x = i * scaledSize;
                        submeshOrigin.z = j * scaledSize;
                        CreateSubmeshGameObject(
                            terrainGO.transform, 
                            submeshOrigin, 
                            GenerateTerrainSubmesh(submeshOrigin, scaledSize, submeshSubdivisions), 
                            terrainMeshMaterial,  
                            j * submeshCount1D + i
                        );
                    }
                }

                // Averaging normals on submesh borders to avoid visible seams
                foreach (Transform submesh in terrainGO.transform)
                {
                    // TODO: Override normals at borders (average)
                    // submesh.GetComponent<MeshFilter>().sharedMesh.normals
                }
            }
            else
            {
                CreateSubmeshGameObject(terrainGO.transform, Vector3.zero, GenerateTerrainSubmesh(Vector3.zero, size, subdivisions), terrainMeshMaterial, 0);
            }
        }

        // Generates a 2D grid mesh of a plane
        public Mesh GenerateTerrainSubmesh(Vector3 origin, int size1D, int subdivisions)
        {
            // Mesh data
            //Vector3[] vertices = new Vector3[VertexCount1D * VertexCount1D];
            //int[] triangleIndices = new int[2 * FaceCount1D * FaceCount1D * 3];
            int localFaceCount1D = 1 << subdivisions;
            int localVertexCount1D = localFaceCount1D + 1;
            float localSizeScalingFactor = (float) size1D / localFaceCount1D;

            Vector3[] vertices = new Vector3[localVertexCount1D * localVertexCount1D];
            int[] triangleIndices = new int[2 * localVertexCount1D * localVertexCount1D * 3];

            // Create vertices
            Vector3 vertexPos = Vector3.zero;
            for(int j = 0; j < localVertexCount1D; j++)
            {
                for(int i = 0; i < localVertexCount1D; i++)
                {
                    float x = i * localSizeScalingFactor;
                    float z = j * localSizeScalingFactor;

                    vertexPos.x = x;
                    vertexPos.z = z;
                    // Assign value of heightfield to y-position
                    vertexPos.y = SettingsManager.Instance.HeightfieldCompositor.GetComposedHeight(x + origin.x, z + origin.z);
                    
                    vertices[j * localVertexCount1D + i] = vertexPos;
                }
            }

            // Create triangles (indices)
            //int triangleIndex = (startX + 2 * startZ) * localFaceCount1D * 6; // Quadrant index * Face count * 6 indeces per face (describing 2 triangles per face)
            int triangleIndex = 0;

            for (int z = 0; z < localFaceCount1D; z++)
            {
                for (int x = 0; x < localFaceCount1D; x++)
                {
                    // Vertices of the current square 
                    int bl = z * localVertexCount1D + x;                 // bottom left
                    int tl = (z + 1) * localVertexCount1D + x;           // top left
                    int tr = (z + 1) * localVertexCount1D + x + 1;       // top right
                    int br = z * localVertexCount1D + x + 1;             // bottom right

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

            // Create and return new mesh
            return new Mesh
            {
                // Assign vertices and triangles data to mesh
                vertices = vertices,
                triangles = triangleIndices
            };
        }

        public void GenerateWaterMesh(float level)
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

        public void SetWaterLevel(float level)
        {
            // Set water level in terrain material (for height dependant coloring)
            terrainGO.GetComponent<Renderer>().sharedMaterial.SetFloat("_WaterLevel", level);
        }

        public void DisplayWaterMesh(bool display)
        {
            waterGO.SetActive(display);
        }

        private GameObject CreateSubmeshGameObject(Transform parent, Vector3 pos, Mesh mesh, Material material, int index)
        {
            // Create new game object and add necessary components
            GameObject submesh = new GameObject($"Submesh {index}");
            submesh.transform.parent = parent;
            submesh.transform.position = pos;
            submesh.AddComponent<MeshFilter>();
            submesh.AddComponent<MeshRenderer>();

            // Recalculate normals and tangents to ensure correct shading
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            // Assign mesh to mesh filter
            submesh.GetComponent<MeshFilter>().sharedMesh = mesh;

            // Assign material to mesh renderer
            submesh.GetComponent<MeshRenderer>().material = material;

            return submesh;
        }
    }
}
