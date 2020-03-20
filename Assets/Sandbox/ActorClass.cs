﻿using Sandbox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActorClass
{
    public string name;
    public int id;

    public CreatureChromosome CreatureChromosome { get; set; }

    public float maxHitpoints;
    public float speed;
    public int evasion;
    public float size;
    public Dictionary<DamageTypes, float> resistances;

    public List<string> resistanceStrings;
    public List<Attack> attacks;

    // Movement
    public float swimmingSpeed;
    public float ruggedLandNavigation;
    public float softLandNavigation;
    public float crampedNavigation;
    public float steepNavigation;

    // Survival
    public float resourceConsumption;
    public float resourceConsumptionEnergyCost;
    public float meatConsumptionEfficiency;
    public float plantConsumptionEfficiency;
    public float diggingSpeed;
    public float climbingSpeed;
    public float divingSkill;

    public float coldLimit;
    public float heatLimit;

    public Color baseColor;
    public Color patternColor;
    public float visibility;
    public float noise;
    public float odor;

    public float meatAmount;

    // Observation
    public float observationRange;

    public float lightVision;
    public float darkVision;

    public float smellSense;
    public float hearing;

    public float tracking;


    public float height;

    private Dictionary<TerrainData, float> movementCosts;

    private Dictionary<ActorClass, float> actorClassRiskValues;
    private Dictionary<ResourceClass, float> resourceClassRiskValues;
    private Dictionary<Attack, float> expectedAttackDamages;
    private Dictionary<ActorClass, float> expectedActorClassDamages;
    private Dictionary<TerrainData, float> visibilityValues;
    private Dictionary<TerrainData, float> noiseValues;

    public float Predatory { get; private set; }

    public ActorClass(string name, int hitpoints, int speed, int evasion, int size, float swimming, float rugged, float soft, float cramped)
    {
        this.name = name;
        maxHitpoints = hitpoints;
        this.speed = speed;
        this.evasion = evasion;
        this.size = size;
        swimmingSpeed = swimming;
        ruggedLandNavigation = rugged;
        softLandNavigation = soft;
        crampedNavigation = cramped;
        attacks = new List<Attack>();
        resistances = new Dictionary<DamageTypes, float>();
    }

    public ActorClass(string name)
    {
        this.name = name;
        attacks = new List<Attack>();
        resistances = new Dictionary<DamageTypes, float>();
    }

    public void Initialize()
    {
        plantConsumptionEfficiency = Mathf.Clamp(plantConsumptionEfficiency, 0, 1f);
        meatConsumptionEfficiency = Mathf.Clamp(meatConsumptionEfficiency, 0, 1f);
        // If this actor can consume plants, calculate ratio of predatory
        if (plantConsumptionEfficiency > 0)
        {
            Predatory = Mathf.Min(1f, meatConsumptionEfficiency / plantConsumptionEfficiency);
        }
        else
        {
            Predatory = 1f;
        }

        // Resistances
        ParseResistances();
    }

    public void ParseResistances()
    {
        resistances = new Dictionary<DamageTypes, float>();
        if (resistanceStrings != null)
        {
            foreach (string resistanceString in resistanceStrings)
            {
                try
                {
                    string[] parts = resistanceString.Split(':');
                    // Find damage type
                    DamageTypes type = DamageTypes.None;
                    foreach (DamageTypes damageType in Enum.GetValues(typeof(DamageTypes)))
                    {
                        if (parts[0] == damageType.ToString())
                        {
                            type = damageType;
                        }
                    }
                    // Continue only if damage type is found
                    if (type != DamageTypes.None)
                    {
                        float resistance = float.Parse(parts[1]);
                        // Set resistance of given damage type
                        resistances[type] = resistance;
                    }

                }
                catch (Exception)
                {
                    Simulation.Log("Failed to parse resistance strings. Correct format is \"DamageType:ResistanceValue\".");
                }
            }
        }
    }

    public void AddAttacks(params Attack[] attackList)
    {
        // Add every attack
        foreach (Attack attack in attackList)
        {
            attacks.Add(attack);
        }
    }

    public float GetResistance(DamageTypes damageType)
    {
        // Default resistance is 0
        float resistance = 0;

        // If resistance exists in resistance dictionary, use that value
        if (resistances.ContainsKey(damageType))
        {
            // Resistance cannot exceed 1 (100% resistance)
            resistance = Mathf.Min(1, resistances[damageType]);
        }

        return resistance;
    }

    public bool IsPassable(TerrainData terrain)
    {
        if (terrain.id == 0)
        {
            // Return false if there is no terrain
            return false;
        }
        if (terrain.waterDepth > height && swimmingSpeed <= 0)
        {
            // Tile is not passable if actor class cannot swim and depth is more than the height of the actor
            return false;
        }

        return true;


    }

    public float GetTerrainMovementCost(TerrainData targetTerrain)
    {
        if (movementCosts == null)
        {
            // Create new movement cost dictionary if it doesn't exist
            movementCosts = new Dictionary<TerrainData, float>();
        }
        // Try to find movement cost from dictionary
        if (movementCosts.ContainsKey(targetTerrain))
        {
            return movementCosts[targetTerrain];
        }
        else
        {
            // Cost is not calculated for this terrain type. Calculate cost.
            // Base cost is determined by land speed
            float cost = 10f / speed;

            if (targetTerrain.waterDepth < height)
            {
                // Grounded or shallow water
                cost *= 1f + (targetTerrain.ruggedness / ruggedLandNavigation);
                cost *= 1f + (targetTerrain.softness / softLandNavigation);
                cost *= 1f + (targetTerrain.density / crampedNavigation);
            }
            else
            {
                // Use swimming speed instead if water there is deep enough
                cost = 10f / swimmingSpeed;
                cost *= 1f + (targetTerrain.density / crampedNavigation);
            }

            // Save cost for this terrain type
            movementCosts[targetTerrain] = cost;
            return cost;
        }
    }

    #region risks
    public float GetActorClassRiskValues(ActorClass actorClass)
    {
        float risk = 0;
        if (actorClassRiskValues == null)
        {
            // Create new dictionary if it doesn't exist
            actorClassRiskValues = new Dictionary<ActorClass, float>();
        }
        // Try to find value from dictionary
        if (actorClassRiskValues.ContainsKey(actorClass))
        {
            return actorClassRiskValues[actorClass];
        }
        else
        {
            float expectedDamageTaken = GetActorClassExpectedDamage(actorClass);
            float expectedDamageGiven = actorClass.GetActorClassExpectedDamage(this);

            if (expectedDamageGiven <= 0)
            {
                // High risk when there's no way to kill the other actor
                risk = 10;
            }
            else if (expectedDamageTaken <= 0)
            {
                // No risk if other actor cannot kill this
                risk = 0;
            }
            else
            {
                // Calculate how many hits it takes to kill or get killed
                float hitsToDie = maxHitpoints / expectedDamageTaken;
                float hitsToKill = actorClass.maxHitpoints / expectedDamageGiven;

                risk = hitsToKill / hitsToDie;
            }

            actorClassRiskValues[actorClass] = risk;
        }

        return risk;
    }

    public float GetResourceClassRiskValues(ResourceClass resourceClass)
    {
        float risk = 0;
        if (resourceClassRiskValues == null)
        {
            // Create new dictionary if it doesn't exist
            resourceClassRiskValues = new Dictionary<ResourceClass, float>();
        }
        // Try to find value from dictionary
        if (resourceClassRiskValues.ContainsKey(resourceClass))
        {
            return resourceClassRiskValues[resourceClass];
        }
        else
        {
            float expectedDamageTaken = 0;
            foreach (Attack a in resourceClass.hazards)
            {
                expectedDamageTaken += GetAttackExpectedDamage(a);
            }

            // Calculate how many hits it takes to kill or get killed
            if (expectedDamageTaken > 0)
            {
                float hitsToDie = maxHitpoints / expectedDamageTaken;

                risk = 1 / hitsToDie;
            }

            resourceClassRiskValues[resourceClass] = risk;
        }

        return risk;
    }

    public float GetAttackExpectedDamage(Attack attack)
    {
        float expectedDamageTotal = 0;
        if (expectedAttackDamages == null)
        {
            // Create new dictionary if it doesn't exist
            expectedAttackDamages = new Dictionary<Attack, float>();
        }
        // Try to find value from dictionary
        if (expectedAttackDamages.ContainsKey(attack))
        {
            return expectedAttackDamages[attack];
        }
        else
        {
            // Calculate estimated damage
            // Chance to hit
            float hitChance = Mathf.Clamp(1 - evasion / (100f + attack.attackBonus), 0.05f, 0.95f);

            // Loop through every damage in attack
            foreach (Attack.Damage dmg in attack.damage)
            {
                // Get base damage from dice roll and bonus
                float baseDamage = (dmg.damageRoll + 1) * 0.5f + dmg.damageBonus;

                // Get resistance
                DamageTypes type = dmg.damageType;
                float resistance = GetResistance(type);

                // Reduce the damage by resistance
                expectedDamageTotal += hitChance * baseDamage * (1 - resistance);
            }
            // Save expected damage for this attack
            expectedAttackDamages[attack] = expectedDamageTotal;
        }

        return expectedDamageTotal;

    }

    public float GetActorClassExpectedDamage(ActorClass actorClass)
    {
        float expectedDamageTotal = 0;
        if (expectedActorClassDamages == null)
        {
            // Create new dictionary if it doesn't exist
            expectedActorClassDamages = new Dictionary<ActorClass, float>();
        }
        // Try to find value from dictionary
        if (expectedActorClassDamages.ContainsKey(actorClass))
        {
            return expectedActorClassDamages[actorClass];
        }
        else
        {

            // Calculate estimated damage
            // Find highest damage attack
            foreach (Attack attack in actorClass.attacks)
            {
                float attackDamage = GetAttackExpectedDamage(attack);
                if (attackDamage > expectedDamageTotal)
                {
                    expectedDamageTotal = attackDamage;
                }
            }

            // Save estimated damage for this actor class
            expectedActorClassDamages[actorClass] = expectedDamageTotal;
        }

        return expectedDamageTotal;

    }
    #endregion

    #region Detection
    public float GetVisibilityValue(TerrainData targetTerrain)
    {
        if (visibilityValues == null)
        {
            // Create new dictionary if it doesn't exist
            visibilityValues = new Dictionary<TerrainData, float>();
        }
        // Try to find visibility from dictionary
        if (visibilityValues.ContainsKey(targetTerrain))
        {
            return visibilityValues[targetTerrain];
        }
        else
        {
            // Visibility is calculated from actor visibility, terrain cover and camouflage
            float terrainVisibility = visibility;

            terrainVisibility /= 1 + targetTerrain.cover;

            // Own colors
            Vector3 baseColorVec = new Vector3(baseColor.r, baseColor.g, baseColor.b);
            Vector3 patternColorVec = new Vector3(patternColor.r, patternColor.g, patternColor.b);

            // Terrain colors
            Vector3 groundColorVec = new Vector3(targetTerrain.groundColor.r, targetTerrain.groundColor.g, targetTerrain.groundColor.b);
            Vector3 groundSecondaryColorVec = new Vector3(targetTerrain.secondaryColor.r, targetTerrain.secondaryColor.g, targetTerrain.secondaryColor.b);

            float baseVisibility = Mathf.Pow(Vector3.Distance(baseColorVec, groundColorVec), 2) + Mathf.Pow(Vector3.Distance(baseColorVec,groundSecondaryColorVec), 1.5f);
            float patternVisibility = Mathf.Pow(Vector3.Distance(patternColorVec, groundSecondaryColorVec), 2f) + Mathf.Pow(Vector3.Distance(patternColorVec, groundColorVec), 1.5f);

            // Apply camouflage modifier
            terrainVisibility *= 1 + 0.3f * baseVisibility + 0.2f * patternVisibility;

            // Save visibility for this terrain type
            terrainVisibility = Mathf.Max(terrainVisibility, 0.05f);
            visibilityValues[targetTerrain] = terrainVisibility;
            return terrainVisibility;
        }
    }

    public float GetNoiseValue(TerrainData targetTerrain)
    {
        if (noiseValues == null)
        {
            // Create new dictionary if it doesn't exist
            noiseValues = new Dictionary<TerrainData, float>();
        }
        // Try to find noise from dictionary
        if (noiseValues.ContainsKey(targetTerrain))
        {
            return noiseValues[targetTerrain];
        }
        else
        {
            // Noise is terrain noise multiplied by actor noise
            float terrainNoise = targetTerrain.noise * noise;

            terrainNoise = Mathf.Max(terrainNoise, 0.05f);

            // Save noise for this terrain type
            noiseValues[targetTerrain] = terrainNoise;
            return terrainNoise;
        }
    }
    #endregion
}

[System.Serializable]
public class Attack
{
    public string name;
    public int attackBonus;
    public List<Damage> damage;

    /// <summary>
    /// Create a new attack with a single type of damage
    /// </summary>
    /// <param name="attackBonus"></param>
    /// <param name="dmg"></param>
    public Attack(string name, int attackBonus, Damage dmg)
    {
        this.name = name;
        this.attackBonus = attackBonus;

        damage = new List<Damage>();
        damage.Add(dmg);
    }

    /// <summary>
    ///  Create a new attack with specific damage values
    /// </summary>
    public Attack(string name, int attackBonus, DamageTypes dmgType, int dmgRoll, int dmgBonus)
    {
        this.name = name;
        this.attackBonus = attackBonus;

        damage = new List<Damage>();
        damage.Add(new Damage(dmgType, dmgRoll, dmgBonus));
    }

    public void AddDamage(Damage dmg)
    {
        damage.Add(dmg);
    }

    [System.Serializable]
    public struct Damage
    {
        public DamageTypes damageType;
        public int damageRoll;
        public int damageBonus;

        public Damage(DamageTypes dmgType, int dmgRoll, int dmgBonus)
        {
            damageType = dmgType;
            damageRoll = dmgRoll;
            damageBonus = dmgBonus;
        }
    }
}


public enum DamageTypes
{
    None,
    Crushing,
    Piercing,
    Slashing,
    Fire,
    Poison
}
