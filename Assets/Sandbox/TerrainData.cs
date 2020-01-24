using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainData
{
    string name;
    char symbol;
    Color symbolColor;
    Color backgroundColor;

    int cover;

    bool passable;

    int difficulty;
    int waterDepth;

    List<Attack> hazards;
}
