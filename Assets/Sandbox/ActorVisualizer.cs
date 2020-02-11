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
        if (actor.hitpoints > 0)
        {
            UpdatePosition();
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
        Vector2Int levelPosition = actor.LevelPosition;
        float elevation = actor.currentTile.elevation;

        transform.position = new Vector3(levelPosition.x, -levelPosition.y, -elevation -1);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (actor.currentPath != null)
        {
            Vector3 prev = transform.position;
            foreach(Tile t in actor.currentPath)
            {
                Vector3 next = new Vector3(t.position.x, -t.position.y, -t.elevation - 1f);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }
    }
}
