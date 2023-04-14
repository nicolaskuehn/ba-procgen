using ProcGen.Settings;
using System.Collections.Generic;
using UnityEngine;

namespace ProcGen.Generation
{
    public class Chunk : MonoBehaviour
    {
        // ... Attributes ... //
        private List<int> grid = new List<int>();
        private int gridResolution;

        private void Awake()
        {
            gridResolution = SettingsManager.Instance.ChunkSettings.gridResolution;
        }

    }
}
