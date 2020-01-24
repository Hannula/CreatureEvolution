using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Actor
{
    public string name;

    public Vector2Int levelPosition;
    public float energy = 0;

    public int hitpoints;
    public readonly ActorClass actorClass;

    public Actor(ActorClass actorClass, string name = "Actor")
    {
        this.name = name;
        this.actorClass = actorClass;
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

}