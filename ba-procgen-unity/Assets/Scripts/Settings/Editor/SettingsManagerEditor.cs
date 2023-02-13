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
        private Dictionary<Guid, Texture2D> octaveTextures = new Dictionary<Guid, Texture2D>();

        // Constants
        private static int NOISE_TEXTURE_SIZE = 200;

        // Scene objects
        MeshGenerator meshGenerator;

        public void OnEnable()
        {
            // Get mesh generator from scene (when not already assigned)
            if (meshGenerator == null)
                meshGenerator = (MeshGenerator)FindObjectOfType(typeof(MeshGenerator));

            meshGenerator.Init();

            // Attach event listener to each setting of the default octave
            Octave defaultOctave = SettingsManager.Instance.HeightfieldCompositor.Octaves[0];
            foreach (Setting setting in defaultOctave.HeightfieldGenerator.Settings)
                setting.valueChanged.AddListener(delegate { UpdateOctave(defaultOctave); });
        }

        public override void OnInspectorGUI()
        {
            // DrawDefaultInspector();
            DrawEditor();
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
                Octave octave = new Octave();
                SettingsManager.Instance.HeightfieldCompositor.AddOctave(octave);
                
                foreach(Setting setting in octave.HeightfieldGenerator.Settings)
                    setting.valueChanged.AddListener(delegate { UpdateOctave(octave); });

                UpdateOctave(octave);
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
                        setting.Value = EditorGUILayout.IntSlider(setting.Name, (int)setting.Value, (int) setting.MinValue, (int) setting.MaxValue);
                    else if (setting.Type == typeof(float))
                        setting.Value = EditorGUILayout.Slider(setting.Name, (float)setting.Value, (float) setting.MinValue, (float) setting.MaxValue);
                    else if (setting.Type == typeof(string))
                    {
                        setting.Value = EditorGUILayout.TextField(setting.Name, setting.Value.ToString());
                    }
                }



                // Draw 2D noise texture of the generated noise as preview
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                if (octaveTextures.ContainsKey(currentOctave.id))
                    GUILayout.Box(octaveTextures[currentOctave.id]);
                else
                    Debug.LogError($"Texture for octave with id {currentOctave.id} could not be found.");
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();

                // Separator line
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }
        }

        private void UpdateOctaveTexture(Octave octave)
        {
            if (!octaveTextures.ContainsKey(octave.id))
                // Generate new texture and add it to the dictionary
                octaveTextures.Add(octave.id, TextureGenerator.GenerateTextureMap(NOISE_TEXTURE_SIZE, NOISE_TEXTURE_SIZE, octave));
                    else
                        // Re-generate texture of correspondinn octave
                        octaveTextures[octave.id] = TextureGenerator.GenerateTextureMap(NOISE_TEXTURE_SIZE, NOISE_TEXTURE_SIZE, octave);        }

        private void UpdateOctaveTextures()
        {
            foreach (Octave octave in SettingsManager.Instance.HeightfieldCompositor.Octaves)
                UpdateOctaveTexture(octave);
        }

        private void UpdateOctave(Octave octave)
        {
            UpdateOctaveTexture(octave);
            DrawOctaveSettings();

            if (SettingsManager.Instance.MeshSettings.autoUpdate)
                meshGenerator.GenerateTerrainMesh();
        }

        private void DrawMeshSettings()
        {
            // Draw heading
            EditorGUILayout.LabelField("Mesh Settings", EditorStyles.boldLabel);

            // TODO: Refactor - Iterate over all mesh settings dynamically
            int origSize = SettingsManager.Instance.MeshSettings.size;
            SettingsManager.Instance.MeshSettings.size = EditorGUILayout.IntSlider("Size", SettingsManager.Instance.MeshSettings.size, 2, 100);

            int origSubdivisions = SettingsManager.Instance.MeshSettings.subdivisions;
            SettingsManager.Instance.MeshSettings.subdivisions = EditorGUILayout.IntSlider("Subdivisions", SettingsManager.Instance.MeshSettings.subdivisions, 1, 7);

            float origOffset = SettingsManager.Instance.MeshSettings.offset;
            SettingsManager.Instance.MeshSettings.offset = EditorGUILayout.Slider("Offset", SettingsManager.Instance.MeshSettings.offset, -1.0f, 1.0f);

            bool origAutoUpdate = SettingsManager.Instance.MeshSettings.autoUpdate;
            SettingsManager.Instance.MeshSettings.autoUpdate = EditorGUILayout.Toggle("Auto-Update", SettingsManager.Instance.MeshSettings.autoUpdate);

            bool origShowWater = SettingsManager.Instance.MeshSettings.showWater;
            SettingsManager.Instance.MeshSettings.showWater = EditorGUILayout.Toggle("Show water", SettingsManager.Instance.MeshSettings.showWater);

            // Check if display of water has been changed
            if (origShowWater != SettingsManager.Instance.MeshSettings.showWater)
                meshGenerator.DisplayWaterMesh(SettingsManager.Instance.MeshSettings.showWater);
            
            // Check if values have changed to update mesh automatically (only if auto-update is enabled)
            if (SettingsManager.Instance.MeshSettings.autoUpdate)
            {
                bool updateTerrainMesh = false;
                bool updateWaterMesh = false;

                if (origSize != SettingsManager.Instance.MeshSettings.size)
                {
                    updateTerrainMesh = true;
                    updateWaterMesh = true;
                }

                if (origSubdivisions != SettingsManager.Instance.MeshSettings.subdivisions
                    || origOffset != SettingsManager.Instance.MeshSettings.offset
                )
                    updateTerrainMesh = true;

                if (!origAutoUpdate)
                    updateTerrainMesh = true;

                if (!origShowWater && SettingsManager.Instance.MeshSettings.showWater)
                    updateWaterMesh = true;

                // Update meshes if required
                if (updateTerrainMesh)
                    meshGenerator.GenerateTerrainMesh();

                if (updateWaterMesh && SettingsManager.Instance.MeshSettings.showWater)
                    meshGenerator.GenerateWaterMesh(SettingsManager.Instance.MeshSettings.waterLevel);
            }

            // Build button for generating the mesh (only if auto-update is disabled)
            if (!SettingsManager.Instance.MeshSettings.autoUpdate)
            {
                if (GUILayout.Button("Generate Mesh"))
                {
                    // Update generation methods of octaves
                    foreach (Octave octave in SettingsManager.Instance.HeightfieldCompositor.Octaves)
                        octave.GenerationMethod = octaveGenerationMethods[octave.id];

                    // Generate meshes
                    meshGenerator.GenerateTerrainMesh();
                    meshGenerator.GenerateWaterMesh(SettingsManager.Instance.MeshSettings.waterLevel);
                }
            }
        }

        private void DrawEditor()
        {
            // ... Mesh Settings ... //
            DrawMeshSettings();


            // ... Space ... //
            EditorGUILayout.Space();


            // ... Noise Settings ... //
            // Octave Textures
            if (octaveTextures.Count == 0)
                UpdateOctaveTextures();

            // Settings
            UpdateNoiseSettings();
        }
    }
}
