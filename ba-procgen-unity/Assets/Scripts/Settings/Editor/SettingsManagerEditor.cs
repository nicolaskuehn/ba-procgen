using UnityEngine;
using UnityEditor;
using ProcGen.Generation;
using System.Collections.Generic;
using System;

namespace ProcGen.Settings
{
    [CustomEditor(typeof(SettingsManager))]
    public class SettingsManagerEditor : Editor
    {
        // Key: Octave Number (int), Value: Generation Method (Octave.EGenerationMethod)
        private Dictionary<Guid, Octave.EGenerationMethod> octaveGenerationMethods = new Dictionary<Guid, Octave.EGenerationMethod>();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // Get mesh generator from scene
            MeshGenerator meshGenerator = (MeshGenerator)FindObjectOfType(typeof(MeshGenerator));

            // ... Noise Settings ... //
            UpdateNoiseSettings();

            // ... Mesh Settings ... //

            // Draw heading
            EditorGUILayout.LabelField("Mesh Settings", EditorStyles.boldLabel);

            // Build button for generating the mesh
            if (GUILayout.Button("Generate Mesh"))
            {
                // Update generation methods of octaves
                foreach (Octave octave in SettingsManager.Instance.HeightfieldCompositor.Octaves)
                    octave.GenerationMethod = octaveGenerationMethods[octave.id];

                // Generate mesh
                meshGenerator.GenerateMesh();
            }
        }

        public void UpdateNoiseSettings()
        {
            // Draw heading
            EditorGUILayout.LabelField("Noise Settings", EditorStyles.boldLabel);

            // Octaves
            DrawOctaveSettings();

            // "Add octave" button
            if (GUILayout.Button("[+]   Add octave"))
            {
                SettingsManager.Instance.HeightfieldCompositor.AddOctave(new Octave());
                DrawOctaveSettings();
            }
        }

        private void DrawOctaveSettings()
        {
            // Build settings for each octave of heightfield compositor
            for (int i = 0; i < SettingsManager.Instance.HeightfieldCompositor.Octaves.Count; i++)
            {
                Octave currentOctave = SettingsManager.Instance.HeightfieldCompositor.Octaves[i];

                // Register generation method of octave
                if (!octaveGenerationMethods.ContainsKey(currentOctave.id))
                    octaveGenerationMethods.Add(currentOctave.id, currentOctave.GenerationMethod);

                GUILayout.BeginHorizontal();

                // Draw heading
                EditorGUILayout.LabelField($"Octave {i}", EditorStyles.helpBox);

                // "Remove octave" button
                if (GUILayout.Button("[-]   Remove octave"))
                {
                    SettingsManager.Instance.HeightfieldCompositor.RemoveOctave(currentOctave);
                    DrawOctaveSettings();
                }

                GUILayout.EndHorizontal();

                // Build EGenerationMethod dropdown selection for each octave
                octaveGenerationMethods[currentOctave.id] = (Octave.EGenerationMethod) EditorGUILayout.EnumPopup("Generation method:", octaveGenerationMethods[currentOctave.id]);

                // Build settings for the heightfield generator of the currently selected octave
                for (int j = 0; j < currentOctave.HeightfieldGenerator.Settings.Count; j++)
                {
                    Setting setting = currentOctave.HeightfieldGenerator.Settings[j];
                    
                    // Display numbers as sliders and string as input field
                    if (setting.Type == typeof(int))    // TODO: use isnumeric instead (to catch float, double etc. aswell) --> switch-case?
                        setting.Value = EditorGUILayout.IntSlider(setting.Name, (int)setting.Value, 0, 20); // TODO: Assignment necessary? How to define range?
                    else if (setting.Type == typeof(float))
                        setting.Value = EditorGUILayout.Slider(setting.Name, (float)setting.Value, 0, 20.0f);
                    else if (setting.Type == typeof(string))
                    {
                        setting.Value = EditorGUILayout.TextField(setting.Name, setting.Value.ToString());
                    }
                }

                // Separator line
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }
        }
    }
}
