using UnityEngine;
using UnityEditor;
using ProcGen.Generation;

namespace ProcGen.Settings
{
    [CustomEditor(typeof(SettingsManager))]
    public class SettingsManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // Get mesh generator from scene
            MeshGenerator meshGenerator = (MeshGenerator)FindObjectOfType(typeof(MeshGenerator));

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
