using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity
{
    public Vector2Int LevelPosition
    {
        get { return CurrentTile.position; }
    }
       
    public Tile CurrentTile { get; protected set; }
}
