using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    private List<Actor> actors;

    private void SimulateRound()
    {

    }

    public void Start()
    {
        Actor erkki = new Actor();
        string actorJson = JsonUtility.ToJson(erkki);
        Debug.Log(actorJson);
    }
}
