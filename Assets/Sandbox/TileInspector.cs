using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sandbox;

public class TileInspector : MonoBehaviour
{
    public Text tileNameText;
    public Text tileText;

    public Text actorNameText;
    public Text actorText;

    public LayerMask raycastTileLayermask;
    public LayerMask raycastActorLayermask;

    public TileVisualizer selectedTile;
    public Actor selectedActor;

    public GameObject actorPanelPrefab;
    public Transform actorPanelContainer;

    private List<ActorPanelManager> actorPanels;

    private void Start()
    {
        actorPanels = new List<ActorPanelManager>();
        StartCoroutine(updateTileInfo());
    }

    void Update()
    {
        // Find tile
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            Physics.Raycast(ray, out hit, raycastTileLayermask);
            try
            {
                selectedTile = hit.collider.transform.parent.GetComponent<TileVisualizer>();
            }
            catch (Exception)
            {

            }
        }

        // Find actor
        Actor newActor = null;
        Physics.Raycast(ray, out hit, raycastActorLayermask);
        if (hit.collider != null)
        {
            try
            {
                newActor = hit.collider.GetComponent<ActorVisualizer>().actor;
            }
            catch (Exception e)
            {

            }
        }

        if (newActor != null)
        {
            selectedActor = newActor;
        }

        // Draw actor info
        if (selectedActor != null && selectedActor.CurrentTile != null)
        {
            actorNameText.text = selectedActor.actorClass.name + "(" + selectedActor.CurrentTile.position.x + ", " + selectedActor.CurrentTile.position.y + ")";
            string info = selectedActor.ToString();


            actorText.text = info;
        }
    }

    void UpdateTileInfo()
    {
        // Draw tile info
        if (selectedTile)
        {
            tileNameText.text = selectedTile.tile.terrain.name + "(" + selectedTile.tile.position.x + ", " + selectedTile.tile.position.y + ")";
            tileText.text = "Elevation: " + selectedTile.tile.elevation.ToString() +
                "\nTemperature: " + selectedTile.tile.temperature.ToString() +
                "\nLight level: " + selectedTile.tile.lightLevel.ToString();

            // Remove existing unlocked panels
            for (int i = actorPanels.Count - 1; i >= 0; --i)
            {
                ActorPanelManager panel = actorPanels[i];
                if (!panel.locked)
                {
                    // Destroy this panel if it's unlocked
                    Destroy(panel.gameObject);
                    actorPanels.RemoveAt(i);
                }
            }


            // Create panel for every actor
            foreach (Actor a in selectedTile.tile.actors)
            {
                bool alreadyContains = ActorsPanelsContainActor(a);

                if (!alreadyContains)
                {
                    GameObject panelObject = Instantiate(actorPanelPrefab, actorPanelContainer);
                    ActorPanelManager panel = panelObject.GetComponent<ActorPanelManager>();
                    panel.actor = a;
                    actorPanels.Add(panel);
                }

            }

        }
    }

    /// <summary>
    /// Checks if given actor is already displayed by some actor panel
    /// </summary>
    /// <param name="a">Actor</param>
    private bool ActorsPanelsContainActor(Actor a)
    {
        foreach (ActorPanelManager panel in actorPanels)
        {
            if (panel.actor == a)
            {
                return true;
            }
        }
        return false;
    }


    IEnumerator updateTileInfo()
    {
        while (true)
        {
            UpdateTileInfo();
            yield return new WaitForSeconds(0.2f);
        }
    }
}
