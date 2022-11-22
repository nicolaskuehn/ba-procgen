using UnityEngine;
using UnityEditor;

namespace ProcGen.Generation
{
    [CustomEditor(typeof(MeshGenerator))]
    public class MeshGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // Get script reference
            MeshGenerator meshGenerator = (MeshGenerator) target;

            // Only draw button in play mode
            if (!Application.isPlaying) return;
            
            // Build button for generating the mesh
            if (GUILayout.Button("Generate Mesh"))
            {
                meshGenerator.GenerateMesh();
            }
        }
    }
}
