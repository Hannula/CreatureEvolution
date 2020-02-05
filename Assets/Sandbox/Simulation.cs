using Sandbox;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    public string dataDefsPath;
    public GameObject tilePrefab;
    public GameObject actorPrefab;
    private string projectPath;
    private List<Actor> actors;
    private DataDefinitions dataDefs;
    private MapData mapData;
    private Dictionary<int, TerrainData> terrainData;
    private Dictionary<int, ActorClass> actorClasses;
    private Dictionary<int, ResourceClass> resourceClasses;
    private Level level;

    private void SimulateRound()
    {

    }

    public static void Log(string text)
    {
        Debug.Log(text);
    }

    public void Start()
    {
        /*TerrainData terrain = new TerrainData();
        string terrainString = JsonUtility.ToJson(terrain, true);

        string path = Application.dataPath + "/JsonData/Terrain/test.json";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path);
        writer.WriteLine(terrainString);
        writer.Close();*/

        projectPath = Path.Combine(Application.dataPath, Path.GetDirectoryName(dataDefsPath));


        // Load data
        LoadDataDefinitions();

        // Load map data
        LoadMapData(Path.Combine(projectPath, dataDefs.mapFilePath));

        // Load terrain
        LoadTerrainData();

        // Load actor classes
        LoadActorClasses();

        // Create new level with map dimensions
        CreateLevel();

        // Spawn actors to the level
        SpawnActors();
                     
        /*ActorClass human = new ActorClass("Human", 100, 15, 10, 5);
        Attack punch = new Attack("Punch", 3, new Attack.Damage(DamageTypes.Crushing, 10, 5));
        Attack kick = new Attack("Kick", 0, new Attack.Damage(DamageTypes.Crushing, 15, 5));
        human.AddAttacks(punch, kick);

        Actor erkki = new Actor(human, "Erkki");
        Actor pertti = new Actor(human, "Pertti");

        int tries = 0;
        while (erkki.hitpoints > 0 && pertti.hitpoints > 0 && tries < 100)
        {
            erkki.PerformAttack(erkki.GetRandomAttack(), pertti);
            pertti.PerformAttack(pertti.GetRandomAttack(), erkki);
            tries++;
        }*/
    }
    private void LoadDataDefinitions()
    {
        string path = Path.Combine(Application.dataPath, dataDefsPath);
        dataDefs = DataDefinitions.LoadFromJson(path);
    }
    private void LoadMapData(string mapJsonPath)
    {
        string mapPath = Path.Combine(Application.dataPath, mapJsonPath);
        mapData = MapData.LoadFromJson(mapPath);
    }

    private void LoadTerrainData()
    {
        terrainData = new Dictionary<int, TerrainData>();
        // Loop through every int, path pair
        foreach(IntStringPair keyValuePair in dataDefs.terrainDataFilePaths)
        {
            string path = keyValuePair.Value;
            string terrainDataJson = FileReader.ReadString(Path.Combine(projectPath, path));
            TerrainData data = JsonUtility.FromJson<TerrainData>(terrainDataJson);
            data.id = keyValuePair.Key;
            terrainData[data.id] = data;


        }
        // Wall
        terrainData[0] = new TerrainData();
    }

    private void LoadActorClasses()
    {
        actorClasses = new Dictionary<int, ActorClass>();
        foreach (IntStringPair keyValuePair in dataDefs.actorClassFilePaths)
        {
            string path = keyValuePair.Value;
            string actorClassJson = FileReader.ReadString(Path.Combine(projectPath, path));
            ActorClass data = JsonUtility.FromJson<ActorClass>(actorClassJson);
            data.id = keyValuePair.Key;
            actorClasses[data.id] = data;
        }
    }

    private void LoadResourceClasses()
    {
        resourceClasses = new Dictionary<int, ResourceClass>();
        foreach (IntStringPair keyValuePair in dataDefs.actorClassFilePaths)
        {
            string path = keyValuePair.Value;
            string resourceClassJson = FileReader.ReadString(Path.Combine(projectPath, path));
            ResourceClass data = JsonUtility.FromJson<ResourceClass>(resourceClassJson);
            data.id = keyValuePair.Key;
            resourceClasses[data.id] = data;
        }
    }

    private void CreateLevel()
    {
        level = new Level(mapData, terrainData);

        int i = 0;
        for(int x = 0; x < level.dimensions.x; x++)
        {
            for(int y = 0; y < level.dimensions.y; y++)
            {
                // Create visual representation for the tile
                Tile tile = level.TileAt(x, y);
                if (tile.terrain.id != 0)
                {
                    TileVisualizer tileVis = GameObject.Instantiate(tilePrefab, new Vector3(x, y), Quaternion.identity).GetComponent<TileVisualizer>();
                    tileVis.position = new Vector2Int(x, y);
                    tileVis.tile = tile;
                    tileVis.tileset = mapData.GetTileset(0);
                    // Reload changes to tile
                    tileVis.Reload();
                }

                i++;
            }
        }

    }

    public void SpawnActors()
    {
        MapData.Layer actorLayer = mapData.GetLayer(dataDefs.actorLayerName);
        int i = 0;
        for (int x = 0; x < level.dimensions.x; x++)
        {
            for (int y = 0; y < level.dimensions.y; y++)
            {
                // Get actor id from actor layer
                int actorClassId = actorLayer.data[i];
                if (actorClassId != 0 && actorClasses.ContainsKey(actorClassId))
                {
                    ActorClass actorClass = actorClasses[actorClassId];
                    //Create actor
                    Actor actor = new Actor(actorClass);
                    actor.levelPosition = new Vector2Int(x, y);

                    // Add actor to level
                    level.actors.Add(actor);

                    ActorVisualizer actorVisualizer = GameObject.Instantiate(actorPrefab, new Vector3(x, y), Quaternion.identity).GetComponent<ActorVisualizer>();
                    actorVisualizer.actor = actor;
                    actorVisualizer.level = level;
                    actorVisualizer.tileset = mapData.GetTileset(1);
                    // Reload changes to actor visualizer
                    actorVisualizer.Reload();
                }

                i++;
            }
        }
    }

}
