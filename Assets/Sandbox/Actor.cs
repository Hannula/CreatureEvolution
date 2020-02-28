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

        public int Age { get; private set; }

        public float MeatAmount { get; set; }

        private int wanderArea = 10;
        private float observationDifficulty;
        public float MemoryLength = 250f;


        // AI
        public State state;
        public List<Tile> CurrentPath { get; private set; }

        private AStar<Tile> pathfinder;
        public Resource ResourceTarget { get; private set; }
        public Actor HuntTarget { get; private set; }
        Tile targetPosition;

        public Dictionary<Actor, Memory> ActorMemory { get; private set; }
        public Dictionary<Resource, Memory> ResourceMemory { get; private set; }

        public Actor(ActorClass actorClass, Level level, float hungerRate, float observationDifficulty, float memoryLength, Tile startingTile)
        {
            this.name = actorClass.name;
            this.actorClass = actorClass;
            this.level = level;
            Hitpoints = actorClass.maxHitpoints;
            this.hungerRate = hungerRate;
            this.observationDifficulty = observationDifficulty;
            MemoryLength = memoryLength;
            Hunger = 0;
            MeatAmount = actorClass.meatAmount;

            // Memory
            ActorMemory = new Dictionary<Actor, Memory>();
            ResourceMemory = new Dictionary<Resource, Memory>();

            pathfinder = new AStar<Tile>(GetAdjacentTiles, GetMovementCostRisk, GetMovementCostEstimation);

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

            openTiles.Enqueue(CurrentTile);

            // Search nearby tiles
            while (openTiles.Count > 0)
            {
                // Get next tile
                Tile tile = openTiles.Dequeue();
                closedTiles.Add(tile);

                float distance = Vector2Int.Distance(CurrentTile.position, tile.position);
                // Only process this tile if it's within observation radius
                if (distance < actorClass.observationRange)
                {
                    if (tile.actors.Count > 0 || tile.resources.Count > 0)
                    {
                        // Linear distance falloff
                        float distanceMultiplier = 1 - distance / actorClass.observationRange;
                        // Different senses
                        float vision = distanceMultiplier * Mathf.Lerp(actorClass.darkVision, actorClass.lightVision, tile.lightLevel);
                        float smell = distanceMultiplier * actorClass.smellSense;
                        float hearing = distanceMultiplier * actorClass.hearing;

                        float heightDifference = CurrentTile.elevation - tile.elevation;
                        // Gain small vision bonus from higher ground
                        vision = Mathf.Clamp(vision * (1 + heightDifference * 0.05f), 0, 1f);
                        // Gain small smell bonus from lower ground
                        smell = Mathf.Clamp(smell * (1 - heightDifference * 0.05f), 0, 1f);

                        // Actors
                        foreach (Actor actor in tile.actors)
                        {
                            // Process every actor which is not same species
                            if (actor.actorClass != actorClass)
                            {
                                float visibility = actor.actorClass.GetVisibilityValue(tile.terrain);
                                float noise = actor.actorClass.GetNoiseValue(tile.terrain);
                                // Odor is reduced if there are more actors in the same space
                                float odor = actor.actorClass.odor / tile.actors.Count;

                                float detectChance = visibility * vision + odor * smell + noise * hearing;
                                // Add tracking bonus if actor is already detected lately
                                if (ActorMemory.ContainsKey(actor))
                                {
                                    Memory mem = ActorMemory[actor];
                                    float memoryAge = Age - mem.Time;
                                    float trackingBonus = actorClass.tracking * (1 - memoryAge / MemoryLength);
                                    detectChance *= 1 + trackingBonus;
                                }
                                if (Random.Range(0, observationDifficulty) < detectChance)
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
            HuntTarget = null;
            ResourceTarget = null;
            // Find path to a random place
            if (CurrentPath == null || CurrentPath.Count == 0)
            {
                int tries = 20;
                bool found = false;
                Tile targetTile = null;
                while (tries > 0 && !found)
                {
                    int x = Random.Range(Mathf.Max(LevelPosition.x, wanderArea) - wanderArea,
                                Mathf.Min(LevelPosition.x, level.dimensions.x - wanderArea) + wanderArea);

                    int y = Random.Range(Mathf.Max(LevelPosition.y, wanderArea) - wanderArea,
                                Mathf.Min(LevelPosition.y, level.dimensions.y - wanderArea) + wanderArea);

                    targetTile = level.TileAt(x, y);

                    if (GetMovementCostRisk(targetTile, targetTile) < 10f / tries)
                    {
                        // Use this tile if movement cost is low
                        found = true;
                    }
                    --tries;
                }
                FindPath(targetTile);
            }
            else
            {
                PathAdvance();
            }

            if (Hunger > 33 && state == State.wander)
            {
                state = State.findFood;
                CurrentPath = null;
            }
        }

        private void FindFood()
        {
            // Stop finding food if full enough
            if (Hunger < 15)
            {
                state = State.wander;
            }
            else
            {
                if (HuntTarget != null && CurrentPath != null && CurrentTile != HuntTarget.CurrentTile)
                {
                    // Try pathing frequently if target is nearby
                    int repathChance = Mathf.CeilToInt(Vector2.Distance(LevelPosition, HuntTarget.LevelPosition));
                    if (Random.Range(0, repathChance + 2) == 0)
                    {
                        FindPath(targetPosition);
                    }
                }
                if (CurrentPath == null || CurrentPath.Count == 0)
                {
                    // If there's no path, find the best food source and try to go there
                    // Find the best resource from memory
                    float bestValue = 0;
                    float hungerRatio = Hunger / 100;

                    foreach (Resource resource in ResourceMemory.Keys)
                    {
                        Memory mem = ResourceMemory[resource];

                        float timeDifference = Age - mem.Time;

                        if (timeDifference < MemoryLength)
                        {
                            float value = mem.Value;
                            float dist = Vector2.Distance(CurrentTile.position, resource.CurrentTile.position);

                            float div = 1 + dist * 0.05f + mem.Risk * 5f / hungerRatio;

                            value /= div + timeDifference * 2f / MemoryLength;

                            if (value > bestValue)
                            {
                                ResourceTarget = resource;
                                bestValue = value;
                                targetPosition = mem.Tile;
                            }
                        }
                    }

                    // Find the best prey from memory
                    HuntTarget = null;
                    foreach (Actor actor in ActorMemory.Keys)
                    {
                        Memory mem = ActorMemory[actor];
                        float value = mem.Value;

                        if (mem.Tile == CurrentTile)
                        {
                            // Reduce value if memory tells actor is here but it's not
                            if (actor.CurrentTile != CurrentTile)
                            {
                                value = 0;
                            }
                            // Remove memory if actor is there but value is 0
                            if (actor.MeatAmount <= 0)
                            {
                                mem.Value = 0;
                                value = 0;
                            }
                        }
                        float dist = Vector2.Distance(CurrentTile.position, actor.LevelPosition);

                        float timeDifference = (Age - mem.Time);

                        float div = 1 + dist * 0.05f + timeDifference * 0.0025f + mem.Risk * 5f / hungerRatio;

                        value /= div;

                        if (value > bestValue)
                        {
                            // Replace previous hunt target or resource target if value is higher
                            ResourceTarget = null;
                            HuntTarget = actor;
                            bestValue = value;
                            targetPosition = mem.Tile;
                        }
                    }

                    if (ResourceTarget != null && ResourceTarget.CurrentTile != CurrentTile)
                    {
                        FindPath(targetPosition);
                    }
                    else if (HuntTarget != null && HuntTarget.CurrentTile != CurrentTile)
                    {
                        FindPath(targetPosition);
                    }
                }
                else
                {
                    PathAdvance();
                }

                // Perform eating if hungry
                if (ResourceTarget != null)
                {
                    if (ResourceTarget.CurrentTile == CurrentTile && ResourceTarget.Amount > 0)
                    {
                        Eat(ResourceTarget);
                    }
                }
                else if (HuntTarget != null)
                {
                    if (HuntTarget.CurrentTile == CurrentTile && HuntTarget.MeatAmount > 0)
                    {
                        if (HuntTarget.Hitpoints > 0)
                        {
                            // Attack if target still has health
                            PerformBestAttack(HuntTarget, false, true);
                            // Prey retaliates
                            HuntTarget.PerformBestAttack(this, true, true);
                        }
                        else
                        {
                            // Eat corpse if target is dead
                            Eat(HuntTarget);
                        }
                    }
                }
                else if (CurrentPath == null || CurrentPath.Count == 0)
                {
                    // Wander around if no food is found
                    Wander();
                }
            }
        }

        private void UpdateMemory(Actor subject)
        {
            if (subject.MeatAmount < 0)
            {
                ActorMemory[subject] = new Memory(subject, 0, 0, Age);
            }
            else
            {
                float value = 0;
                if (subject.MeatAmount > 0)
                {
                    value = Mathf.Min(actorClass.resourceConsumption, subject.MeatAmount * actorClass.meatConsumptionEfficiency);

                    // Reduce value if other actor is faster
                    float energyCost = GetMovementCost(subject.CurrentTile, subject.CurrentTile);
                    float energyCostSubject = subject.GetMovementCost(subject.CurrentTile, subject.CurrentTile);

                    if (energyCost > energyCostSubject)
                    {
                        float difference = energyCostSubject - energyCost;
                        value /= 1 + difference;
                    }
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


                ActorMemory[subject] = new Memory(subject, value, risk, Age);
            }
        }

        private void UpdateMemory(Resource subject)
        {
            if (subject.Amount < 0)
            {
                ResourceMemory[subject] = new Memory(subject, 0, 0, Age);
            }
            else
            {
                float value = Mathf.Min(actorClass.resourceConsumption, subject.Amount * actorClass.plantConsumptionEfficiency) / EatingEnergyCost(subject);
                float risk = actorClass.GetResourceClassRiskValues(subject.resourceClass);
                // Take hitpoints in to account
                risk /= GetHitpointRatio();

                ResourceMemory[subject] = new Memory(subject, value, risk, Age);
            }
        }

        #endregion


        #region Actions
        public bool Act()
        {
            if (Hitpoints > 0)
            {
                // Age this actor
                Age += 1;

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
                        Wander();
                    }
                    else if (state == State.findFood)
                    {
                        FindFood();
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
            if (CurrentPath == null || CurrentPath.Count == 0)
            {
                return true;
            }
            // Get the next tile
            Tile nextTile = UtilityFunctions.Pop<Tile>(CurrentPath);
            // Calculate movement cost
            float movementCost = GetMovementCost(CurrentTile, nextTile);

            // Advance to next tile
            MoveToTile(nextTile);
            // Reduce energy after moving
            Energy -= movementCost;

            // Attack immediately if hunt target is found
            if (HuntTarget != null && HuntTarget.CurrentTile == CurrentTile)
            {
                PerformBestAttack(HuntTarget, false, true);
                HuntTarget.PerformBestAttack(this, true, true);
                CurrentPath = null;
                return true;
            }

            // Return true if actor is now at the end of the path
            return CurrentPath.Count == 0;
        }
        public void PerformAttack(Attack attack, Actor target, bool retaliation = false, bool log = false)
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

        public void PerformBestAttack(Actor target, bool retaliation, bool log = false)
        {
            // Find the best attack
            float bestDamage = 0;
            Attack bestAttack = null;

            foreach (Attack attack in actorClass.attacks)
            {
                float damageExpected = target.actorClass.GetAttackExpectedDamage(attack);
                if (damageExpected >= bestDamage)
                {
                    bestAttack = attack;
                }
            }
            if (bestAttack != null)
            {
                PerformAttack(bestAttack, target, retaliation, log);
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

                    float requiredAmount = (Hunger / 100f * actorClass.resourceConsumption) / actorClass.plantConsumptionEfficiency;

                    // Reduce hunger
                    float foodAmount = Mathf.Min(resource.Amount, requiredAmount);

                    float hungerReduction = foodAmount / actorClass.resourceConsumption * 100;

                    Hunger -= hungerReduction;
                    resource.Amount -= foodAmount;
                    Simulation.Log(actorClass.name + " eats " + foodAmount + " units of " + resource.resourceClass.name + " at " + CurrentTile.position.ToString() + ". " + actorClass.name + " loses " + energyCost + " energy and " + hungerReduction + " hunger.");
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

                    float requiredAmount = (Hunger / 100f * actorClass.resourceConsumption) / actorClass.meatConsumptionEfficiency;

                    // Reduce hunger
                    float foodAmount = Mathf.Min(actor.MeatAmount, requiredAmount);

                    float hungerReduction = foodAmount / actorClass.resourceConsumption * 100;

                    Hunger -= hungerReduction;
                    actor.MeatAmount -= foodAmount;
                    Simulation.Log(actorClass.name + " eats " + foodAmount + " units of " + actor.actorClass.name + " at " + CurrentTile.position.ToString() + ". " + actorClass.name + " loses " + energyCost + " energy and " + hungerReduction + " hunger.");
                }

            }
        }

        /// <summary>
        /// Always use this method for moving actor
        /// </summary>
        /// <param name="targetTile"></param>
        public void MoveToTile(Tile targetTile)
        {
            if (CurrentTile != null)
            {
                // Remove this actor from previous tile
                CurrentTile.RemoveActor(this);
            }
            // Add this actor the target tile
            targetTile.AddActor(this);
            // Set current tile to target tile
            CurrentTile = targetTile;

            // Reduce viability of memories of actors that should be here but are not
            foreach (Actor actor in ActorMemory.Keys)
            {
                Memory mem = ActorMemory[actor];
                if (mem.Tile == targetTile && actor.CurrentTile != mem.Tile)
                {
                    mem.Time = 0;
                }
            }

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
                float cost = actorClass.GetTerrainMovementCost(targetTile.terrain);

                // Add additional penalty from elevation difference
                float elevationDifference = targetTile.elevation - startTile.elevation;
                if (elevationDifference > 0)
                {
                    // Target tile is higher
                    cost *= 1 + (elevationDifference / actorClass.steepNavigation);
                }

                // Temperature penalty
                float temperaturePenalty = 0;
                if (targetTile.temperature < actorClass.coldLimit)
                {
                    temperaturePenalty = Mathf.Abs(targetTile.temperature - actorClass.coldLimit);
                }
                else if (targetTile.temperature > actorClass.heatLimit)
                {
                    temperaturePenalty = targetTile.temperature - actorClass.heatLimit;
                }

                cost *= 1 + temperaturePenalty * 0.1f;
                return cost;
            }
            return float.MaxValue;

        }

        public float GetMovementCostRisk(Tile startTile, Tile targetTile)
        {
            float cost = GetMovementCost(startTile, targetTile);

            float totalRisk = 0;
            // Terrain risk
            foreach (Attack attack in targetTile.terrain.hazards)
            {
                totalRisk += actorClass.GetAttackExpectedDamage(attack);
            }

            totalRisk /= actorClass.maxHitpoints;

            // Hunter risk
            foreach (Actor actor in ActorMemory.Keys)
            {
                float dist = Vector2.Distance(targetTile.position, actor.CurrentTile.position);

                // Only increase risk if hunter is nearby
                if (dist < 5)
                {
                    Memory mem = ActorMemory[actor];

                    // Reduced risk if actor is far away or prefers plants
                    float actorRisk = mem.Risk / (1 + dist * 0.2f) * actor.actorClass.Predatory;

                    totalRisk += actorRisk;
                }

            }
            return cost * (1 + totalRisk);

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

            float estimatedCost = estimatedDistance;

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
            List<Tile> path = pathfinder.FindPath(CurrentTile, targetTile);
            CurrentPath = path;
        }

        public override string ToString()
        {
            string info = name;
            if (Hitpoints > 0)
            {
                info += "\nState: " + state + "\n";
                info += "Hitpoints: " + Mathf.Ceil(Hitpoints) + "/" + actorClass.maxHitpoints +
                    "\nAge: " + Mathf.Ceil(Age) +
                    "\nHunger: " + Mathf.Ceil(Hunger) +
                    "\nEnergy: " + Energy +
                    "\nVisibility: " + actorClass.GetVisibilityValue(CurrentTile.terrain);

                info += "\nResistances: \n";
                // Find every resistance
                foreach (DamageTypes dmgType in actorClass.resistances.Keys)
                {
                    float value = Mathf.Floor(actorClass.resistances[dmgType] * 100);
                    string text = dmgType.ToString() + ": " + value + "%\n";

                    info += text;
                }

                if (ResourceTarget != null)
                {
                    info += "\nResource target: " + ResourceTarget.resourceClass.name;
                }
                if (HuntTarget != null)
                {
                    info += "\nHunt target: " + HuntTarget.actorClass.name;
                }

                info += "\n\nMemories:";
                // Go through every memory
                foreach (Resource resourceMemory in ResourceMemory.Keys)
                {
                    Memory mem = ResourceMemory[resourceMemory];
                    info += "\n" + resourceMemory.resourceClass.name + "  Val: " + mem.Value + ", Risk: " + mem.Risk + ", T: " + mem.Time;
                }

                foreach (Actor actorMemory in ActorMemory.Keys)
                {
                    Memory mem = ActorMemory[actorMemory];
                    info += "\n" + actorMemory.actorClass.name + "  Val: " + mem.Value + ", Risk: " + mem.Risk + ", T: " + mem.Time;
                }
            }
            else
            {
                info += "Hitpoints: " + Mathf.Ceil(Hitpoints) + "/" + actorClass.maxHitpoints +
                    "\nAge: " + Mathf.Ceil(Age) +
                    "\nMeat amount: " + MeatAmount;
            }
            return info;
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
        public int Time { get; set; }
        public float Value { get; set; }
        public float Risk { get; private set; }

        public Memory(Entity subject, float value, float risk, int time)
        {
            this.Subject = subject;
            Tile = subject.CurrentTile;
            this.Value = value;
            this.Risk = risk;
            this.Time = time;
        }


    }
}