using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainData
{
    public int id;
    public string name;
    public Color groundColor;
    public char symbol;
    public Color symbolColor;

    public float cover;
    public float noise;

    public bool passable;

    public float difficulty;

    public float waterDepth;

    public List<Attack> hazards;

    public TerrainData()
    {
        id = 0;
        name = "Solid Wall";
        symbol = '.';
        symbolColor = Color.black;
        groundColor = Color.black;

        cover = 0;
        noise = 0;
        passable = false;

        difficulty = 0;
        waterDepth = 0;

        hazards = new List<Attack>();
    }

}