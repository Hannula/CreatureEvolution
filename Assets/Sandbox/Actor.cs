using Pathfinding;
using Sandbox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
namespace Sandbox
{
    [System.Serializable]
    public class Actor
    {
        public string name;

        public Vector2Int LevelPosition
        {
            get { return currentTile.position; }
        }

        public Tile currentTile { get; private set; }
        public float energy = 0;

        public float hunger;
        public float hungerRate;

        public float hitpoints;
        public readonly ActorClass actorClass;
        public Level level;

        public List<Tile> currentPath;

        private AStar<Tile> pathfinder;

        public Actor(ActorClass actorClass, Level level, float hungerRate, Tile startingTile, string name = "Actor")
        {
            this.name = name;
            this.actorClass = actorClass;
            this.level = level;
            hitpoints = actorClass.maxHitpoints;
            this.hungerRate = hungerRate;
            hunger = 0;

            pathfinder = new AStar<Tile>(GetAdjacentTiles, GetMovementCost, GetMovementCostEstimation);

            MoveToTile(startingTile);
        }

        public void PerformAttack(Attack attack, Actor target, bool log = true)
        {
            if (log)
            {
                Simulation.Log(name + "(" + hitpoints + " hp) attacks " + target.name + "(" + target.hitpoints + " hp) with " + attack.name + ".");
            }
            bool attackSuccess = false;
            // Determine if the attack hits the target
            // Roll
            int attackRoll = Dice.Roll(20);

            if (attackRoll == 20)
            {
                // Critical hit always hits
                attackSuccess = true;
            }
            else if (attackRoll > 1)
            {
                // Attack didn't miss but no critical hit
                int toHit = attackRoll + attack.attackBonus;

                // Compare toHit -value to target's armor class
                if (toHit >= target.actorClass.armorClass)
                {
                    // Attack hits if attackRoll + attack bonus exceeds target's armor class
                    attackSuccess = true;
                }
            }

            // If the attack was successful, deal damage
            if (attackSuccess)
            {
                float totalDamage = 0;
                // Loop through every damage in attack
                foreach (Attack.Damage dmg in attack.damage)
                {
                    // Get base damage by dice roll
                    float baseDamage = Dice.Roll(dmg.damageRoll) + dmg.damageBonus;

                    // Get resistance
                    DamageTypes type = dmg.damageType;
                    float resistance = target.GetResistance(type);

                    // Reduce the damage by resistance
                    totalDamage += baseDamage * (1 - resistance);
                }

                // Reduce target's hitpoints by damage (rounded up)
                target.hitpoints -= Mathf.CeilToInt(totalDamage);

                if (log)
                {
                    if (target.hitpoints > 0)
                    {
                        Simulation.Log(attack.name + " hits " + target.name + " causing " + totalDamage + " damage. " + target.name + " now has " + target.hitpoints + " hitpoints.");
                    }
                    else
                    {
                        Simulation.Log(attack.name + " hits " + target.name + " causing " + totalDamage + " damage. " + target.name + " now has " + target.hitpoints + " hitpoints and is thus dead.");
                    }
                }
            }
            else
            {
                if (log)
                {
                    Simulation.Log(attack.name + " misses.");
                }
            }
        }
        /// <summary>
        /// Get a random attack from attack list with equal odds.
        /// </summary>
        /// <returns></returns>
        public Attack GetRandomAttack()
        {
            // Pick a random attack if there are any. Otherwise default to null.
            return Utility.UtilityFunctions.GetRandomElement(actorClass.attacks);
        }

        public float GetResistance(DamageTypes damageType)
        {
            return actorClass.GetResistance(damageType);
        }

        /// <summary>
        /// Always use this method for moving actor
        /// </summary>
        /// <param name="targetTile"></param>
        public void MoveToTile(Tile targetTile)
        {
            if (currentTile != null)
            {
                // Remove this actor from previous tile
                currentTile.RemoveActor(this);
            }
            // Add this actor the target tile
            targetTile.AddActor(this);
            // Set current tile to target tile
            currentTile = targetTile;
        }

        public bool Act()
        {
            if (hitpoints > 0)
            {
                hunger += hungerRate;
                // Start taking damage when too hungry
                if (hunger > 100)
                {
                    hitpoints -= actorClass.maxHitpoints * 0.01f;
                }

                if (energy < 0)
                {
                    energy += 0.1f;
                }
                else
                {
                    PathAdvance();
                }
            }
            // Return true if actor is still alive
            return hitpoints > 0;
        }
        /// <summary>
        /// Advance on current path.
        /// </summary>
        /// <returns>End of the path.</returns>
        public bool PathAdvance()
        {
            if (currentPath == null || currentPath.Count == 0)
            {
                return true;
            }
            // Get the next tile
            Tile nextTile = UtilityFunctions.Pop<Tile>(currentPath);
            // Calculate movement cost
            float movementCost = GetMovementCost(currentTile, nextTile);

            // Advance to next tile
            MoveToTile(nextTile);
            // Reduce energy after moving
            energy -= movementCost;

            // Return true if actor is now at the end of the path
            return currentPath.Count == 0;
        }

        public float GetMovementCost(Tile startTile, Tile targetTile)
        {
            // Base cost is based on actor class and terrain
            if (targetTile != null)
            {
                // Take both start tile and target tile into account
                float cost = actorClass.GetTerrainMovementCost(currentTile.terrain);
                cost += actorClass.GetTerrainMovementCost(targetTile.terrain);

                // Add additional penalty from elevation difference
                float elevationDifference = targetTile.elevation - startTile.elevation;
                if (elevationDifference > 0)
                {
                    // Target tile is higher
                    cost *= 1 + (elevationDifference / actorClass.steepNavigation);
                }
                return cost;
            }
            return float.MaxValue;

        }
        /// <summary>
        /// Get estimated cost from current tile to target tile using manhattan distance.
        /// </summary>
        /// <param name="targetTile"></param>
        /// <returns></returns>
        public float GetMovementCostEstimation(Tile startTile, Tile targetTile)
        {
            // Use manhattan distance for estimation
            float estimatedDistance = Mathf.Abs(targetTile.position.x - startTile.position.x) + Mathf.Abs(targetTile.position.y - startTile.position.y);

            float estimatedCost = estimatedDistance / actorClass.speed;

            return estimatedCost;
        }

        public List<Tile> GetAdjacentTiles(Tile tile)
        {
            List<Tile> adjacentTiles = new List<Tile>();

            // Get adjacent tiles from level
            Tile leftTile = level.TileAt(tile.position.x - 1, tile.position.y);
            Tile rightTile = level.TileAt(tile.position.x + 1, tile.position.y);
            Tile upTile = level.TileAt(tile.position.x, tile.position.y - 1);
            Tile downTile = level.TileAt(tile.position.x, tile.position.y + 1);

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

            return adjacentTiles;
        }

        public void FindPath(Tile targetTile)
        {
            List<Tile> path = pathfinder.FindPath(currentTile, targetTile);
            currentPath = path;
        }

    }
}