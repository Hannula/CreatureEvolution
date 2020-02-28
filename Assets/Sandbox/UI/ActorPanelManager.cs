using Sandbox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActorPanelManager : MonoBehaviour
{
    public Text actorNameText;
    public Text actorInfoText;

    public Button lockButton;
    private Text lockButtonText;

    public Actor actor;

    public bool locked = false;

    void Start()
    {
        lockButtonText = lockButton.GetComponentInChildren<Text>();
    }
    void Update()
    {
        actorNameText.text = actor.actorClass.name + "(" + actor.CurrentTile.position.x + ", " + actor.CurrentTile.position.y + ")";
        actorInfoText.text = "Hitpoints: " + Mathf.Ceil(actor.Hitpoints) + "/" + actor.actorClass.maxHitpoints +
            "\nHunger: " + Mathf.Ceil(actor.Hunger) +
            "\nEnergy: " + actor.Energy;
    }

    public void ToggleLock()
    {
        locked = !locked;
        if (locked)
        {
            lockButton.image.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            lockButtonText.text = "Unlock";
        }
        else
        {
            lockButton.image.color = new Color(1f, 1f, 1f, 1f);
            lockButtonText.text = "Lock";
        }
    }

    public void Info()
    {
        FindObjectOfType<TileInspector>().selectedActor = actor;
    }
}
