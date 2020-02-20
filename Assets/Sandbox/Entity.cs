using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity
{
    public Vector2Int LevelPosition
    {
        get { return currentTile.position; }
    }

    public Tile currentTile { get; protected set; }
}
