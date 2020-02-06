using Sandbox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile {
    public int elevation { get; private set; }
    public int temperature { get; private set; }
    public TerrainData terrain { get; private set; }   
    public Vector2Int position { get; private set; }

    private Level level; 

    public Tile(Level level, Vector2Int position, TerrainData terrain, int elevation, int temperature)
    {
        this.level = level;
        this.position = position;
        this.terrain = terrain;
        this.elevation = elevation;
        this.temperature = temperature;
    }

}
