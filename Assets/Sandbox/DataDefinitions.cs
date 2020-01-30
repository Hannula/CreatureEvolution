using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class DataDefinitions
{
    public string mapFilePath;
    public List<IntStringPair> terrainDataFilePaths;
    public List<IntStringPair> faunaDataFilePaths;

    public static DataDefinitions LoadFromJson(string path)
    {
        // Read defs json from file
        string defsJson = FileReader.ReadString(path);
        // Parse json to DataDefinitions-object
        DataDefinitions dataDefs = JsonUtility.FromJson<DataDefinitions>(defsJson);

        return dataDefs;
    }
}
[System.Serializable]
public class IntStringPair
{
    public int Key;
    public string Value;

    public IntStringPair(int key, string value)
    {
        Key = key;
        Value = value;
    }
}
