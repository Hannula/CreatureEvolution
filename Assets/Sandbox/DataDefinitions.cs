using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class DataDefinitions
{
    public string mapFilePath;
    public List<IntStringPair> terrainDataFilePaths;
    public List<IntStringPair> actorClassFilePaths;
    public List<IntStringPair> resourceClassFilePaths;

    public string terrainLayerName;
    public string actorLayerName;
    public string resourceLayerName;

    public float elevationStep;
    public int elevationStart;

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
