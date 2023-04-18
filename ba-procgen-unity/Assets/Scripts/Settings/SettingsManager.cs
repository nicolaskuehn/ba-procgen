using UnityEngine;
using ProcGen.Generation;

namespace ProcGen.Settings
{   
    public sealed class SettingsManager : MonoBehaviour
    {
        // Singleton setup (not thread-safe)
        private static SettingsManager instance;
        public static SettingsManager Instance
        {
            get => instance = instance != null ? instance : FindAnyObjectByType<SettingsManager>()?.Init();
            private set => instance = value;
        }
        private SettingsManager() { }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
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

        // Returns the initialized SettingsManager instance
        private SettingsManager Init()
        {
            HeightfieldCompositor = new HeightfieldCompositor();
            // Add one octave with diamond-square algorithm as generation method per default
            HeightfieldCompositor.AddOctave(new Octave());

            return this;
        }

        // ... User Settings ... //

        // TODO: Use new class -> [Serializable] MeshSettings and SerializedProperties instead of simple variables (to access these in SettingsManagerEditor)

        [SerializeField]
        private MeshSettings meshSettings;
        public MeshSettings MeshSettings => meshSettings;

        [SerializeField]
        private ChunkSettings chunkSettings;
        public ChunkSettings ChunkSettings => chunkSettings;

        /*
        [Header("Mesh Settings")]
        // Determines the size (number of tiles/squares) of the mesh of the plane in x- and z-direction
        [Range(2, 100), Tooltip("Size of the mesh in x- and z-direction (in whole units)")]
        public int size = 32;

        // Determines the resolution (subdivisions of tiles/squares) of the mesh of the plane in x- and z-direction
        [Range(1, 7), Tooltip("Resolution of the mesh in x- and z-direction (per unit)")]
        public int subdivisions = 6;

        // Determines the offset in y-direction
        [Range(-1, 1), Tooltip("Offset in y-direction (height). 0 corresponds to a centered height distribution.")]
        public float offset = 0;

        [Tooltip("Determines if the mesh updates automatically with every change in the settings menu")]
        public bool autoUpdate = false;
        */
    }

    [System.Serializable]
    public class MeshSettings
    {
        // Determines the size (number of tiles/squares) of the mesh of the plane in x- and z-direction
        //[Range(2, 100), Tooltip("Size of the mesh in x- and z-direction (in whole units)")]
        public int size = 32;

        // Determines the resolution (subdivisions of tiles/squares) of the mesh of the plane in x- and z-direction
        //[Range(1, 7), Tooltip("Resolution of the mesh in x- and z-direction (per unit)")]
        public int subdivisions = 6;

        // Determines the offset in y-direction
        //[Range(-1, 1), Tooltip("Offset in y-direction (height). 0 corresponds to a centered height distribution.")]
        public float offset = 0;

        //[Tooltip("Determines if the mesh updates automatically with every change in the settings menu")]
        public bool autoUpdate = false;

        // Can be toggled to show and hide the water plane
        public bool showWater = true;

        // Level of the water plane
        public float waterLevel = 0.0f;
    }
    

    [System.Serializable]
    public class ChunkSettings
    {
        // Determines the size of each grid cell in the chunk
        public int gridCellSize = 1;

        // Can be toggled to visualize the chunks/bounds of the chunks
        public bool drawChunkBounds = false;

        // Can be toggled to visualize the grid (cells) of the chunks
        public bool drawChunkGrid = false;
    } 

}
