using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TileVisualizer : MonoBehaviour
{
    public Vector2Int position;
    public Tile tile;
    public Texture2D texture;
    public Tileset tileset;
    public MeshRenderer backgroundMesh;
    public TextMeshPro text;
    private Material material;
    private TileInspector inspector;

    public void Start()
    {
        inspector = FindObjectOfType<TileInspector>();
    }

    public void Update()
    {
        // Draw movement cost if actor is selected
        if (inspector && inspector.selectedActor != null && tile != null && Input.GetKey(KeyCode.Tab))
        {
            text.text = "Cost: " + inspector.selectedActor.GetMovementCost(tile, tile) +
                    "\nCostRisk: " + inspector.selectedActor.GetMovementCostRisk(tile, tile) +
                    "\nVisibility: " + inspector.selectedActor.actorClass.GetVisibilityValue(tile.terrain) +
                    "\nNoise: " + inspector.selectedActor.actorClass.GetNoiseValue(tile.terrain);
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            text.text = "";
        }
    }
    public void Reload()
    {
        // Create new material
        texture = tileset.texture;
        material = new Material(backgroundMesh.material);
        material.SetTextureScale("_MainTex", tileset.textureScale);
        float texturePosition = tile.terrain.id - 1;
        Vector2 offset = new Vector2(
            (texturePosition % tileset.columns) * tileset.textureScale.x,
            1 - Mathf.Floor(texturePosition / tileset.columns) * tileset.textureScale.y - tileset.textureScale.y
            );

        material.SetTextureOffset("_MainTex", offset);
        material.SetTexture("_MainTex", texture);
        float lightLevel = 0.25f + tile.lightLevel * 0.75f;
        float heat = tile.temperature * 0.005f;
        if (heat > 0)
        {
            material.color = new Color(lightLevel, lightLevel - heat * 0.5f, lightLevel - heat, 1f);
        }
        else if (heat < 0)
        {
            material.color = new Color(lightLevel + heat, lightLevel + heat, lightLevel, 1f);
        }

        backgroundMesh.material = material;

        // Rename transform
        transform.name = tile.terrain.name;

        // Set z according to tile elevation
        transform.position = new Vector3(transform.position.x, transform.position.y, -tile.elevation);
    }

    public void OnDrawGizmos()
    {
        if (inspector && inspector.selectedTile == this)
        {
            // Draw marker
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 1.25f);
        }
    }

}
