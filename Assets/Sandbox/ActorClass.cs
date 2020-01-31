using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActorClass
{
    public readonly string name;
    public int id;
    public readonly Color baseColor;
    public readonly Color patternColor;

    public readonly float maxHitpoints;
    public readonly float speed;
    public readonly int armorClass;
    public readonly float size;
    public readonly Dictionary<DamageTypes, float> resistances;
    [SerializeField]
    private List<string> resistanceStrings;
    public List<Attack> attacks;

    public readonly float swimmingSpeed;

    public readonly float ruggedLandNavigation;
    public readonly float softLandNavigation;
    public readonly float crampedNavigation;

    public readonly float height;

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
