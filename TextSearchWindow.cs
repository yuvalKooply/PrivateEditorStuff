using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Com.Kooply.Unity.ExtensionMethods;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Private
{
    public class TextSearchWindow : EditorWindow
    {
        private string _searchText = string.Empty;
        private bool _foundOccurrence;
        private bool _ignoreCase;

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
                _foundOccurrence = false;
                SearchInAllAssets();
                if (!_foundOccurrence)
                {
                    // If after searching, no occurrence is found, log a message
                    Debug.LogError($"No occurrences of '{_searchText}' found in any project assets.");
                }
            }

            _ignoreCase = GUILayout.Toggle(_ignoreCase, "Ignore Case", GUILayout.Width(100));
        }

        private void SearchInAllAssets()
        {
            var allAssetGUIDs = AssetDatabase.FindAssets("");
            foreach (var guid in allAssetGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

                if (asset == null)
                    continue;
                
                switch (asset)
                {
                    case GameObject gameObject:
                        SearchPrefabForText(gameObject, assetPath);
                        break;
                    case ScriptableObject scriptableObject:
                        SearchScriptableObjectForText(scriptableObject, assetPath);
                        break;
                }
            }
        }

        private void SearchPrefabForText(GameObject prefab, string assetPath)
        {
            if (_ignoreCase)
                _searchText = _searchText.ToLower();

            var visitedObjects = new HashSet<object>();
            var components = prefab.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (var component in components)
            {
                try
                {
                    if (component == null)
                        continue;

                    SearchObjectFields(component, component.gameObject, assetPath, visitedObjects);
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"Error during string search in prefab: '{prefab.name}' and game object: '{component.gameObject.name}'! Exception: {e}");
                }
            }
        }
        
        private void SearchScriptableObjectForText(ScriptableObject scriptableObject, string assetPath)
        {
            if (_ignoreCase)
                _searchText = _searchText.ToLower();

            var visitedObjects = new HashSet<object>();
            SearchObjectFields(scriptableObject, null, assetPath, visitedObjects);
        }

        private void SearchObjectFields(object obj, GameObject gameObject, string assetPath, HashSet<object> visitedObjects)
        {
            try
            {
                if (obj == null || !visitedObjects.Add(obj))
                    return;
            }
            catch (Exception)
            {
                // obj is probably null 
                return;
            }

            var type = obj.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                if (field == null || typeof(Object).IsAssignableFrom(field.FieldType))
                    continue;
                
                try
                {
                    var fieldValue = field.GetValue(obj);
                    if (fieldValue == null)
                        continue;

                    if (field.FieldType == typeof(string))
                    {
                        var stringValue = fieldValue as string;
                        if (!stringValue.IsNullOrEmpty())
                        {
                            if (_ignoreCase)
                                stringValue = stringValue.ToLower();

                            if (stringValue.Contains(_searchText))
                            {
                                Debug.LogError($"Found '{_searchText}' in {field.Name} of {type.Name} in asset {assetPath}");
                                _foundOccurrence = true;
                            }
                        }
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(field.FieldType) && field.FieldType != typeof(string))
                    {
                        if (fieldValue is IEnumerable enumerable)
                            foreach (var item in enumerable)
                                SearchObjectFields(item, gameObject, assetPath, visitedObjects);
                    }
                    else if (field.FieldType.IsClass || (field.FieldType.IsValueType && !field.FieldType.IsPrimitive))
                    {
                        if (fieldValue != null)
                            SearchObjectFields(fieldValue, gameObject, assetPath, visitedObjects);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error during string search in asset '{assetPath}': {e}");
                    
                    if (gameObject != null && field != null)
                        Debug.LogError($"game object: '{gameObject.name}' and field: '{field.Name}'!");
                }
            }
        }
    }
}