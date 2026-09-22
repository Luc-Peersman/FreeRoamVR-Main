using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace FreeRoamVR.EditorTools
{
    // A fresh git clone has no Library folder, so Unity has no "last opened scene" to
    // restore and opens an empty Untitled scene instead. This opens SampleScene instead,
    // once per editor session (not on every script recompile).
    [InitializeOnLoad]
    internal static class ClaudeOpenSampleSceneOnLoad
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string SessionKey = "FreeRoamVR.OpenedSampleSceneThisSession";

        static ClaudeOpenSampleSceneOnLoad()
        {
            EditorApplication.delayCall += OpenIfUntitled;
        }

        private static void OpenIfUntitled()
        {
            if (SessionState.GetBool(SessionKey, false))
            {
                return;
            }

            SessionState.SetBool(SessionKey, true);

            if (!string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path))
            {
                return;
            }

            if (!File.Exists(ScenePath))
            {
                return;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
    }
}
