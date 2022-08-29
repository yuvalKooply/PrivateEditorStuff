using System;
using System.Globalization;
using Com.Kooply.Unity.Services;
using UnityEditor;
using UnityEngine;

namespace Editor.Private
{
    #if UNITY_EDITOR
    
    [InitializeOnLoad]
    public static class StateClearer
    {
        private const string PromptTitle = "State Clearer";
        private const string EnableStateClearerMenuItem = "Kooply/Enable State Clearer";
        private const string EditorPrefEnable = "StateClearer.Enable";
        private const string EditorPrefRemindMeTimestamp = "StateClearer.RemindMeTimestamp";
        private const int RemindMeHours = 4; // Number of hours from when the "Remind Me" button is clicked until the prompt is shown again

        private static bool EnableStateClearer
        {
            get => EditorPrefs.GetBool(EditorPrefEnable);
            set => EditorPrefs.SetBool(EditorPrefEnable, value);
        }
        
        private static string RemindMeTimestamp
        {
            get => EditorPrefs.GetString(EditorPrefRemindMeTimestamp);
            set => EditorPrefs.SetString(EditorPrefRemindMeTimestamp, value);
        }
        
        [MenuItem(EnableStateClearerMenuItem)]
        private static void EnableStateClearerCheckMenu()
        {
            EnableStateClearer = !EnableStateClearer;
            Menu.SetChecked(EnableStateClearerMenuItem, EnableStateClearer);

            if (EnableStateClearer)
                RemindMeTimestamp = "";
        }
   
        static StateClearer()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            
            Menu.SetChecked(EnableStateClearerMenuItem, EnableStateClearer);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (!EnableStateClearer)
                    return;
                
                var remindMeTimeExists = DateTime.TryParse(RemindMeTimestamp, out var remindMeDT);
                if (remindMeTimeExists && DateTime.Compare(remindMeDT, DateTime.Now) > 0)
                {
                    return;
                }
                
                if (EditorUtility.DisplayDialog(PromptTitle, "Would you like to clear the state?", "Clear State",
                    $"Remind Me in {RemindMeHours} Hours"))
                {
                    ClearState();
                    RemindLater();
                }
                else
                {
                    RemindLater();
                }
            }
        }

        private static void RemindLater()
        {
            RemindMeTimestamp = DateTime.Now.AddHours(RemindMeHours).ToString(CultureInfo.CurrentCulture);
        }
        
        private static void ClearState()
        {
            PlayerPrefs.DeleteAll();
            StorageService.DeleteAllPersistentDataPathFiles();
        }
    }
    #endif
}
