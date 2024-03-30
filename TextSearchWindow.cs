using UnityEditor;
using UnityEngine;

namespace Editor.Private
{
    public class TextSearchWindow : EditorWindow
    {
        private string _searchText = string.Empty;

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
                SearchForTextInAllComponents();
        }

        private void SearchForTextInAllComponents()
        {
            var found = false;
            var allAssetPaths = AssetDatabase.GetAllAssetPaths();

            foreach (var assetPath in allAssetPaths)
            {
                if (assetPath.EndsWith(".prefab") || assetPath.EndsWith(".asset"))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    if (asset != null)
                    {
                        var serializedObject = new SerializedObject(asset);
                        var prop = serializedObject.GetIterator();

                        while (prop.NextVisible(true))
                        {
                            if (prop.propertyType == SerializedPropertyType.String)
                            {
                                if (prop.stringValue.Contains(_searchText))
                                {
                                    Debug.LogError($"Found '{_searchText}' in {assetPath}", asset);
                                    found = true;
                                    Selection.activeGameObject = asset as GameObject;
                                }
                            }
                        }
                    }
                }
            }

            if (!found)
            {
                Debug.LogError($"Text Search: No results found for '{_searchText}'.");
            }
        }
    }
}