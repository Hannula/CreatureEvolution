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

    public Tile(Vector2Int position, TerrainData terrain, float elevation, int temperature, float lightLevel)
    {
        this.position = position;
        this.terrain = terrain;
        this.elevation = elevation;
        this.temperature = temperature;
        this.lightLevel = lightLevel;
    }

}
