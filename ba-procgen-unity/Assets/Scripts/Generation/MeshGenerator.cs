using UnityEngine;
using ProcGen.Settings;

namespace ProcGen.Generation
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MeshGenerator : MonoBehaviour
    {
        // ... Components ... //
        private Mesh mesh;
        private Material _meshMaterial;
        private Material meshMaterial 
            => _meshMaterial == null 
            ? (_meshMaterial = new Material(Shader.Find("Legacy Shaders/Diffuse"))) 
            : _meshMaterial;
       
        private MeshFilter _meshFilter;
        private MeshFilter meshFilter 
            => _meshFilter == null
            ? (_meshFilter = GetComponent<MeshFilter>())
            : _meshFilter;

        private MeshRenderer _meshRenderer;
        private MeshRenderer meshRenderer
            => _meshRenderer == null
            ? (_meshRenderer = GetComponent<MeshRenderer>())
            : _meshRenderer;

        // Generates a 2D grid mesh of a plane
        public void GenerateMesh()
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
            mesh = new Mesh
            {
                // Assign vertices and triangles data to mesh
                vertices = vertices,
                triangles = triangleIndices
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
