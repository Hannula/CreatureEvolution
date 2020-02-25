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

        public float MeatAmount { get; set; }


        // AI
        public State state;
        public List<Tile> currentPath;

        private AStar<Tile> pathfinder;
        public Resource ResourceTarget;
        public Actor HuntTarget;

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
            MeatAmount = actorClass.meatAmount;

            // Memory
            actorMemory = new Dictionary<Actor, Memory>();
            resourceMemory = new Dictionary<Resource, Memory>();

            pathfinder = new AStar<Tile>(GetAdjacentTiles, GetMovementCost, GetMovementCostEstimation);

            MoveToTile(startingTile);
        }

        public float GetHitpointRatio()
        {
            return Hitpoints / actorClass.maxHitpoints;
        }

        #region AI
        private void Observe()
        {
            // Use breadth first search to observe surrounding area
            HashSet<Tile> closedTiles = new HashSet<Tile>();
            Queue<Tile> openTiles = new Queue<Tile>();

            openTiles.Enqueue(currentTile);

            // Search nearby tiles
            while(openTiles.Count > 0)
            {
                // Get next tile
                Tile tile = openTiles.Dequeue();
                closedTiles.Add(tile);

                float distance = Vector2Int.Distance(currentTile.position, tile.position);
                // Only process this tile if it's within observation radius
                if (distance < actorClass.ObservationRange)
                {
                    if (tile.actors.Count > 0 || tile.resources.Count > 0)
                    {
                        // Different senses
                        float vision = 1 - (distance / actorClass.visionRange) * Mathf.Lerp(actorClass.darkVision, actorClass.lightVision, tile.lightLevel);
                        float smell = 1 - (distance / actorClass.smellSenseRange);
                        float hearing = 1 - (distance / actorClass.hearingRange);

                        float heightDifference = currentTile.elevation - tile.elevation;
                        // Gain vision bonus from higher ground
                        vision = Mathf.Clamp(vision * ( 1 + heightDifference * 0.1f), 0, 1f);
                        // Gain smell bonus from lower ground
                        smell = Mathf.Clamp(smell * (1 - heightDifference * 0.1f), 0, 1f);

                        // Actors
                        foreach (Actor actor in tile.actors)
                        {
                            // Process every actor which is not same species
                            if (actor.actorClass != actorClass)
                            {
                                float visibility = actor.actorClass.GetVisibilityValue(tile.terrain);

                                if (Random.Range(0, 1f) < visibility * vision)
                                {
                                    UpdateMemory(actor);
                                }
                            }
                        }

                        // Resources
                        foreach (Resource resource in tile.resources)
                        {
                            float visibility = resource.resourceClass.visibility;

                            if (Random.Range(0, 1f) < visibility * vision)
                            {
                                UpdateMemory(resource);
                            }
                        }
                    }

                    // Add neighbors
                    foreach (Tile neighbor in tile.GetAdjacentTiles())
                    {
                        if (!closedTiles.Contains(neighbor) && !openTiles.Contains(neighbor))
                        {
                            openTiles.Enqueue(neighbor);
                        }
                    }
                }

            }

        }

        private void Wander()
        {

        }

        private void UpdateMemory(Actor subject)
        {
            if (subject.Hitpoints < 0)
            {
                actorMemory[subject] = new Memory(subject, 0, 0, age);
            }
            else
            {
                float value = 0;
                if (subject.MeatAmount > 0)
                {
                    value = Mathf.Min(actorClass.resourceConsumption, subject.MeatAmount / actorClass.meatConsumptionEfficiency);
                }
                float risk = 0;
                if (subject.Hitpoints > 0)
                {
                    risk = actorClass.GetActorClassRiskValues(subject.actorClass);
                    // Take hitpoints in to account
                    risk *= subject.GetHitpointRatio() / GetHitpointRatio();

                    // Risk is reduced if both actors prefer plant based food
                    risk *= Mathf.Max(actorClass.Predatory, subject.actorClass.Predatory);
                }


                actorMemory[subject] = new Memory(subject, value, risk, age);
            }
        }

        private void UpdateMemory(Resource subject)
        {
            if (subject.Amount < 0)
            {
                resourceMemory[subject] = new Memory(subject, 0, 0, age);
            }
            else
            {
                float value = Mathf.Min(actorClass.resourceConsumption, subject.Amount * actorClass.plantConsumptionEfficiency) / EatingEnergyCost(subject);
                float risk = actorClass.GetResourceClassRiskValues(subject.resourceClass);
                // Take hitpoints in to account
                risk /= GetHitpointRatio();

                resourceMemory[subject] = new Memory(subject, value, risk, age);
            }
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
                    if (state == State.wander)
                    {
                        HuntTarget = null;
                        ResourceTarget = null;
                        // Find path to random place
                        if (currentPath == null || currentPath.Count == 0)
                        {
                                FindPath(level.TileAt(Random.Range(0, level.dimensions.x), Random.Range(0, level.dimensions.y)));
                        }
                        else
                        {
                            PathAdvance();
                        }

                        if (Hunger > 50)
                        {
                            state = State.findFood;
                            currentPath = null;
                        }

                    }
                    else if (state == State.findFood )
                    {
                        if (Hunger < 25)
                        {
                            state = State.wander;

                        }
                        else 
                        {
                            if (currentPath == null || currentPath.Count == 0)
                            {
                                Resource bestResource = null;
                                float bestValue = 0;
                                foreach (Resource resource in resourceMemory.Keys)
                                {
                                    Memory mem = resourceMemory[resource];
                                    float value = mem.Value;
                                    float dist = Vector2.Distance(currentTile.position, resource.CurrentTile.position);

                                    float timeDifference = (age - mem.Time);

                                    float div = 1 + timeDifference * 0.01f;

                                    if (dist > 0)
                                    {
                                        value /= dist;
                                        value /= dist;
                                    }

                                    if (value >= bestValue)
                                    {
                                        bestResource = resource;
                                        bestValue = value;
                                    }

                                    ResourceTarget = bestResource;

                                }

                                if (ResourceTarget == null)
                                {
                                    FindPath(level.TileAt(Random.Range(0, level.dimensions.x), Random.Range(0, level.dimensions.y)));
                                }
                                else if (ResourceTarget.CurrentTile != currentTile)
                                {
                                    FindPath(ResourceTarget.CurrentTile);
                                }
                            }
                            else
                            {
                                PathAdvance();
                            }



                            // Perform eating if hungry
                            if (ResourceTarget != null)
                            {
                                if (ResourceTarget.CurrentTile == currentTile && ResourceTarget.Amount > 0)
                                {
                                    Eat(ResourceTarget);
                                }
                            }
                        }
                    }

                    Observe();
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
        public void PerformAttack(Attack attack, Actor target, bool log = false)
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

                // Compare toHit -value to target's evasion
                if (toHit >= target.actorClass.evasion)
                {
                    // Attack hits if attackRoll + attack bonus exceeds target's evasion
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

        public void TakeAttack(Attack attack, bool log = true)
        {
            if (log)
            {
                Simulation.Log(name + "(" + Hitpoints + " hp) takes attack " + attack.name + ".");
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

                // Compare toHit -value to evasion
                if (toHit >= actorClass.evasion)
                {
                    // Attack hits if attackRoll + attackBonus exceed evasion
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
                    float resistance = GetResistance(type);

                    // Reduce the damage by resistance
                    totalDamage += baseDamage * (1 - resistance);
                }

                // Reduce hitpoints by damage
                Hitpoints -= totalDamage;

                if (log)
                {
                    if (Hitpoints > 0)
                    {
                        Simulation.Log(attack.name + " hits " + name + " causing " + totalDamage + " damage. " + name + " now has " + Hitpoints + " hitpoints.");
                    }
                    else
                    {
                        Simulation.Log(attack.name + " hits " + name + " causing " + totalDamage + " damage. " + name + " now has " + Hitpoints + " hitpoints and is thus dead.");
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
            if (resource.Amount > 0)
            {
                float energyCost = EatingEnergyCost(resource);

                if (energyCost < float.MaxValue)
                {

                    // Apply energy cost
                    Energy -= energyCost;

                    float requiredAmount = ( Hunger / 100f * actorClass.resourceConsumption ) / actorClass.plantConsumptionEfficiency;

                    // Reduce hunger
                    float foodAmount = Mathf.Min(resource.Amount, requiredAmount);

                    float hungerReduction = foodAmount / actorClass.resourceConsumption * 100;

                    Hunger -= hungerReduction;
                    resource.Amount -= foodAmount;
                    Simulation.Log(actorClass.name + " eats " + foodAmount + " units of " + resource.resourceClass.name + " at " + currentTile.position.ToString() + ". " + actorClass.name + " loses " + energyCost + " energy and " + hungerReduction + " hunger.");
                    foreach (Attack attack in resource.resourceClass.hazards)
                    {
                        TakeAttack(attack);
                    }
                }

            }
        }
        public void Eat(Actor actor)
        {
            if (actor.Hitpoints <= 0 && actor.MeatAmount > 0 && actorClass.meatConsumptionEfficiency > 0)
            {
                float energyCost = actorClass.resourceConsumptionEnergyCost / actorClass.meatConsumptionEfficiency;

                if (energyCost < float.MaxValue)
                {

                    // Apply energy cost
                    Energy -= energyCost;

                    float requiredAmount = (Hunger / 100f * actorClass.resourceConsumption) / actorClass.plantConsumptionEfficiency;

                    // Reduce hunger
                    float foodAmount = Mathf.Min(actor.MeatAmount, requiredAmount);

                    float hungerReduction = foodAmount / actorClass.resourceConsumption * 100;

                    Hunger -= hungerReduction;
                    actor.MeatAmount -= foodAmount;
                    Simulation.Log(actorClass.name + " eats " + foodAmount + " units of " + actor.actorClass.name + " at " + currentTile.position.ToString() + ". " + actorClass.name + " loses " + energyCost + " energy and " + hungerReduction + " hunger.");
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
            float energyCost = actorClass.resourceConsumptionEnergyCost * resource.resourceClass.gatheringDifficulty / actorClass.plantConsumptionEfficiency;

            float resourceDepth = resource.resourceClass.depth;

            // If there is water, add diving cost
            if (resource.CurrentTile.terrain.waterDepth > 0 && resourceDepth > 0)
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
        public Entity Subject { get; private set; }
        public Tile Tile { get; private set; }
        public int Time { get; private set; }
        public float Value { get; private set; }
        public float Risk { get; private set; }

        public Memory(Entity subject, float value, float risk, int time)
        {
            this.Subject = subject;
            Tile = subject.currentTile;
            this.Value = value;
            this.Risk = risk;
            this.Time = time;
        }


    }
}