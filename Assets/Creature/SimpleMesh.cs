using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMesh : MonoBehaviour
{
    public int steps = 4;

    public List<Lump> meshLumps;

    private MeshRenderer meshRenderer;

    public MeshFilter meshFilter;

    private Rect bounds;

    private void Update()
    {
        GenerateMesh();
    }

    private void GenerateMesh()
    {
        CalculateBounds();
        Mesh newMesh = new Mesh();
        newMesh.vertices = GetVertices();

        newMesh.triangles = GetTriangles(newMesh.vertices.Length);
        meshFilter.mesh = newMesh;
    }

    private Vector3[] GetVertices()
    {
        List<Vector3> vertices = new List<Vector3>();
        float angleDifference = Mathf.PI / (steps + 1);
        float stepSize = bounds.height / (steps + 1);

        // Create add steps times new vertices
        for (int i = 1; i <= steps; i++)
        {
            float y = bounds.yMin + i * stepSize;
            float x = GetMaxX(y);

            // Only add vertex if x is not 0
            //if (x != 0)
            {
                Vector3 vertex = new Vector3(x, y, 0);
                AddVertex(vertex, vertices);
            }

        }

        return vertices.ToArray();
    }

    private void AddVertex(Vector3 vertex, List<Vector3> vertices)
    {
        // Center vertex
        vertices.Add(new Vector3(0, vertex.y));
        // Vertex itself
        vertices.Add(vertex);
        // X-mirrored vertex
        vertices.Add(new Vector3(-vertex.x, vertex.y));
    }

    private float GetMaxX(float y)
    {
        float x = 0;
        // Check max x for every lump
        foreach (Lump lump in meshLumps)
        {
            float lumpX = lump.GetX(y);
            // If lump x is higher than the previous x, set is as the new x
            if (lumpX > x)
            {
                x = lumpX;
            }
        }
        return x;
    }

    private int[] GetTriangles(int numberOfVertices)
    {
        int triCount = (numberOfVertices - 3) / 3 * 4;
        int[] tris = new int[triCount * 3];
        for (int i = 0; i < triCount / 4; i++)
        {
            // First triangle index
            int t = i * 12;
            // First vertex index
            int v = i * 3;

            // Top right
            tris[t] = i;
            tris[t + 1] = i + 1;
            tris[t + 2] = i + 3;

            // Top left
            tris[t + 3] = i;
            tris[t + 4] = i + 3;
            tris[t + 5] = i + 2;

            // Bottom right
            tris[t + 6] = i + 3;
            tris[t + 7] = i + 1;
            tris[t + 8] = i + 4;

            // Bottom left
            tris[t + 9] = i + 3;
            tris[t + 10] = i + 5;
            tris[t + 11] = i + 2;
        }

        return tris;
    }

    private void CalculateBounds()
    {
        bounds = new Rect(0, 0, 0, 0);

        foreach (Lump meshLump in meshLumps)
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

        public float GetX(float yPos)
        {
            // Relative y
            float relativeY = Mathf.Abs(position.y - yPos);

            // If y is outside of the lump radius, return 0
            if (relativeY > radius)
            {
                return 0;
            }
            // If y is inside lump radius, calculate x
            return Mathf.Sqrt(Mathf.Pow(relativeY, 2) - Mathf.Pow(radius, 2));
        }
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
                Gizmos.DrawWireSphere(transform.position + new Vector3(-meshLump.position.x, meshLump.position.y, 0), meshLump.radius);
            }
        }
        Gizmos.DrawWireCube(transform.position + (Vector3)bounds.center, bounds.size);

        // Draw origin
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.2f, transform.position + Vector3.down * 0.2f);
        Gizmos.DrawLine(transform.position + Vector3.left * 0.2f, transform.position + Vector3.right * 0.2f);
        if (meshFilter.mesh != null)
        {
            foreach (Vector3 vert in meshFilter.mesh.vertices)
            {
                Gizmos.DrawCube(transform.position + vert, Vector3.one * 0.05f);
            }
        }
    }

    private string VerticesToString(IEnumerable<Vector3> vertices)
    {
        string str = "[";
        foreach (Vector3 vertex in vertices)
        {
            str += vertex.ToString() + ", ";
        }
        return str + "]";
    }
}
