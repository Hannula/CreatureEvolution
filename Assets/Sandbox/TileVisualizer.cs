using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileVisualizer : MonoBehaviour
{
    public Vector2Int position;
    public Tile tile;
    public Texture2D texture;
    public Tileset tileset;
    public MeshRenderer backgroundMesh;
    private Material material;

    public void Reload()
    {
        // Create new material
        texture = tileset.texture;
        material = new Material(backgroundMesh.material);
        material.SetTextureScale("_MainTex", tileset.textureScale);
        float texturePosition = tile.terrain.id - 1;
        Vector2 offset = new Vector2(
            (texturePosition % tileset.columns) * tileset.textureScale.x,
            1 - Mathf.Floor(texturePosition/ tileset.columns) * tileset.textureScale.y - tileset.textureScale.y
            );

        material.SetTextureOffset("_MainTex", offset);
        material.SetTexture("_MainTex", texture);
        float lightLevel = 0.25f + tile.lightLevel * 0.75f;
        material.color = new Color(lightLevel, lightLevel, lightLevel, 1f);

        backgroundMesh.material = material;

        // Rename transform
        transform.name = tile.terrain.name;

        // Set z according to tile elevation
        transform.position = new Vector3(transform.position.x, transform.position.y, -tile.elevation);
    }

}
