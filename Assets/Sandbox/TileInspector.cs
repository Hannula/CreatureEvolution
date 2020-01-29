using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileInspector : MonoBehaviour
{
    public Text tileNameText;
    public Text tileElevationText;
    public Text tileTemperatureText;

    public TileVisualizer selectedTile;

    void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit);
        selectedTile = hit.collider.transform.parent.GetComponent<TileVisualizer>();
        
        

        if (selectedTile)
        {
            tileNameText.text = selectedTile.tile.terrain.name;
            tileElevationText.text = selectedTile.tile.elevation.ToString();
            tileTemperatureText.text = selectedTile.tile.temperature.ToString();
        }
    }
}
