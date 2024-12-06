using Com.Kooply.Unity.Services;
using Com.Kooply.Unity.Utils;
using UnityEditor;
using UnityEngine;

namespace Editor
{
#if UNITY_EDITOR
    public static class Shortcuts
    {
        [MenuItem("Kooply/ToggleAdminDebugText")]
        public static void ToggleAdminDebugText()
        {
            Keys.Register("admin_text", KeyCode.Q, KeyCode.LeftShift, () => KCoreServices.Admin.ToggleAdminDebugText());
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