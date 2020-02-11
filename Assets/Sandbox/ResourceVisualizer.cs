using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sandbox;

public class ResourceVisualizer : MonoBehaviour
{

    public Vector2Int position;
    public Resource resource;
    public Texture2D texture;
    public Tileset tileset;
    public MeshRenderer backgroundMesh;
    private Material material;
    public Level level;

    private void Start()
    {
        Reload();
    }

    // Always face camera
    void Update()
    {
        if (resource.amount > 0)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void Reload()
    {
        // Create new material
        texture = tileset.texture;
        material = new Material(backgroundMesh.material);
        material.SetTextureScale("_MainTex", tileset.textureScale);
        float texturePosition = resource.resourceClass.id - 1;
        Vector2 offset = new Vector2(
            (texturePosition % tileset.columns) * tileset.textureScale.x,
            1 - Mathf.Floor(texturePosition / tileset.columns) * tileset.textureScale.y - tileset.textureScale.y
            );

        material.SetTextureOffset("_MainTex", offset);
        material.SetTexture("_MainTex", texture);

        backgroundMesh.material = material;

        // Rename transform
        transform.name = resource.resourceClass.name;

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector2Int levelPosition = resource.LevelPosition;
        float elevation = resource.currentTile.elevation;

        transform.position = new Vector3(levelPosition.x, -levelPosition.y, -elevation - 1);
    }

}
