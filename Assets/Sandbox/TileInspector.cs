using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileInspector : MonoBehaviour
{
    public Text tileNameText;
    public Text tileText;

    public Text actorNameText;
    public Text actorText;

    public LayerMask raycastTileLayermask;
    public LayerMask raycastActorLayermask;

    public TileVisualizer selectedTile;
    public ActorVisualizer selectedActor;

    void Update()
    {
        // Find tile
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit, raycastTileLayermask);
        try
        {
            selectedTile = hit.collider.transform.parent.GetComponent<TileVisualizer>();
        }
        catch(Exception)
        {

        }

        // Find actor
        ActorVisualizer newActor = null;
        Physics.Raycast(ray, out hit, raycastActorLayermask);
        if (hit.collider != null)
            newActor = hit.collider.GetComponent<ActorVisualizer>();

        if (newActor != null)
        {
            selectedActor = newActor;
        }
        // Draw tile info
        if (selectedTile)
        {
            tileNameText.text = selectedTile.tile.terrain.name + "(" + selectedTile.tile.position.x + ", " + selectedTile.tile.position.y + ")";
            tileText.text = "Elevation: " + selectedTile.tile.elevation.ToString() +
                "\nTemperature: " + selectedTile.tile.temperature.ToString() +
                "\nLight level: " + selectedTile.tile.lightLevel.ToString();
        }

        // Draw actor info
        if (selectedActor)
        {
            actorNameText.text = selectedActor.actor.actorClass.name + "(" + selectedTile.tile.position.x + ", " + selectedTile.tile.position.y + ")";
            string info = "Hitpoints: " + Mathf.Ceil(selectedActor.actor.hitpoints) + "/" + selectedActor.actor.actorClass.maxHitpoints +
                "\nHunger: " + Mathf.Ceil(selectedActor.actor.hunger) +
                "\nEnergy: " + selectedActor.actor.energy +
                "\nResistances: \n";
            // Find every resistance
            foreach(DamageTypes dmgType in selectedActor.actor.actorClass.resistances.Keys)
            {
                float value = Mathf.Floor(selectedActor.actor.actorClass.resistances[dmgType] * 100);
                string text = dmgType.ToString() + ": " + value + "%\n";

                info += text;
            }
            actorText.text = info;
        }
    }
}
