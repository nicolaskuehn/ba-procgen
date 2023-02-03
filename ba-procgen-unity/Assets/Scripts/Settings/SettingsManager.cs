using UnityEngine;
using System.Collections.Generic;
using ProcGen.Generation;

namespace ProcGen.Settings
{   
    public sealed class SettingsManager : MonoBehaviour
    {
        // Singleton setup (not thread-safe)
        public static SettingsManager Instance { get; private set; }
        private SettingsManager() { }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                Debug.LogError("Destroyed duplicate instance of SettingsManager (Singleton)!");
            }
            else
                Instance = this;
        }

        private void OnEnable()
        {
            Init();
        }

        public HeightfieldCompositor HeightfieldCompositor { get; private set; }

        private void Init()
        {
            HeightfieldCompositor = new HeightfieldCompositor();
            // Add one octave with diamond-square algorithm as generation method per default
            HeightfieldCompositor.AddOctave(new Octave());
        }

        // ... User Settings ... //
        // Determines the resolution (number of tiles/squares) of the mesh of the plane in x- and z-direction
        [SerializeField, Range(2, 250), Tooltip("Resolution of the mesh in x- and z-direction")]
        public int resolution = 25; // TODO: split current resolution functionality in: resolution and size (currently size scales with resolution)
    }
}
