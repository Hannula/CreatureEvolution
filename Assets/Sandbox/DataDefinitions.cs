using Sandbox;
using System.Collections.Generic;
using UnityEngine;
using Utility;
[System.Serializable]
public class DataDefinitions
{
    public string MapFilePath;
    public List<IntStringPair> TerrainDataFilePaths;
    public List<IntStringPair> ActorClassFilePaths;
    public List<IntStringPair> ResourceClassFilePaths;

    public string TerrainLayerName;
    public string ActorLayerName;
    public string ResourceLayerName;
    public string LightLevelLayerName;
    public string ElevationLayerName;
    public string TemperatureLayerName;

    public int EvolutionCreatureID;

    public float ElevationStep;
    public int ElevationStartID;

    public float LightLevelStep;
    public int LightLevelStartID;

    public int TemperatureStart;
    public float TemperatureStep;
    public int TemperatureStartID;

    public float GlobalHungerRate;
    public float GlobalObservationDifficulty;
    public float GlobalResourceAmountMultiplier;
    public float GlobalMemoryLength;

    public int PopulationSize;
    public int EliteProportion;
    public float FitnessPower = 1f;

    [SerializeField]
    public List<KeyIntRangePair> GeneLimits;

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

[System.Serializable]
public class KeyIntRangePair
{
    public string Key;
    public int Min;
    public int Max;

}
