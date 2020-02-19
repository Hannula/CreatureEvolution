using Sandbox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Sandbox
{
    public class Resource
    {
        public string name;

        public Vector2Int LevelPosition
        {
            get { return currentTile.position; }
        }

        public Tile currentTile;
        public float plantAmount = 0;

        public float hitpoints;
        public readonly ResourceClass resourceClass;
        public Level level;

        public Resource(ResourceClass resourceClass, Level level, float amount, Tile startingTile, string name = "Resource")
        {
            this.name = name;
            this.resourceClass = resourceClass;
            this.level = level;
            this.plantAmount = resourceClass.plantAmount * amount;

            MoveToTile(startingTile);
        }

        public void MoveToTile(Tile targetTile)
        {
            if (currentTile != null)
            { 
                // Remove this actor from previous tile
                currentTile.RemoveResource(this);
            }
            // Add this actor the target tile
            targetTile.AddResource(this);
            // Set current tile to target tile
            currentTile = targetTile;
        }
    }
}