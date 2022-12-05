using UnityEngine;

namespace ProcGen.Settings
{
    // Singleton (not thread-safe)
    public sealed class SettingsManager : MonoBehaviour
    {
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

        // ... User Settings ... //
        // Determines the resolution (number of tiles/squares) of the mesh of the plane in x- and z-direction
        [SerializeField, Range(2, 250), Tooltip("Resolution of the mesh in x- and z-direction")]
        public int resolution = 10;

    }
}
