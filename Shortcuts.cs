using UnityEditor;
using UnityEngine;

namespace Editor
{
#if UNITY_EDITOR
    public static class Shortcuts
    {
        static Shortcuts()
        {
            
        }
        
        [MenuItem("Kooply/Config...")]
        public static void ShowGameConfigService()
        {
            Selection.activeGameObject = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/ConfiguredResources/GameSpecific/GameServicesPrefabs/ConfigService.prefab");
        }
    }
#endif
}