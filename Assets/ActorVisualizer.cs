using Sandbox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorVisualizer : MonoBehaviour
{

    public Vector2Int position;
    public Actor actor;
    public Texture2D texture;
    public Tileset tileset;
    public MeshRenderer backgroundMesh;
    private Material material;
    public Level level;

    // Always face camera
    void Update()
    {
        UpdatePosition();
        transform.rotation = Camera.main.transform.rotation;
    }

    public void Reload()
    {
        // Create new material
        texture = tileset.texture;
        material = new Material(backgroundMesh.material);
        material.SetTextureScale("_MainTex", tileset.textureScale);
        float texturePosition = actor.actorClass.id - 1;
        Vector2 offset = new Vector2(
            (texturePosition % tileset.columns) * tileset.textureScale.x,
            1 - Mathf.Floor(texturePosition / tileset.columns) * tileset.textureScale.y - tileset.textureScale.y
            );

        material.SetTextureOffset("_MainTex", offset);
        material.SetTexture("_MainTex", texture);

        backgroundMesh.material = material;

        // Rename transform
        transform.name = actor.actorClass.name;

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector2Int levelPosition = actor.levelPosition;
        float elevation = level.TileAt(levelPosition.x, levelPosition.y).elevation;

        transform.position = new Vector3(levelPosition.x + 0.5f, levelPosition.y + 0.5f, elevation -1);
    }
}
