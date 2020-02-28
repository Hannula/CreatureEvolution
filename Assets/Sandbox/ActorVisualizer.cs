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

    private bool showPath;
    private bool showMemory;

    private TileInspector inspector;

    private void Start()
    {
        inspector = FindObjectOfType<TileInspector>();
    }
    // Always face camera
    void Update()
    {
        if (actor.Hitpoints > 0)
        {
            UpdatePosition();
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, -1, transform.localScale.z);
            if (actor.MeatAmount <= 0)
            {
                enabled = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            showMemory = !showMemory;
        }
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showPath = !showPath;
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
        float elevation = actor.CurrentTile.elevation;

        transform.position = new Vector3(levelPosition.x, -levelPosition.y, -elevation - 1);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (actor.CurrentPath != null && actor.Hitpoints > 0 && showPath)
        {
            Vector3 prev = transform.position;
            foreach (Tile t in actor.CurrentPath)
            {
                Vector3 next = new Vector3(t.position.x, -t.position.y, -t.elevation - 1f);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }
        if (inspector && inspector.selectedActor == actor)
        {
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.75f);
            if (showMemory)
            {
                foreach (Actor a in actor.ActorMemory.Keys)
                {
                    Memory mem = actor.ActorMemory[a];
                    float age = Mathf.Max(1, a.Age - mem.Time);
                    if (age < actor.MemoryLength)
                    {
                        float colorValue = age / actor.MemoryLength;
                        Gizmos.color = new Color(1, 1 - colorValue, 0);
                        Gizmos.DrawLine(new Vector3(mem.Tile.position.x, -mem.Tile.position.y, -2f), new Vector3(a.LevelPosition.x, -a.LevelPosition.y, -2f));
                    }
                }
            }
        }
    }
}
