using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile {
    public int elevation { get; private set; }
    public int temperature { get; private set; }
    public TerrainData terrain { get; private set; }   

    public Tile(TerrainData terrain, int elevation, int temperature)
    {
        this.terrain = terrain;
        this.elevation = elevation;
        this.temperature = temperature;
    }
}
