using Pathfinding;
using Sandbox;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;
namespace Sandbox
{
    [System.Serializable]
    public class Actor : Entity
    {
        public string name;

        public float Energy { get; private set; }

        public float Hunger { get; private set; }
        private float hungerRate;

        public float Hitpoints { get; private set; }
        public readonly ActorClass actorClass;
        public Level level;

        public int age { get; private set; }


        // AI
        public State state;
        public List<Tile> currentPath;

        private AStar<Tile> pathfinder;
        private Resource foodTarget;
        private Actor huntTarget;

        public Dictionary<Actor, Memory> actorMemory;
        public Dictionary<Resource, Memory> resourceMemory;

        public Actor(ActorClass actorClass, Level level, float hungerRate, Tile startingTile, string name = "Actor")
        {
            this.name = name;
            this.actorClass = actorClass;
            this.level = level;
            Hitpoints = actorClass.maxHitpoints;
            this.hungerRate = hungerRate;
            Hunger = 0;

            // Memory
            actorMemory = new Dictionary<Actor, Memory>();
            resourceMemory = new Dictionary<Resource, Memory>();

            pathfinder = new AStar<Tile>(GetAdjacentTiles, GetMovementCost, GetMovementCostEstimation);

            MoveToTile(startingTile);
        }

        #region AI
        private void Observe()
        {

        }

        private void Wander()
        {

        }

        private void UpdateMemory(Actor actor)
        {
            float value = 0;
            float risk = 0;
            actorMemory[actor] = new Memory(actor, value, risk, age);
        }

        #endregion


        #region Actions
        public bool Act()
        {
            if (Hitpoints > 0)
            {
                // Age this actor
                age += 1;

                Hunger += hungerRate;
                // Start taking damage when too hungry
                if (Hunger > 100)
                {
                    Hitpoints -= actorClass.maxHitpoints * 0.01f;
                }

                if (Energy < 0)
                {
                    Energy += 1f;
                }
                else
                {
                    if (currentTile.resources.Count > 0)
                    {
                        foodTarget = currentTile.resources.ElementAt(0);
                    }
                    // Perform eating if hungry
                    if (foodTarget != null && Hunger > 50)
                    {
                        if (foodTarget.currentTile == currentTile && foodTarget.plantAmount > 0)
                        {
                            Eat(foodTarget);
                        }
                    }

                    PathAdvance();
                }

            }
            // Return true if actor is still alive
            return Hitpoints > 0;
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
            Energy -= movementCost;

            // Return true if actor is now at the end of the path
            return currentPath.Count == 0;
        }
        public void PerformAttack(Attack attack, Actor target, bool log = true)
        {
            if (log)
            {
                Simulation.Log(name + "(" + Hitpoints + " hp) attacks " + target.name + "(" + target.Hitpoints + " hp) with " + attack.name + ".");
            }
            bool attackSuccess = false;
            // Determine if the attack hits the target
            // Roll
            int attackRoll = Dice.Roll(100);

            if (attackRoll >= 95)
            {
                // Critical hit always hits
                attackSuccess = true;
            }
            else if (attackRoll >= 5)
            {
                // Attack didn't miss but no critical hit
                int toHit = attackRoll + attack.attackBonus;

                // Compare toHit -value to target's armor class
                if (toHit >= target.actorClass.evasion)
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

                // Reduce target's hitpoints by damage
                target.Hitpoints -= totalDamage;

                if (log)
                {
                    if (target.Hitpoints > 0)
                    {
                        Simulation.Log(attack.name + " hits " + target.name + " causing " + totalDamage + " damage. " + target.name + " now has " + target.Hitpoints + " hitpoints.");
                    }
                    else
                    {
                        Simulation.Log(attack.name + " hits " + target.name + " causing " + totalDamage + " damage. " + target.name + " now has " + target.Hitpoints + " hitpoints and is thus dead.");
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

        public void Eat(Resource resource)
        {
            if (resource.plantAmount > 0)
            {
                float energyCost = EatingEnergyCost(resource);

                if (energyCost < float.MaxValue)
                {

                    // Apply energy cost
                    Energy -= energyCost;

                    float requiredAmount = Hunger / 100f * actorClass.resourceConsumption;

                    // Reduce hunger
                    float foodAmount = Mathf.Min(resource.plantAmount, requiredAmount);

                    float hungerReduction = foodAmount / actorClass.resourceConsumption * 100;

                    Hunger -= hungerReduction;
                    resource.plantAmount -= foodAmount;
                    Simulation.Log(actorClass.name + " eats " + foodAmount + " units of " + resource.resourceClass.name + " at " + currentTile.position.ToString() + ". " + actorClass.name + " loses " + energyCost + " energy and " + hungerReduction + " hunger.");
                }

            }
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

        #endregion

        public float EatingEnergyCost(Resource resource)
        {
            // Calculate how long eating a singe unit is going to take
            float energyCost = actorClass.resourceConsumptionEnergyCost * resource.resourceClass.gatheringDifficulty;

            float resourceDepth = resource.resourceClass.depth;

            // If there is water, add diving cost
            if (resource.currentTile.terrain.waterDepth > 0 && resourceDepth > 0)
            {
                if (actorClass.divingSkill > 0)
                {
                    energyCost += resourceDepth / actorClass.divingSkill;
                }
                else
                {
                    energyCost = float.MaxValue;
                }
            }
            else if (resourceDepth > 0)
            {
                // Resource is underground, add digging cost
                if (actorClass.diggingSpeed > 0)
                {
                    energyCost += resourceDepth / actorClass.diggingSpeed;
                }
                else
                {
                    energyCost = float.MaxValue;
                }
            }
            else if (resourceDepth < -actorClass.height)
            {
                // Resource is high up, add climbing cost
                if (actorClass.climbingSpeed > 0)
                {
                    energyCost += (-resourceDepth - actorClass.height) / actorClass.climbingSpeed;
                }
                else
                {
                    energyCost = float.MaxValue;
                }
            }

            return energyCost;
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
            if (leftTile != null && actorClass.IsPassable(leftTile.terrain))
            {
                adjacentTiles.Add(leftTile);
            }
            if (rightTile != null && actorClass.IsPassable(rightTile.terrain))
            {
                adjacentTiles.Add(rightTile);
            }
            if (upTile != null && actorClass.IsPassable(upTile.terrain))
            {
                adjacentTiles.Add(upTile);
            }
            if (downTile != null && actorClass.IsPassable(downTile.terrain))
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

    public enum State
    {
        wander,
        findFood,
        escape
    }

    public class Memory
    {
        // Memory of a certain entity in certain place at certain time
        Entity subject;
        Tile tile;
        int time;
        float value;
        float risk;

        public Memory(Entity subject, float value, float risk, int time)
        {
            this.subject = subject;
            tile = subject.currentTile;
            this.value = value;
            this.risk = risk;
            this.time = time;
        }


    }
}