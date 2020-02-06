using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileInspector : MonoBehaviour
{
    public Text tileNameText;
    public Text tileText;

    public LayerMask raycastLayermask;

    public TileVisualizer selectedTile;

    void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit, raycastLayermask);
        if (hit.collider != null)
            selectedTile = hit.collider.transform.parent.GetComponent<TileVisualizer>();



        if (selectedTile)
        {
            tileNameText.text = selectedTile.tile.terrain.name + "(" + selectedTile.tile.position.x + ", " + selectedTile.tile.position.y + ")";
            tileText.text = "Elevation: " + selectedTile.tile.elevation.ToString() +
                "\nTemperature: " + selectedTile.tile.temperature.ToString();
        }
    }
}
