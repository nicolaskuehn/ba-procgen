using UnityEngine;

namespace ProcGen.Generation
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MeshGenerator : MonoBehaviour
    {
        // ... Components ... //
        private Mesh mesh;
        private Material meshMaterial;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        // ... Settings ... //
        // Determines the resolution (number of tiles/squares) of the mesh of the plane in x- and z-direction
        [SerializeField, Range(2, 250), Tooltip("Resolution of the mesh in x- and z-direction")]
        int resolution = 10;

        private void Awake()
        {
            // Initialize components
            meshMaterial = new Material(Shader.Find("Legacy Shaders/Diffuse"));
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        // Generates a 2D grid mesh of a plane
        public void GenerateMesh()
        {
            // Mesh data
            int vertexCount1D = resolution + 1;
            Vector3[] vertices = new Vector3[vertexCount1D * vertexCount1D];
            int[] triangles = new int[2 * resolution * resolution * 3];

            // Create vertices
            Vector3 vertexPos = Vector3.zero;
            for(int z = 0; z < vertexCount1D; z++)
            {
                for(int x = 0; x < vertexCount1D; x++)
                {
                    vertexPos.x = x;
                    vertexPos.z = z;
                    vertices[z * vertexCount1D + x] = vertexPos;
                }
            }

            // Create triangles (indices)
            int triangleIndex = 0;
            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    // Vertices of the current square 
                    int bl = z * vertexCount1D + x;                 // bottom left
                    int tl = (z + 1) * vertexCount1D + x;           // top left
                    int tr = (z + 1) * vertexCount1D + x + 1;       // top right
                    int br = z * vertexCount1D + x + 1;             // bottom right

                    // Lower left triangle
                    triangles[triangleIndex] = bl;
                    triangles[triangleIndex + 1] = tl;
                    triangles[triangleIndex + 2] = br;

                    // Upper right triangle
                    triangles[triangleIndex + 3] = br;
                    triangles[triangleIndex + 4] = tl;
                    triangles[triangleIndex + 5] = tr;

                    triangleIndex += 6;
                }
            }

            // Create new mesh
            mesh = new Mesh
            {
                // Assign vertices and triangles data to mesh
                vertices = vertices,
                triangles = triangles
            };

            // Recalculate normals and tangents to ensure correct shading
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            // Assign mesh to mesh filter
            meshFilter.sharedMesh = mesh;

            // Assign material to mesh renderer
            meshRenderer.material = meshMaterial;
        }
    }
}
