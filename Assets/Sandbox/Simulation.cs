using Sandbox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    private List<Actor> actors;

    private Level level;

    private void SimulateRound()
    {

    }

    public static void Log(string text)
    {
        Debug.Log(text);
    }

    public void Start()
    {
        ActorClass human = new ActorClass("Human", 100, 15, 10, 5);
        Attack punch = new Attack("Punch", 3, new Attack.Damage(DamageTypes.Crushing, 10, 5));
        Attack kick = new Attack("Kick", 0, new Attack.Damage(DamageTypes.Crushing, 15, 5));
        human.AddAttacks(punch, kick);

        Actor erkki = new Actor(human, "Erkki");
        Actor pertti = new Actor(human, "Pertti");

        int tries = 0;
        while (erkki.hitpoints > 0 && pertti.hitpoints > 0 && tries < 100)
        {
            erkki.PerformAttack(erkki.GetRandomAttack(), pertti);
            pertti.PerformAttack(pertti.GetRandomAttack(), erkki);
            tries++;
        }
    }
}
