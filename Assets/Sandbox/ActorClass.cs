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
    public int armorClass;
    public float size;
    public Dictionary<DamageTypes, float> resistances;
    [SerializeField]
    private List<string> resistanceStrings;
    public List<Attack> attacks;

    public float swimmingSpeed;
    public float ruggedLandNavigation;
    public float softLandNavigation;
    public float crampedNavigation;
    public float steepNavigation;

    public readonly float height;

    private Dictionary<TerrainData, float> movementCosts;

    public ActorClass(string name, int hitpoints, int speed, int armorClass, int size, float swimming, float rugged, float soft, float cramped)
    {
        this.name = name;
        maxHitpoints = hitpoints;
        this.speed = speed;
        this.armorClass = armorClass;
        this.size = size;
        swimmingSpeed = swimming;
        ruggedLandNavigation = rugged;
        softLandNavigation = soft;
        crampedNavigation = cramped;
        attacks = new List<Attack>();
        resistances = new Dictionary<DamageTypes, float>();
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
            float cost = 1f / speed;

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
                cost = 1f / swimmingSpeed;
                cost *= 1f + (targetTerrain.density / crampedNavigation);
            }

            // Save cost for this terrain type
            movementCosts[targetTerrain] = cost;
            return cost;
        }
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
    Crushing,
    Percing,
    Slashing,
    Fire,
    Poison
}
