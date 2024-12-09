using Com.Kooply.Unity.Services;
using UnityEditor;
using UnityEngine;

namespace Editor
{
#if UNITY_EDITOR
    public static class Shortcuts
    {
        [MenuItem("Tools/Config")]
        public static void ShowConfig()
        {
            var path = Application.productName == "Sugar Rush" ? "Assets/Scripts/_INFRA_SPECIFICS/Config.Active.cs" : "Assets/KooplyRun/Scripts/Configuration/ActiveTestsConfig.cs";
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(path));
        }

        [MenuItem("Kooply/Toggle Admin Debug Text")]
        public static void ToggleAdminDebugText()
        {
            KCoreServices.Admin.ToggleAdminDebugText();
        }

        [MenuItem("Window/Toggle Simulator")]
        public static void ToggleSimulator()
        {
            var editorWindow = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in editorWindow)
            {
                if (window.titleContent.text.StartsWith("Simulator"))
                    window.maximized = !window.maximized;
            }
        }
        
        [MenuItem("Kooply/Toggle Screenshot Mode")]
        public static void ToggleScreenshotMode()
        {
            KCoreServices.Admin.ScreenshotModeState.Value = !KCoreServices.Admin.ScreenshotModeState;
        }
    }
#endif
}