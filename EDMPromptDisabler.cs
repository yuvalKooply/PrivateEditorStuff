using System;
using System.Xml;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class EDMPromptDisabler : MonoBehaviour
{
    static EDMPromptDisabler()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        CloseAndDisableEDMPrompt();
    }

    [MenuItem("Kooply/Disable EDM Prompt")]
    public static void CloseAndDisableEDMPrompt()
    {
        CloseEDMPrompt();
        DisableEDMPrompt();
    }
    
    private static void CloseEDMPrompt()
    {
        var editorWindow = Resources.FindObjectsOfTypeAll<EditorWindow>();
        foreach (var window in editorWindow)
        {
            if (window.titleContent.text.StartsWith("Enable Android Auto-resolution?"))
            {
                window.Close();
                break;
            }
        }
    }
    
    private static void DisableEDMPrompt()
    {
        try
        {
            using var reader = new XmlTextReader("ProjectSettings/GvhProjectSettings.xml");
            var doc = new XmlDocument();
            doc.Load(reader);
            reader.Dispose();

            var promptNode =
                doc.SelectSingleNode(
                    "projectSettings/projectSetting[@name='GooglePlayServices.PromptBeforeAutoResolution']");
            if (promptNode != null)
            {
                var valueAttribute = promptNode.Attributes?["value"];
                if (valueAttribute != null)
                {
                    valueAttribute.Value = "False";
                }
                else
                {
                    if (promptNode is XmlElement promptElement)
                        promptElement.SetAttribute("value", "False");
                    else
                        throw new Exception("Failed to read GvhProjectSettings.xml file!");
                }
            }
            else
            {
                var projectSettingsNode = doc.SelectSingleNode("projectSettings");
                if (projectSettingsNode != null)
                {
                    var projectSettingNode = doc.CreateElement("projectSetting");
                    projectSettingNode.SetAttribute("name", "GooglePlayServices.PromptBeforeAutoResolution");
                    projectSettingNode.SetAttribute("value", "False");

                    projectSettingsNode.AppendChild(projectSettingNode);
                    doc.Save("ProjectSettings/GvhProjectSettings.xml");
                }
                else
                    throw new Exception("Failed to patch GvhProjectSettings.xml file!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("EDM Prompt Disabler: " + e.Message);
        }
    }

    private static async void DisableEDMPromptAfterDelay()
    {
        await UniTask.Delay(2000);
        CloseAndDisableEDMPrompt();
    }
    
    // ******************************************************************************
    // Events
    // ******************************************************************************

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        DisableEDMPrompt();
    }
    
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        DisableEDMPromptAfterDelay();
    }
}
