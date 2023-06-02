using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
        
        
        
        [MenuItem("Window/Custom Panels/Favorites")]
        public static void ShowWindow()
        {
            GetWindow<FavoriteAssets>("♥ Favorites");
        }
        
        [MenuItem("Assets/♥")]
        private static void AddToFavorites()
        {
            var instance = GetWindow<FavoriteAssets>();
            if (instance)
            {
                foreach (var assetGUID in Selection.assetGUIDs)
                    instance.PinAsset(assetGUID);
            }
            
            ShowProjectPanel();
        }
        
        private static void ShowProjectPanel()
        {
            EditorApplication.ExecuteMenuItem("Window/General/Project");
        }

        private static string GetPrefix() => Application.productName + "_"; 
        
        
        
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
        private bool _shouldShowMenu;
        private int _focusedIndex = -1;
        

        public void OnGUI() 
        {
            if (assetsData.assets.Count > 0)
            {
                GUILayout.BeginVertical();

                if (!_shouldShowMenu)
                {
                    if (GUILayout.Button("Menu", EditorStyles.miniButton, GUILayout.Width(80), GUILayout.Height(20)))
                        _shouldShowMenu = true;
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("<", EditorStyles.miniButton, GUILayout.Width(80), GUILayout.Height(20)))
                        _shouldShowMenu = false;
                    
                    var previousColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.red;

                    if (GUILayout.Button("Clear All", EditorStyles.miniButton, GUILayout.Width(80), GUILayout.Height(20)))
                    {
                        _focusedIndex = -1;
                        _shouldShowMenu = false;
                        assetsData.assets.Clear();
                        SaveData();
                    }
                    
                    GUI.backgroundColor = previousColor;
                    
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
            
                _scrollView = GUILayout.BeginScrollView(_scrollView);

                var index = 0;
                foreach (var assetData in AssetsData.assets)
                {
                    GUILayout.BeginHorizontal();

                    try
                    {

                        if (_focusedIndex == index)
                        {
                            if (GUILayout.Button(new GUIContent("X", "Remove"), GUILayout.ExpandWidth(false)))
                            {
                                RemovePin(assetData);
                                break;
                            }
                        }

                        if (GUILayout.Button(
                                new GUIContent(" " + assetData.name, AssetDatabase.GetCachedIcon(assetData.path)),
                                GUILayout.Height(18)))
                        {
                            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetData.path);
                            EditorGUIUtility.PingObject(asset);
                            Selection.activeObject = asset;

                            var extension = Path.GetExtension(assetData.path);
                            if (!extension.IsNullOrEmpty())
                            {
                                AssetDatabase.OpenAsset(asset);
                                ShowProjectPanel();
                            }
                            else if (AssetDatabase.IsValidFolder(assetData.path))
                            {
                                var projectTab = Type.GetType("UnityEditor.ProjectBrowser,UnityEditor");
                                var instance = projectTab.GetField("s_LastInteractedProjectBrowser",
                                    BindingFlags.Static | BindingFlags.Public).GetValue(null);
                                var showDirMeth = projectTab.GetMethod("ShowFolderContents",
                                    BindingFlags.NonPublic | BindingFlags.Instance);
                                showDirMeth?.Invoke(instance, new object[] {asset.GetInstanceID(), true});

                                ShowProjectPanel();
                            }
                        }

                        if (_focusedIndex == index)
                        {
                            GUI.enabled = index > 0;
                            if (GUILayout.Button(new GUIContent("▲", "Move up"), GUILayout.ExpandWidth(false)))
                            {
                                MoveAssetUp(index);
                                break;
                            }

                            GUI.enabled = index < AssetsData.assets.Count - 1;

                            if (GUILayout.Button(new GUIContent("▼", "Move down"), GUILayout.ExpandWidth(false)))
                            {
                                MoveAssetDown(index);
                                break;
                            }

                            GUI.enabled = true;

                            if (GUILayout.Button(new GUIContent("Done"), GUILayout.ExpandWidth(false)))
                                _focusedIndex = -1;
                        }
                        else
                        {
                            if (GUILayout.Button(new GUIContent("Edit"), GUILayout.ExpandWidth(false)))
                                _focusedIndex = index;
                        }
                    }
                    finally
                    {
                        // Workaround to the "Invalid GUILayout state" error message
                        index++;
                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.EndScrollView();
            }
            else
                GUILayout.Label("No Favorites (right click an asset and select ♥)");
        }

        private void SaveData()
        {
            var key = GetPrefix() + "pinned";
            var json = JsonUtility.ToJson(AssetsData);
            EditorPrefs.SetString(key, json);
        }

        private void LoadData()
        {
            assetsData = new DataWrapper();

            var key = GetPrefix() + "pinned";
            if (EditorPrefs.HasKey(key))
            {
                var json = EditorPrefs.GetString(key);
                assetsData = JsonUtility.FromJson<DataWrapper>(json);
            }
        }

        private void PinAsset(string assetGuid)
        {
            var assetData = new AssetData
            {
                guid = assetGuid,
                path = AssetDatabase.GUIDToAssetPath(assetGuid)
            };
                    
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetData.path);
            assetData.name = asset.name;
            assetData.type = asset.GetType().ToString();
            assetsData.assets.Add(assetData);
        }

        private void RemovePin(AssetData assetData)
        {
            assetsData.assets.Remove(assetData);
            SaveData();
            _focusedIndex = -1;
        }

        private void MoveAssetUp(int index)
        {
            if (index <= 0)
                return;

            (assetsData.assets[index - 1], assetsData.assets[index]) = (assetsData.assets[index], assetsData.assets[index - 1]);
            _focusedIndex--;
        }
        
        private void MoveAssetDown(int index)
        {
            if (index >= assetsData.assets.Count)
                return;

            (assetsData.assets[index + 1], assetsData.assets[index]) = (assetsData.assets[index], assetsData.assets[index + 1]);
            _focusedIndex++;
        }

        private void OnLostFocus()
        {
            _focusedIndex = -1;
            _shouldShowMenu = false;
        }
    }
}
