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
        public float amount = 0;

        public float hitpoints;
        public readonly ResourceClass resourceClass;
        public Level level;

        public Resource(ResourceClass resourceClass, Level level, float amount, string name = "Resource")
        {
            this.name = name;
            this.resourceClass = resourceClass;
            this.level = level;
            this.amount = resourceClass.foodAmount * amount;
        }
    }
}