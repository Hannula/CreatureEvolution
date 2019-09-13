using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMesh : MonoBehaviour
{
    public int numberOfSteps = 10;

    public List<Lump> meshLumps;

    private MeshRenderer meshRenderer;

    private MeshFilter meshFilter;

    private Rect bounds;

    private void Update()
    {
        CalculateBounds();
    }

    private void GenerateMesh()
    {
        CalculateBounds();
    }

    private Vector3[] GetVertices()
    {
        Vector3[] vertices = new Vector3[numberOfSteps * 2];


        return vertices;
    }

    private void CalculateBounds()
    {
        bounds = new Rect(0, 0, 0, 0);

        foreach(Lump meshLump in meshLumps)
        {
            // X-axis
            if (meshLump.position.x - meshLump.radius < bounds.xMin)
            {
                bounds.xMin = meshLump.position.x - meshLump.radius;
            }
            if (meshLump.position.x + meshLump.radius > bounds.xMax)
            {
                bounds.xMax = meshLump.position.x + meshLump.radius;
            }

            // Mirrored X-axis
            if (meshLump.position.x != 0)
            {
                // X-axis
                if (-meshLump.position.x - meshLump.radius < bounds.xMin)
                {
                    bounds.xMin = -meshLump.position.x - meshLump.radius;
                }
                if (-meshLump.position.x + meshLump.radius > bounds.xMax)
                {
                    bounds.xMax = -meshLump.position.x + meshLump.radius;
                }
            }

            // Y-axis
            if (meshLump.position.y - meshLump.radius < bounds.yMin)
            {
                bounds.yMin = meshLump.position.y - meshLump.radius;
            }
            if (meshLump.position.y + meshLump.radius > bounds.yMax)
            {
                bounds.yMax = meshLump.position.y + meshLump.radius;
            }
        }
    }


    [System.Serializable]
    public struct Lump
    {
        public Vector2 position;
        public float radius;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        foreach (Lump meshLump in meshLumps)
        {
            Gizmos.DrawWireSphere(transform.position + (Vector3)meshLump.position, meshLump.radius);

            // Draw mirrored x
            if (meshLump.position.x != 0)
            {
                Gizmos.DrawWireSphere(transform.position + new Vector3(-meshLump.position.x,meshLump.position.y, 0), meshLump.radius);
            }
        }
        Gizmos.DrawWireCube(transform.position + (Vector3)bounds.center, bounds.size);

        // Draw origin
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.2f, transform.position + Vector3.down * 0.2f);
        Gizmos.DrawLine(transform.position + Vector3.left * 0.2f, transform.position + Vector3.right * 0.2f);

    }
}
