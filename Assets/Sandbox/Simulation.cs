using GA;
using Sandbox;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    public string dataDefsPath;
    public static Logger logger;
    public GameObject tilePrefab;
    public GameObject actorPrefab;
    public GameObject resourcePrefab;
    private string projectPath;
    private DataDefinitions dataDefs;
    private MapData mapData;
    private Dictionary<int, TerrainData> terrainData;
    private Dictionary<int, ActorClass> actorClasses;
    private Dictionary<int, ResourceClass> resourceClasses;
    private Level level;
    private CreatureChromosome creatureChromosome;

    private float roundDuration = 1f;
    public float roundDurationNormal = 1f;
    public float roundDurationFast = 1f;
    public float roundDurationFastest = 1f;

    private CreatureEvolution creatureEvolution;

    public bool paused = true;

    private bool simulationFinished = false;

    private bool simulateAll = false;

    private int SimulateRound()
    {
        int evolutionActorsAlive = 0;
        int totalAge = 0;
        foreach (Actor actor in level.actors)
        {
            if (actor.ActorClass.id == dataDefs.EvolutionCreatureID)
            {
                totalAge += actor.Age;
                if (actor.Hitpoints > 0)
                {
                    evolutionActorsAlive += 1;
                }
            }
            actor.Act();
        }

        // Return total age if every creature is dead
        if (evolutionActorsAlive == 0)
        {
            return totalAge;
        }
        else
        {
            return 0;
        }
    }

    IEnumerator simulate()
    {
        while (!simulationFinished)
        {
            if (!paused)
            {
                int totalAge = SimulateRound();

                // Stop simulation if every creature is dead
                if (totalAge > 0)
                {
                    Log("Simulation finished! Total age: " + totalAge);
                    // Set fitness value of chromosome to total age
                    creatureChromosome.fitness = totalAge;
                    simulationFinished = true;
                }
                yield return new WaitForSeconds(roundDuration);
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public void SimulateAll()
    {
        while (!simulationFinished)
        {
            int totalAge = SimulateRound();

            // Stop simulation if every creature is dead
            if (totalAge > 0)
            {
                Log("Simulation finished! Total age: " + totalAge);
                creatureChromosome.fitness = totalAge;
                simulationFinished = true;
            }
        }
    }

    public static void Log(string text)
    {
        if (logger == null)
        {
            logger = FindObjectOfType<Logger>();
        }
        logger.Log(text);
    }

    public void Start()
    {

        projectPath = Path.Combine(Application.dataPath, Path.GetDirectoryName(dataDefsPath));

        roundDuration = roundDurationNormal;

        // Load data
        LoadDataDefinitions();

        // Create CreatureEvolution GA-handler
        creatureEvolution = new CreatureEvolution(dataDefs.PopulationSize, dataDefs.GeneLimits);

        // Load map data
        LoadMapData(Path.Combine(projectPath, dataDefs.MapFilePath));

        // Load terrain
        LoadTerrainData();

        // Load actor classes
        LoadActorClasses();

        // Load resource classes
        LoadResourceClasses();

        // Create new level with map dimensions
        CreateLevel();

        // Create tile visualizers for the level
        VisualizeLevel();

        creatureChromosome = creatureEvolution.GetNext();

        // Spawn actors to the level
        SpawnActors();

        // Spawn resources
        SpawnResources();

        StartCoroutine(simulate());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Alpha0))
        {
            paused = !paused;
        }

        if (Input.GetKey(KeyCode.Alpha1))
        {
            paused = false;
            roundDuration = roundDurationNormal;
            simulateAll = false;
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            paused = false;
            roundDuration = roundDurationFast;
            simulateAll = false;
        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            paused = false;
            roundDuration = roundDurationFastest;
            simulateAll = false;
        }
        if (Input.GetKey(KeyCode.Alpha4))
        {
            paused = false;
            roundDuration = 0;
            simulateAll = false;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            // Toggle super simulation
            simulateAll = !simulateAll;
        }

        if (Input.GetKeyDown(KeyCode.Return) || simulateAll)
        {
            // Simulate instantly
            StopCoroutine(simulate());
            SimulateAll();
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            GUIUtility.systemCopyBuffer = JsonUtility.ToJson(creatureChromosome.ToActorClass(), true);
        }

        if (simulationFinished)
        {
            float fitness = creatureChromosome.fitness;
            Log("Fitness value for " + creatureChromosome.Name + " is " + fitness + ".");
            creatureChromosome = creatureEvolution.GetNext();
            if (creatureChromosome != null)
            {
                NewSimulation();
                Log("Starting new simulation for " + creatureChromosome.Name + ".");
            }
        }
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
        foreach (IntStringPair keyValuePair in dataDefs.TerrainDataFilePaths)
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
        foreach (IntStringPair keyValuePair in dataDefs.ActorClassFilePaths)
        {
            string path = keyValuePair.Value;
            string actorClassJson = FileReader.ReadString(Path.Combine(projectPath, path));
            ActorClass data = JsonUtility.FromJson<ActorClass>(actorClassJson);
            data.id = keyValuePair.Key;
            data.Initialize();
            actorClasses[data.id] = data;


        }
    }

    private void LoadResourceClasses()
    {
        resourceClasses = new Dictionary<int, ResourceClass>();
        foreach (IntStringPair keyValuePair in dataDefs.ResourceClassFilePaths)
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
        Vector2Int dimensions = new Vector2Int(mapData.width, mapData.height);
        Tile[,] tileGrid = new Tile[dimensions.x, dimensions.y];
        level = new Level(tileGrid);

        // Get map layers
        MapData.Layer terrainLayer = mapData.GetLayer(dataDefs.TerrainLayerName);
        MapData.Layer elevationLayer = mapData.GetLayer(dataDefs.ElevationLayerName);
        MapData.Layer temperatureLayer = mapData.GetLayer(dataDefs.TemperatureLayerName);
        MapData.Layer lightLayer = mapData.GetLayer(dataDefs.LightLevelLayerName);

        int i = 0;
        for (int y = 0; y < dimensions.y; y++)
        {
            for (int x = 0; x < dimensions.x; x++)
            {


                int terrainIndex = terrainLayer.data[i];
                float elevation = 0;
                float temperature = 0;
                float lightLevel = 1;

                // Try to get elevation
                if (elevationLayer != null && elevationLayer.data[i] != 0)
                {
                    elevation = (elevationLayer.data[i] - dataDefs.ElevationStartID) * dataDefs.ElevationStep;
                }

                // Try to get temperature
                if (temperatureLayer != null)
                {
                    temperature = dataDefs.TemperatureStart + (temperatureLayer.data[i] - dataDefs.TemperatureStartID) * dataDefs.TemperatureStep;
                }

                // Try to get light level
                if (lightLayer != null)
                {
                    lightLevel = Mathf.Clamp((lightLayer.data[i] - dataDefs.LightLevelStartID) * dataDefs.LightLevelStep, 0f, 1f);
                }

                TerrainData terrain = terrainData[0];
                if (terrainData.ContainsKey(terrainIndex))
                {
                    terrain = terrainData[terrainIndex];
                }

                Tile newTile = new Tile(level, new Vector2Int(x, y), terrain, elevation, temperature, lightLevel);

                tileGrid[x, y] = newTile;

                i++;
            }
        }
    }

    private void VisualizeLevel()
    {
        for (int x = 0; x < level.dimensions.x; x++)
        {
            for (int y = 0; y < level.dimensions.y; y++)
            {
                // Create visual representation for the tile
                Tile tile = level.TileAt(x, y);
                if (tile.terrain.id != 0)
                {
                    TileVisualizer tileVis = GameObject.Instantiate(tilePrefab, new Vector3(x, -y), Quaternion.identity).GetComponent<TileVisualizer>();
                    tileVis.position = new Vector2Int(x, y);
                    tileVis.tile = tile;
                    tileVis.tileset = mapData.GetTileset(0);
                    // Reload changes to tile
                    tileVis.Reload();
                }
            }
        }

    }

    public void NewSimulation()
    {
        simulationFinished = false;
        ActorClass evolutionActorClass = creatureChromosome.ToActorClass();
        evolutionActorClass.id = dataDefs.EvolutionCreatureID;
        evolutionActorClass.name = creatureChromosome.Name;
        evolutionActorClass.CreatureChromosome = creatureChromosome;
        actorClasses[dataDefs.EvolutionCreatureID] = evolutionActorClass;

        // Replace custom actor classes with new actor classes
        foreach (Actor a in level.actors)
        {
            if (a.ActorClass.id == dataDefs.EvolutionCreatureID)
            {
                a.ActorClass = evolutionActorClass;
            }
        }

        foreach (ActorVisualizer actorVisualizer in FindObjectsOfType<ActorVisualizer>())
        {
            // Update every actor visualizer
            actorVisualizer.Reload();
        }

        // Reset level
        level.Reset();
    }

    public void SpawnActors()
    {
        MapData.Layer actorLayer = mapData.GetLayer(dataDefs.ActorLayerName);

        // Get custom actor class from chromosome
        ActorClass evolutionActorClass = creatureChromosome.ToActorClass();
        evolutionActorClass.id = dataDefs.EvolutionCreatureID;
        evolutionActorClass.name = creatureChromosome.Name;
        evolutionActorClass.CreatureChromosome = creatureChromosome;
        actorClasses[dataDefs.EvolutionCreatureID] = evolutionActorClass;

        int i = 0;
        for (int y = 0; y < level.dimensions.y; y++)
        {
            for (int x = 0; x < level.dimensions.x; x++)
            {
                // Get actor id from actor layer
                int actorClassId = actorLayer.data[i];

                if (actorClasses.ContainsKey(actorClassId))
                {
                    ActorClass actorClass = actorClasses[actorClassId];
                    //Create actor
                    Tile startingTile = level.TileAt(x, y);
                    Actor actor = new Actor(actorClass, level, dataDefs.GlobalHungerRate, dataDefs.GlobalObservationDifficulty, dataDefs.GlobalMemoryLength, startingTile);

                    // Add actor to level
                    level.actors.Add(actor);

                    ActorVisualizer actorVisualizer = GameObject.Instantiate(actorPrefab, new Vector3(x, -y), Quaternion.identity).GetComponent<ActorVisualizer>();
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

    public void SpawnResources()
    {
        MapData.Layer resourceLayer = mapData.GetLayer(dataDefs.ResourceLayerName);
        int i = 0;
        for (int y = 0; y < level.dimensions.y; y++)
        {
            for (int x = 0; x < level.dimensions.x; x++)
            {
                // Get resource id from resouce layer
                int resourceClassId = resourceLayer.data[i];
                if (resourceClassId != 0 && resourceClasses.ContainsKey(resourceClassId))
                {
                    ResourceClass resourceClass = resourceClasses[resourceClassId];
                    //Create resource
                    Tile startingTile = level.TileAt(x, y);
                    Resource resource = new Resource(resourceClass, level, dataDefs.GlobalResourceAmountMultiplier, startingTile);

                    // Add actor to level
                    level.resources.Add(resource);

                    ResourceVisualizer resourceVisualizer = GameObject.Instantiate(resourcePrefab, new Vector3(x, -y), Quaternion.identity).GetComponent<ResourceVisualizer>();
                    resourceVisualizer.resource = resource;
                    resourceVisualizer.level = level;
                    resourceVisualizer.tileset = mapData.GetTileset(2);
                }

                i++;
            }
        }
    }

}
