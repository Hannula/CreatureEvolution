using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActorClass
{
    public string name;
    public int id;
    public Color baseColor;
    public Color patternColor;

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

    public float meatAmount;


    public float height;

    private Dictionary<TerrainData, float> movementCosts;

    private Dictionary<ActorClass, float> actorClassRiskValues;
    private Dictionary<ResourceClass, float> resourceValues;
    private Dictionary<Attack, float> expectedDamages;


    public ActorClass(string name, int hitpoints, int speed, int armorClass, int size, float swimming, float rugged, float soft, float cramped)
    {
        this.name = name;
        maxHitpoints = hitpoints;
        this.speed = speed;
        this.evasion = armorClass;
        this.size = size;
        swimmingSpeed = swimming;
        ruggedLandNavigation = rugged;
        softLandNavigation = soft;
        crampedNavigation = cramped;
        attacks = new List<Attack>();
        resistances = new Dictionary<DamageTypes, float>();
    }

    public void ParseResistances()
    {
        resistances = new Dictionary<DamageTypes, float>();
        foreach(string resistanceString in resistanceStrings)
        {
            try
            {
                string[] parts = resistanceString.Split(':');
                // Find damage type
                DamageTypes type = DamageTypes.None;
                foreach(DamageTypes damageType in Enum.GetValues(typeof(DamageTypes)))
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
            catch(Exception)
            {
                Simulation.Log("Failed to parse resistance strings. Correct format is \"DamageType:ResistanceValue\".");
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
            // Calculate risk value

        }

        return risk;
    }

    public float GetAttackExpectedDamage(Attack attack)
    {
        float damage = 0;
        if (expectedDamages == null)
        {
            // Create new dictionary if it doesn't exist
            expectedDamages = new Dictionary<Attack, float>();
        }
        // Try to find value from dictionary
        if (expectedDamages.ContainsKey(attack))
        {
            return expectedDamages[attack];
        }
        else
        {
            // Calculate estimated damage

        }

        return damage;

    }

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
