using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Editor.Private
{
    public class TextSearchWindow : EditorWindow
    {
        private string _searchText = string.Empty;
        private bool _foundOccurrence = false;

        [MenuItem("Tools/Text Search")]
        public static void ShowWindow()
        {
            GetWindow<TextSearchWindow>("Text Search");
        }

        private void OnGUI()
        {
            GUILayout.Label("Search Text in Scene Components", EditorStyles.boldLabel);
            _searchText = EditorGUILayout.TextField("Search Term", _searchText);

            if (GUILayout.Button("Search") && !string.IsNullOrWhiteSpace(_searchText))
            {
                SearchInAllAssets();
                if (!_foundOccurrence)
                {
                    // If after searching, no occurrence is found, log a message
                    Debug.LogError($"No occurrences of '{_searchText}' found in any project assets.");
                }
            }
        }

        private void SearchInAllAssets()
        {
            var allAssetGUIDs = AssetDatabase.FindAssets("");
            foreach (var guid in allAssetGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

                switch (asset)
                {
                    case GameObject gameObject: CheckGameObject(gameObject, assetPath);
                        break;
                    case ScriptableObject scriptableObject: CheckScriptableObject(scriptableObject, assetPath);
                        break;
                }
            }
        }

        private void CheckGameObject(GameObject obj, string assetPath)
        {
            CheckComponentsInChildren(obj, assetPath);
        }

        private void CheckComponentsInChildren(GameObject obj, string assetPath)
        {
            var components = obj.GetComponentsInChildren<Component>(true);
            foreach (var component in components)
            {
                if (component == null) continue; // Handle missing (broken) components gracefully

                var fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(string))
                    {
                        var value = field.GetValue(component) as string;
                        if (!string.IsNullOrEmpty(value) && value.Contains(_searchText))
                        {
                            Debug.LogError($"Found '{_searchText}' in {field.Name} of {component.GetType().Name} on {obj.name} in asset {assetPath}", obj);
                            _foundOccurrence = true;
                        }
                    }
                }
            }
        }

        private void CheckScriptableObject(ScriptableObject obj, string assetPath)
        {
            var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(string))
                {
                    var value = field.GetValue(obj) as string;
                    if (!string.IsNullOrEmpty(value) && value.Contains(_searchText))
                    {
                        Debug.LogError($"Found '{_searchText}' in {field.Name} of {obj.GetType().Name} in asset {assetPath}", obj);
                        _foundOccurrence = true;
                    }
                }
            }
        }
    }
}