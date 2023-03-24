using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class FolderStructureGenerator : EditorWindow
{
    [MenuItem("CodeWithK/Generate Default Folders")]
    private static void GenerateDefaultFolders()
    {
        string[] folders = new string[]
        {
            "Scripts",
            "Art",
            "Animations",
            "Audio",
            "Prefabs",
            "Scenes",
            "Resources",
            "Materials",
            "Textures",
            "Fonts",
            "ThirdParty"
        };

        string rootPath = Application.dataPath;

        foreach (string folder in folders)
        {
            string folderPath = Path.Combine(rootPath, folder);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Debug.Log("Created folder: " + folderPath);
            }
            else
            {
                Debug.Log("Folder already exists: " + folderPath);
            }
        }

        AssetDatabase.Refresh();
    }
}
