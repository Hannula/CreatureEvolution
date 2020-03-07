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
    public GameObject customMesh;
    private bool showPath;
    private bool showMemory;
    private CreatureChromosome creatureChromosome;
    private TileInspector inspector;

    public MeshRenderer bodyRenderer;
    public MeshRenderer headRenderer;

    private void Start()
    {
        inspector = FindObjectOfType<TileInspector>();
    }
    // Always face camera
    void Update()
    {
        if (actor.Hitpoints > 0)
        {
            transform.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
            UpdatePosition();
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, -1, transform.localScale.z);
        }

        backgroundMesh.enabled = actor.MeatAmount > 0;

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

        if (actor.ActorClass.CreatureChromosome == null)
        {
            // Add texture for default actors
            // Create new material
            texture = tileset.texture;
            material = new Material(backgroundMesh.material);
            material.SetTextureScale("_MainTex", tileset.textureScale);
            float texturePosition = actor.ActorClass.id - 1;
            Vector2 offset = new Vector2(
                (texturePosition % tileset.columns) * tileset.textureScale.x,
                1 - Mathf.Floor(texturePosition / tileset.columns) * tileset.textureScale.y - tileset.textureScale.y
                );

            material.SetTextureOffset("_MainTex", offset);
            material.SetTexture("_MainTex", texture);

            backgroundMesh.material = material;
        }
        else
        {
            // Enable custom visuals for evolution creature
            customMesh.SetActive(true);

            CreatureChromosome chromosome = actor.ActorClass.CreatureChromosome;
            float size = chromosome.GetGeneValue(CreatureGeneKeys.Size) / 10f;
            customMesh.transform.localScale = new Vector3(size, size, size);

            // Body
            bodyRenderer.material.color = actor.ActorClass.baseColor;
            bodyRenderer.sharedMaterials[1].color = actor.ActorClass.patternColor;

        }

        // Rename transform
        transform.name = actor.ActorClass.name;

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
        if (inspector && inspector.selectedActor == actor)
        {
            Gizmos.color = Color.green;
            if (actor.CurrentPath != null && actor.Hitpoints > 0 && showPath)
            {
                Vector3 prev = transform.position;
                foreach (Tile t in actor.CurrentPath)
                {
                    Vector3 next = new Vector3(t.position.x, -t.position.y, -t.elevation - 1f);
                    Gizmos.DrawLine(prev, next);
                    Gizmos.DrawSphere(prev, 0.1f);
                    prev = next;
                }
            }

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
                        Gizmos.DrawLine(new Vector3(mem.Tile.position.x, -mem.Tile.position.y, -1f), new Vector3(a.LevelPosition.x, -a.LevelPosition.y, -1f));
                    }
                }
            }
        }
    }
}
