using Sandbox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile {
    public float elevation { get; private set; }
    public int temperature { get; private set; }
    public float lightLevel { get; private set; }
    public TerrainData terrain { get; private set; }   
    public Vector2Int position { get; private set; }

    public HashSet<Actor> actors;
    public HashSet<Resource> resources;

    public Tile(Vector2Int position, TerrainData terrain, float elevation, int temperature, float lightLevel)
    {
        this.position = position;
        this.terrain = terrain;
        this.elevation = elevation;
        this.temperature = temperature;
        this.lightLevel = lightLevel;

        actors = new HashSet<Actor>();
        resources = new HashSet<Resource>();
    }

    public void AddActor(Actor actor)
    {
        actors.Add(actor);
    }

    public void RemoveActor(Actor actor)
    {
        actors.Remove(actor);
    }

    public void AddResource(Resource resource)
    {
        resources.Add(resource);
    }

    public void RemoveResource(Resource resource)
    {
        resources.Remove(resource);
    }


}
