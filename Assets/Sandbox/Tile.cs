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

    private Level level;
    private List<Tile> adjacentTiles;

    public Tile(Level level, Vector2Int position, TerrainData terrain, float elevation, int temperature, float lightLevel)
    {
        this.level = level;
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

    public List<Tile> GetAdjacentTiles()
    {
        if (adjacentTiles == null)
        {
            adjacentTiles = new List<Tile>();

            // Get adjacent tiles from level
            Tile leftTile = level.TileAt(position.x - 1, position.y);
            Tile rightTile = level.TileAt(position.x + 1, position.y);
            Tile upTile = level.TileAt(position.x, position.y - 1);
            Tile downTile = level.TileAt(position.x, position.y + 1);

            // Add tiles if they exists and are traversable
            if (leftTile != null && leftTile.terrain.id != 0)
            {
                adjacentTiles.Add(leftTile);
            }
            if (rightTile != null && rightTile.terrain.id != 0)
            {
                adjacentTiles.Add(rightTile);
            }
            if (upTile != null && upTile.terrain.id != 0)
            {
                adjacentTiles.Add(upTile);
            }
            if (downTile != null && downTile.terrain.id != 0)
            {
                adjacentTiles.Add(downTile);
            }
        }

        return adjacentTiles;
    }


}
