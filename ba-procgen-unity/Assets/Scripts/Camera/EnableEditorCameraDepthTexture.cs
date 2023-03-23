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

        // TODO: 
        // - Rename script
        // - adjust camera view frustum to see if this fixes depth map resolution (only black and white)
        // - increase buffer size in project settings
    }
}
