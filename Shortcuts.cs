using UnityEditor;

namespace Editor
{
#if UNITY_EDITOR
    public static class Shortcuts
    {
        private const string UseMonoScriptingBackendMenuItem = "Kooply/Use Mono Scripting Backend";
        
        
        
        static Shortcuts()
        {
            Menu.SetChecked(UseMonoScriptingBackendMenuItem, GetCurrentScriptingBackend() == ScriptingImplementation.Mono2x);
        }

        [MenuItem(UseMonoScriptingBackendMenuItem)]
        public static void SetScriptingBackend()
        {
            var isUsingMono = GetCurrentScriptingBackend() == ScriptingImplementation.Mono2x;
            PlayerSettings.SetScriptingBackend(GetCurrentBuildTargetGroup(),
                isUsingMono ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);

            Menu.SetChecked(UseMonoScriptingBackendMenuItem, !isUsingMono);
        }

        private static ScriptingImplementation GetCurrentScriptingBackend()
        {
            return PlayerSettings.GetScriptingBackend(GetCurrentBuildTargetGroup());
        }

        private static BuildTargetGroup GetCurrentBuildTargetGroup()
        {
            return BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        }
    }
#endif
}