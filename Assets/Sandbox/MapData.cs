using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
[System.Serializable]
public class MapData
{
    public int height;
    public int width;
    public List<Layer> layers;
    public int tileheight;
    public int tilewidth;
    [SerializeField]
    private List<TilesetData> tilesets;
    public List<Tileset> loadedTilesets;

    #region Unused
    public string backgroundcolor;
    public Settings editorsettings;
    public int compressionlevel;
    public bool infinite;
    public int nextlayerid;
    public int nextobjectid;
    public string orientation;
    public string renderorder;
    public string tiledversion;
    public string type;
    public float version;

    [System.Serializable]
    public struct Settings
    {
        public Export export;
    }
    [System.Serializable]
    public struct Export
    {
        public string format;
        public string target;
    }
    #endregion

    [System.Serializable]
    public class Layer
    {
        public int[] data;

        #region Unused
        public float opacity;
        public string type;
        public bool visible;
        public int height;
        public int width;
        public int id;
        public string name;
        public int x;
        public int y;
        #endregion
    }

    [System.Serializable]
    public class TilesetData
    {
        public int firstgid;
        public string source;
    }

    public static MapData LoadFromJson(string path)
    {
        // Read map json from file
        string mapJson = FileReader.ReadString(path);
        // Parse json to MapData-object
        MapData mapData = JsonUtility.FromJson<MapData>(mapJson);
        mapData.LoadTilesets(Path.GetDirectoryName(path));
        return mapData;
    }

    private void LoadTilesets(string path)
    {
        loadedTilesets = new List<Tileset>();
        // Load every tileset file for this map
        foreach(TilesetData tilesetData in tilesets)
        {
            string tilesetPath = Path.Combine(path, tilesetData.source);
            // Load tileset json from file
            string tilesetJson = FileReader.ReadString(tilesetPath);
            // Parse json
            Tileset tileset = JsonUtility.FromJson<Tileset>(tilesetJson);

            // Load texture image for loaded tileset
            tileset.LoadTexture(Path.GetDirectoryName(tilesetPath));

            // Add the loaded tileset to the list of tilesets
            loadedTilesets.Add(tileset);
        }
    }

    public Layer GetLayer(string layerName)
    {
        foreach(Layer layer in layers)
        {
            if (layer.name == layerName)
            {
                return layer;
            }
        }
        Simulation.Log("Layer '" + layerName + "' not found!");
        return null;
    }
    public Tileset GetTileset(int number)
    {
        if (number < loadedTilesets.Count)
        {
            return loadedTilesets[number];
        }
        Simulation.Log("Tileset '" + number + "' not found!");
        return null;
    }
}