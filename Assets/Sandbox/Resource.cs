using Sandbox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Sandbox
{
    public class Resource : Entity
    {
        public string Name { get; private set; }

        public Tile CurrentTile { get; private set; }
        public float Amount { get; set; }

        public float HitPoints { get; private set; }
        public readonly ResourceClass resourceClass;
        private Level level;

        public Resource(ResourceClass resourceClass, Level level, float amount, Tile startingTile, string name = "Resource")
        {
            this.Name = name;
            this.resourceClass = resourceClass;
            this.level = level;
            this.Amount = resourceClass.plantAmount * amount;

            currentTile = startingTile;

            MoveToTile(startingTile);
        }

        public void MoveToTile(Tile targetTile)
        {
            if (CurrentTile != null)
            { 
                // Remove this actor from previous tile
                CurrentTile.RemoveResource(this);
            }
            // Add this actor the target tile
            targetTile.AddResource(this);
            // Set current tile to target tile
            CurrentTile = targetTile;
        }
    }
}