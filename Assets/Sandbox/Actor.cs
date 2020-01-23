using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Actor
{
    public Vector2Int levelPosition;
    public float initiative = 0;

    public int hitpoints;
    public int maxHitpoints;
    public int speed;
    public int armorClass;
    public float size;
    public Dictionary<DamageTypes, float> resistances;
    public List<Attack> attacks;

    public Actor()
    {
        maxHitpoints = 100;
        hitpoints = maxHitpoints;
        initiative = 10;
        speed = 15;
        armorClass = 10;
        size = 4;

    }

    public void PerformAttack(Attack attack, Actor target)
    {
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
            if (toHit >= target.armorClass)
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
            foreach(Attack.Damage dmg in attack.damage)
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
    public int attackBonus;
    public List<Damage> damage;

    [System.Serializable]
    public struct Damage
    {
        public DamageTypes damageType;
        public int damageRoll;
        public int damageBonus;
    }
}


public enum DamageTypes
{
    Crushing,
    Percing,
    Slashing
}