using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using ExcelDataReader;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using System.Linq;
public class ExcelToJSON : EditorWindow
{
    private string excelFilePath;
    private string jsonFilePath;
    private string jsonFileName;
    private string jsonFullPath;
    [MenuItem("Window/CodeWithK/Excel To JSON Converter")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ExcelToJSON));
    }

    public void OnGUI()
    {
        GUILayout.Label("Excel to JSON Converter", EditorStyles.boldLabel);
        GUILayout.Space(10f);

        GUILayout.Label("Excel File Path:");
        if (GUILayout.Button("Select Excel File"))
        {
            // Open a file selection dialog and set the selected file path to excelFilePath
            excelFilePath = EditorUtility.OpenFilePanel("Select Excel File", "", "xlsx,xls");
        }
        GUILayout.TextField(excelFilePath);

        GUILayout.Label("JSON File Name (Mandatory):");
        jsonFileName = GUILayout.TextField(jsonFileName);
        GUILayout.Space(10f);

        GUILayout.Label("Excel file should have the following rules:");
        GUILayout.Label("- The first row should contain the column headers.");
        GUILayout.Label("- The second row should contain the data types for each column (string, int, float, or bool).");
        GUILayout.Label("- The remaining rows should contain the data.");

        // Check if jsonFileName is empty or contains only whitespace
        bool isJsonFileNameValid = !string.IsNullOrWhiteSpace(jsonFileName);

        // Disable the Convert button if the JSON file name is not valid
        EditorGUI.BeginDisabledGroup(!isJsonFileNameValid);
        if (GUILayout.Button("Convert"))
        {
            ConvertExcelToJson();
        }
        EditorGUI.EndDisabledGroup();
    }


    private void ConvertExcelToJson()
    {
        // Load the Excel file into a DataTable
        DataTable dataTable = LoadExcelFile(excelFilePath);

        // Convert the DataTable into a List of Dictionaries, where each Dictionary represents a row in the Excel file
        List<Dictionary<string, object>> rows = ConvertDataTableToListOfDictionaries(dataTable);

        // Convert the List of Dictionaries into a JSON string
        string jsonString = ConvertListOfDictionariesToJson(rows);

        // Set the JSON file path and name
        if (jsonFileName == "")
        {
            // If the JSON file name is empty, use the name of the Excel file
            jsonFileName = Path.GetFileNameWithoutExtension(excelFilePath) + ".json";
        }
        else if (!jsonFileName.EndsWith(".json"))
        {
            jsonFileName += ".json";
        }

        jsonFilePath = "Assets/GeneratedJSONFromExcel";
        if (!AssetDatabase.IsValidFolder(jsonFilePath))
        {
            string parentFolder = Path.GetDirectoryName(jsonFilePath);
            string newFolderName = Path.GetFileName(jsonFilePath);
            AssetDatabase.CreateFolder(parentFolder, newFolderName);
        }
        jsonFullPath = Path.Combine(jsonFilePath, jsonFileName);

        // Write the JSON string to a file
        WriteJsonFile(jsonString, jsonFullPath);

        // Check if the script already exists and prompt the user to replace it or not
        string scriptPath = $"Assets/Scripts/ModelScripts/{Path.GetFileNameWithoutExtension(jsonFileName)}.cs";
        if (File.Exists(scriptPath))
        {
            bool replaceScript = EditorUtility.DisplayDialog("Model Script Already Exists",
                $"A model script with the name {Path.GetFileNameWithoutExtension(jsonFileName)} already exists. Do you want to replace it?",
                "Yes",
                "No");

            if (!replaceScript)
            {
                Debug.LogWarning("Please change the name of the model script to be generated.");
                return;
            }
        }

        GenerateModelClassFromJSON(Path.GetFileNameWithoutExtension(jsonFileName), jsonString, jsonFullPath);
        Debug.Log("Excel file converted to JSON successfully!");
    }


    private DataTable LoadExcelFile(string filePath)
    {
        FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

        // Read the Excel file into a DataSet
        DataSet dataSet = excelReader.AsDataSet();

        // Get the first DataTable from the DataSet
        DataTable dataTable = dataSet.Tables[0];

        return dataTable;
    }

    private List<Dictionary<string, object>> ConvertDataTableToListOfDictionaries(DataTable dataTable)
    {
        List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

        // Get the headers from the first row of the DataTable
        string[] headers = new string[dataTable.Columns.Count];
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            headers[i] = dataTable.Rows[0][i].ToString();
        }

        // Get the data types from the second row of the DataTable
        Type[] dataTypes = new Type[dataTable.Columns.Count];
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            string dataTypeString = dataTable.Rows[1][i].ToString();

            if (dataTypeString == "string")
            {
                dataTypes[i] = typeof(string);
            }
            else if (dataTypeString == "int")
            {
                dataTypes[i] = typeof(int);
            }
            else if (dataTypeString == "float")
            {
                dataTypes[i] = typeof(float);
            }
            else if (dataTypeString == "bool")
            {
                dataTypes[i] = typeof(bool);
            }
            else
            {
                Debug.LogError("Invalid data type: " + dataTypeString);
            }
        }

        // Get the data from the remaining rows of the DataTable
        for (int i = 2; i < dataTable.Rows.Count; i++)
        {
            DataRow row = dataTable.Rows[i];

            Dictionary<string, object> rowData = new Dictionary<string, object>();
            for (int j = 0; j < dataTable.Columns.Count; j++)
            {
                string header = headers[j];
                Type dataType = dataTypes[j];
                object value = null;

                if (dataType == typeof(string))
                {
                    value = row[j].ToString();
                }
                else if (dataType == typeof(int))
                {
                    int.TryParse(row[j].ToString(), out int intValue);
                    value = intValue;
                }
                else if (dataType == typeof(float))
                {
                    float.TryParse(row[j].ToString(), out float floatValue);
                    value = floatValue;
                }
                else if (dataType == typeof(bool))
                {
                    bool.TryParse(row[j].ToString(), out bool boolValue);
                    value = boolValue;
                }
                else
                {
                    Debug.LogError("Invalid data type: " + dataType.Name);
                }

                rowData.Add(header, value);
            }

            rows.Add(rowData);
        }

        return rows;
    }

    private string ConvertListOfDictionariesToJson(List<Dictionary<string, object>> list)
    {
        StringBuilder jsonStringBuilder = new StringBuilder();
        int index = 1;

        foreach (var dict in list)
        {
            // Add sequential index to each JSON object
            jsonStringBuilder.Append("\"" + index + "\": ");

            // Convert Dictionary to JSON string
            string json = JsonConvert.SerializeObject(dict);

            // Add JSON object to the main JSON string with a comma
            jsonStringBuilder.Append(json + ",");

            // Increment index
            index++;
        }

        // Remove the last comma from the main JSON string
        jsonStringBuilder.Remove(jsonStringBuilder.Length - 1, 1);

        // Enclose the main JSON string with curly braces
        string jsonString = "{" + jsonStringBuilder.ToString() + "}";
        return jsonString;
    }

    private void WriteJsonFile(string jsonString, string filePath)
    {
        File.WriteAllText(filePath, jsonString);
    }

    private void GenerateModelClassFromJSON(string scriptName, string jsonString, string jsonFullPath)
    {
        string folderPath = "Assets/Scripts/ModelScripts";
        string scriptPath = $"Assets/Scripts/ModelScripts/{scriptName}.cs";

        Dictionary<string, Dictionary<string, object>> data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(jsonString);

        if (data == null)
        {
            Debug.LogError("Failed to deserialize JSON file");
            return;
        }

        Dictionary<string, object> firstRow = data.Values.First();

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scripts"))
            {
                AssetDatabase.CreateFolder("Assets", "Scripts");
            }
            AssetDatabase.CreateFolder("Assets/Scripts", "ModelScripts");
        }

        using (StreamWriter sw = new StreamWriter("Assets/Scripts/ModelScripts/" + scriptName + ".cs"))
        {
            sw.WriteLine("using System;");
            sw.WriteLine("using System.Collections.Generic;");
            sw.WriteLine("using System.IO;");
            sw.WriteLine("using Newtonsoft.Json;");
            sw.WriteLine("using UnityEngine;");
            sw.WriteLine("");
            sw.WriteLine("[Serializable]");
            sw.WriteLine("public class " + scriptName);
            sw.WriteLine("{");

            // Generate properties from the first row of the data
            foreach (string colName in firstRow.Keys)
            {
                object colValue = firstRow[colName];
                switch (colValue.GetType().ToString())
                {
                    case "System.Int64":
                        sw.WriteLine("    public int " + colName + " { get; set; }");
                        break;
                    case "System.String":
                        sw.WriteLine("    public string " + colName + " { get; set; }");
                        break;
                    case "System.Boolean":
                        sw.WriteLine("    public bool " + colName + " { get; set; }");
                        break;
                    case "System.Char":
                        sw.WriteLine("    public float " + colName + " { get; set; }");
                        break;
                    case "System.Double":
                        sw.WriteLine("    public double " + colName + " { get; set; }");
                        break;
                    default:
                        Debug.LogWarning($"Unsupported data type for column {colName}");
                        break;
                }
            }

            sw.WriteLine("");
            sw.WriteLine("    public static string JsonFilePath => \"" + jsonFullPath.Replace("\\", "\\\\") + "\";");
            sw.WriteLine("");
            sw.WriteLine("    public static " + $"Dictionary<string,{scriptName}>" + " LoadData()");
            sw.WriteLine("    {");
            sw.WriteLine("        if (File.Exists(JsonFilePath))");
            sw.WriteLine("        {");
            sw.WriteLine("            string jsonString = File.ReadAllText(JsonFilePath);");
            sw.WriteLine("            Dictionary<string, " + scriptName + "> data = JsonConvert.DeserializeObject<Dictionary<string, " + scriptName + ">>(jsonString);");
            sw.WriteLine("            return data;");
            sw.WriteLine("        }");
            sw.WriteLine("        else");
            sw.WriteLine("        {");
            sw.WriteLine("            Debug.LogError(\"JSON file not found at: \" + JsonFilePath);");
            sw.WriteLine("            return null;");
            sw.WriteLine("        }");
            sw.WriteLine("    }");

            sw.WriteLine("}");
        }

        AssetDatabase.Refresh();
    }
}
