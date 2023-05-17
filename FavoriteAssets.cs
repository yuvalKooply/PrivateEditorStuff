using System;
using System.Collections.Generic;
using System.IO;
using Com.Kooply.Unity.ExtensionMethods;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Private
{
    public class FavoriteAssets : EditorWindow
    {
        [Serializable]
        public class DataWrapper
        {
            public List<AssetData> assets = new ();
        }

        [Serializable]
        public class AssetData
        {
            public string guid;
            public string path;
            public string name;
            public string type;
        }

        private static string GetPrefix() => Application.productName; 
        
        [SerializeField]
        private DataWrapper assetsData;
        private DataWrapper AssetsData
        {
            get
            {
                if (assetsData == null)
                    LoadData();
                
                return assetsData;
            }
        }

        private Vector2 _scrollView = Vector2.zero;

        [MenuItem("Window/Custom Panels/Favorites")]
        public static void ShowWindow()
        {
            GetWindow<FavoriteAssets>("Favorites");
        }

        public void OnGUI() 
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            if (GUILayout.Button("Pin Selected Assets", EditorStyles.miniButton))
            {
                foreach (var assetGUID in Selection.assetGUIDs)
                {
                    var assetData = new AssetData
                    {
                        guid = assetGUID,
                        path = AssetDatabase.GUIDToAssetPath(assetGUID)
                    };
                    
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(assetData.path);
                    assetData.name = asset.name;
                    assetData.type = asset.GetType().ToString();
                    assetsData.assets.Add(assetData);
                }
                SaveData();
            }
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label("Pinned Assets:");
                if (GUILayout.Button("▼ Sort Assets", EditorStyles.toolbarButton))
                    assetsData.assets.Sort(AssetDataComparer);
            }
            GUILayout.EndHorizontal();

            _scrollView = GUILayout.BeginScrollView(_scrollView);
            foreach (var assetData in AssetsData.assets) 
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(new GUIContent("Open", "Open file with default app"), GUILayout.ExpandWidth(false)))
                {
                    var extension = Path.GetExtension(assetData.path);
                    
                    if (!extension.Equals(".unity"))
                        EditorUtility.OpenWithDefaultApp(assetData.path);
                    else
                        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(assetData.path, UnityEditor.SceneManagement.OpenSceneMode.Single);
                }

                if (GUILayout.Button(new GUIContent("Ping", "Highlight asset on Project panel"), GUILayout.ExpandWidth(false)))
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(assetData.path));

                if (GUILayout.Button(new GUIContent(" " + assetData.name, AssetDatabase.GetCachedIcon(assetData.path)), GUILayout.Height(18)))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(assetData.path);
                    EditorGUIUtility.PingObject(asset);
                    Selection.activeObject = asset;
                    
                    var extension = Path.GetExtension(assetData.path);
                    if (!extension.IsNullOrEmpty() && extension!.Equals(".prefab"))
                        AssetDatabase.OpenAsset(asset);
                }

                if (GUILayout.Button(new GUIContent("X", "Un-pin"), GUILayout.ExpandWidth(false))){
                    RemovePin(assetData);
                    break;
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        private void SaveData ()
        {
            var key = GetPrefix() + "pinned";
            var json = JsonUtility.ToJson(AssetsData);
            EditorPrefs.SetString(key, json);
        }

        private void LoadData ()
        {
            assetsData = new DataWrapper();

            var key = GetPrefix() + "pinned";
            if (EditorPrefs.HasKey(key))
            {
                var json = EditorPrefs.GetString(key);
                assetsData = JsonUtility.FromJson<DataWrapper>(json);
            }
        }

        private void RemovePin (AssetData assetData)
        {
            assetsData.assets.Remove(assetData);
            SaveData();
        }

        private int AssetDataComparer (AssetData left, AssetData right)
        {
            return left.type.CompareTo(right.type);
        }
    }
}
