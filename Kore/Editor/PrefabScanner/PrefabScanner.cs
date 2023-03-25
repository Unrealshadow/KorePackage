using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class PrefabScanner : EditorWindow
{
    private static GameObject selectedPrefab;
    private string scriptName = "";

    [MenuItem("CodeWithK/Prefab Scanner")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(PrefabScanner));
    }

    private void OnGUI()
    {
        ValidatePrefabSelection();

        EditorGUILayout.Space();

        scriptName = EditorGUILayout.TextField(
            new GUIContent("Script Name", "Enter a name for the script."),
            scriptName,
            EditorStyles.textField
        );

        EditorGUILayout.Space();

        DisplaySelectedPrefab();

        EditorGUILayout.Space();

        if (GUILayout.Button("Scan Prefab"))
        {
            ScanPrefab();
        }

        Repaint();
    }

    private void ValidatePrefabSelection()
    {
        if (Selection.activeObject != null
            && AssetDatabase.Contains(Selection.activeObject)
            && PrefabUtility.GetPrefabAssetType(Selection.activeObject) == PrefabAssetType.Regular)
        {
            selectedPrefab = (GameObject)Selection.activeObject;
        }
        else
        {
            selectedPrefab = null;
        }
    }

    private void DisplaySelectedPrefab()
    {
        if (selectedPrefab != null)
        {
            EditorGUILayout.LabelField("Selected Prefab: " + selectedPrefab.name);
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Please select a prefab asset from the project window.",
                MessageType.Info
            );
        }
    }

    private void ScanPrefab()
    {
        if (selectedPrefab != null)
        {
            if (string.IsNullOrEmpty(scriptName))
            {
                scriptName = selectedPrefab.name + "UIData";
            }

            string scriptPath = "Assets/Scripts/UIScripts/" + scriptName + ".cs";

            if (!ProcessExistingScript(scriptPath))
            {
                return;
            }

            if (!IsValidScriptName(scriptName))
            {
                EditorUtility.DisplayDialog(
                    "Invalid script name",
                    "The script name contains invalid characters. Please enter a valid name.",
                    "OK"
                );
                return;
            }

            CreateUIScriptsFolderIfNeeded();

            using (StreamWriter writer = new StreamWriter(scriptPath))
            {
                WriteScriptContent(writer);
            }

            AssetDatabase.Refresh();

            AddGeneratedScriptToPrefab();

            EditorUtility.DisplayDialog(
                "Script generated",
                "The script was successfully generated at " + scriptPath,
                "OK"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Invalid prefab",
                "Please select a valid prefab asset from the project window.",
                "OK"
            );
        }

        ResetFields();
    }

    private void ResetFields()
    {
        scriptName = "";
    }

    private bool IsValidScriptName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        char[] invalidChars = Path.GetInvalidFileNameChars();
        foreach (char c in invalidChars)
        {
            if (name.Contains(c.ToString()))
            {
                return false;
            }
        }

        return true;
    }

    private bool ProcessExistingScript(string scriptPath)
    {
        if (File.Exists(scriptPath))
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "Script already exists",
                "The script already exists. Do you want to overwrite it?",
                "Yes",
                "No"
            );

            if (!overwrite)
            {
                return false;
            }
        }

        return true;
    }

    private void CreateUIScriptsFolderIfNeeded()
    {
        string scriptsPath = "Assets/Scripts";
        if (!AssetDatabase.IsValidFolder(scriptsPath))
        {
            AssetDatabase.CreateFolder("Assets", "Scripts");
        }

        string folderPath = "Assets/Scripts/UIScripts";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Scripts", "UIScripts");
        }
    }

    private void WriteScriptContent(StreamWriter writer)
    {
        writer.WriteLine("using UnityEngine;");
        writer.WriteLine("using System;");
        writer.WriteLine("using UnityEngine.UI;");
        writer.WriteLine("using TMPro;");

        writer.WriteLine("");
        writer.WriteLine("[Serializable]");
        writer.WriteLine("public class " + scriptName + " : MonoBehaviour");
        writer.WriteLine("{");

        FindSerializedFields(selectedPrefab, writer);

        writer.WriteLine("\tprivate void Start()");
        writer.WriteLine("\t{");
        writer.WriteLine("\t\tAssignUIComponents();");
        writer.WriteLine("\t}");

        writer.WriteLine("\tpublic void AssignUIComponents()");
        writer.WriteLine("\t{");
        writer.WriteLine("\t\tforeach (Transform child in transform)");
        writer.WriteLine("\t\t{");
        writer.WriteLine("\t\t\tswitch(child.gameObject.name)");
        writer.WriteLine("\t\t\t{");

        AssignUIComponents(selectedPrefab, writer);

        writer.WriteLine("\t\t\t}");
        writer.WriteLine("\t\t}");
        writer.WriteLine("\t}");

        writer.WriteLine("}");
    }

    private void AddGeneratedScriptToPrefab()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            AssetDatabase.GetAssetPath(selectedPrefab)
        );

        MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(
            "Assets/Scripts/UIScripts/" + scriptName + ".cs"
        );

        if (prefab.GetComponent(script.GetClass()) != null)
        {
            EditorUtility.DisplayDialog(
                "Script already added",
                "The script is already added to the prefab.",
                "OK"
            );
            ResetFields();
            return;
        }

        prefab.AddComponent(script.GetClass());
    }

    private void FindSerializedFields(Transform parent, StreamWriter writer)
{
    // Find all UI elements that have a name starting with "m_" in the current parent
    foreach (Transform child in parent)
    {
        if (child.gameObject.name.StartsWith("m_"))
        {
            // Get the type of the UI component attached to the element
            Component uiComponent = child.gameObject.GetComponent(typeof(Component));
            if (uiComponent != null)
            {
                System.Type uiComponentType = uiComponent.GetType();

                // Write a public field for the UI element of the appropriate type
                string fieldName = child.gameObject.name;
                writer.WriteLine(
                    "\tpublic "
                        + uiComponentType.Name
                        + " "
                        + fieldName
                        + ";"
                );
            }
        }

        // Recurse into the child's children
        FindSerializedFields(child, writer);
    }
}

private void AssignUIComponents(Transform parent, StreamWriter writer)
{
    // Recurse through all children of the parent
    foreach (Transform child in parent)
    {
        if (child.gameObject.name.StartsWith("m_"))
        {
            // Get the type of the UI component attached to the element
            Component uiComponent = child.gameObject.GetComponent(typeof(Component));
            if (uiComponent != null)
            {
                System.Type uiComponentType = uiComponent.GetType();

                // Write a switch case for the UI element to assign the component to the appropriate field
                string elementName = child.gameObject.name;
                writer.WriteLine("\t\t\t\tcase \"" + elementName + "\":");
                writer.WriteLine(
                    "\t\t\t\t\t"
                        + elementName
                        + " = child.GetComponent<"
                        + uiComponentType.Name
                        + ">();"
                );
                writer.WriteLine("\t\t\t\t\tbreak;");
            }
        }
        else
        {
            // Recurse into the child's children
            AssignUIComponents(child, writer);
        }
    }
}

}
