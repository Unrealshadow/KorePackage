using UnityEditor;
using UnityEngine;

public class RemoveMissingScripts : EditorWindow
{
    [MenuItem("CodeWithK/Remove Missing Scripts")]
    public static void ShowWindow()
    {
        GetWindow(typeof(RemoveMissingScripts));
    }

    // Declare variables to store the selected objects
    private GameObject[] selectedObjects;

    void OnGUI()
    {
        if (GUILayout.Button("Remove Missing Scripts"))
        {
            RemoveMissingScriptsFromSelectedObjects();
        }

        // Display the selected objects
        GUILayout.Label("Selected Objects:");
        if (selectedObjects != null)
        {
            foreach (GameObject selectedObject in selectedObjects)
            {
                GUILayout.Label("- " + (selectedObject != null ? selectedObject.name : "None"));
            }
        }
    }

    private void RemoveMissingScriptsFromSelectedObjects()
    {
        if (selectedObjects == null)
        {
            return;
        }

        foreach (GameObject selectedObject in selectedObjects)
        {
            if (selectedObject == null)
            {
                continue;
            }

            RemoveMissingScriptsFromGameObject(selectedObject);
        }
    }

    private void RemoveMissingScriptsFromGameObject(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        Component[] components = obj.GetComponents<Component>();

        if (components != null)
        {
            foreach (Component component in components)
            {
                if (component == null)
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
                }
            }
        }

        // Check for missing scripts in children
        foreach (Transform child in obj.transform)
        {
            RemoveMissingScriptsFromGameObject(child.gameObject);
        }
    }

    void OnSelectionChange()
    {
        // Update the selected objects whenever the selection changes
        selectedObjects = Selection.gameObjects;
        Repaint();
    }
}
