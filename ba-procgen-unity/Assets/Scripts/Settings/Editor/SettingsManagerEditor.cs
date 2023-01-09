using UnityEngine;
using UnityEditor;
using ProcGen.Generation;
using System;

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

            // Build settings for the currently selected heightfield generator
            foreach (Setting setting in SettingsManager.Instance.HeightfieldGenerator.Settings)
            {
                // Display numbers as sliders and string as input field
                if (setting.Type == typeof(int))    // TODO: use isnumeric instead (to catch float, double etc. aswell)
                    setting.Value = EditorGUILayout.IntSlider(setting.Name, (int) setting.Value, 0, 100); // TODO: Assignment necessary? How to define range?
                else if (setting.Type == typeof(string))
                {
                    setting.Value = EditorGUILayout.TextField(setting.Name, setting.Value.ToString());
                }
            }
        }
    }
}
