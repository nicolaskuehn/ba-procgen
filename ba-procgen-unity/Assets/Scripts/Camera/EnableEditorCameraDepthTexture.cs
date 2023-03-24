using UnityEditor;
using UnityEngine;

namespace ProcGen.Camera
{
    [InitializeOnLoad]
    public class EnableEditorCameraDepthTexture : MonoBehaviour
    {
        static EnableEditorCameraDepthTexture()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            UnityEngine.Camera editorCamera = sceneView.camera;

            if (editorCamera != null)
                editorCamera.depthTextureMode = DepthTextureMode.Depth;
        }

        // TODO: only do this initially
    }
}
