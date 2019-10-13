using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMesh : MonoBehaviour
{
    public int steps = 4;

    public float offset = 0.01f;

    public List<Lump> meshLumps;

    private MeshRenderer meshRenderer;

    public MeshFilter meshFilter;

    private Rect bounds;

    private void Update()
    {
        GenerateMesh();
    }

    public void SetLumps(List<Vector3> lumps)
    {
        meshLumps.Clear();
        foreach (Vector3 lump in lumps)
        {
            Lump newLump = new Lump();
            newLump.position = new Vector2(lump.x, lump.y);
            newLump.radius = lump.z;
            meshLumps.Add(newLump);
        }
    }

    private void GenerateMesh()
    {
        bool success = false;
        // Only generate if there are lumps and they contain vertices
        if (meshLumps.Count >= 0)
        {
            CalculateBounds();
            Mesh newMesh = new Mesh();
            newMesh.vertices = GetVertices();

            if (newMesh.vertices.Length > 0)
            {
                newMesh.triangles = GetTriangles(newMesh.vertices.Length);
                newMesh.uv = GetUVs(newMesh.vertices);
                meshFilter.mesh = newMesh;
                success = true;
            }
        }

        if (!success)
        {
            // Otherwise use no mesh at all
            meshFilter.mesh = null;
        }
    }

    private Vector3[] GetVertices()
    {
        List<Vector3> vertices = new List<Vector3>();
        float startOffset = bounds.height * offset;
        float stepSize = (bounds.height - startOffset * 2) / (steps - 1);


        // Create add steps times new vertices
        for (int i = 0; i < steps; i++)
        {
            float y = bounds.yMin + startOffset +  i * stepSize;
            float x = GetMaxX(y);

            // Only add vertex if x is not 0
            if (x != 0)
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
            tris[t] = v;
            tris[t + 1] = v + 3;
            tris[t + 2] = v + 1;

            // Top left
            tris[t + 3] = v;
            tris[t + 4] = v + 2;
            tris[t + 5] = v + 3;

            // Bottom right
            tris[t + 6] = v + 3;
            tris[t + 7] = v + 4;
            tris[t + 8] = v + 1;

            // Bottom left
            tris[t + 9] = v + 3;
            tris[t + 10] = v + 2;
            tris[t + 11] = v + 5;
        }

        return tris;
    }

    private Vector2[] GetUVs(Vector3[] verts)
    {
        // Store UVs in new array
        Vector2[] uvs = new Vector2[verts.Length];

        float halfWidth = bounds.xMax;
        float maxY = bounds.yMax;
        float minY = bounds.yMin;

        float height = maxY - minY;

        for (int i = 0; i < verts.Length; i += 3)
        {
            // Get row of vertices
            Vector3 center = verts[i];
            Vector3 right = verts[i + 1];
            Vector3 left = verts[i + 2];

            // Set UVs
            float y = (center.y - minY) / height;
            // Center
            uvs[i] = new Vector2(0.5f, y);
            // Right
            uvs[i + 1] = new Vector2(1, y);
            // Left
            uvs[i + 2] = new Vector2(0, y);

        }
        return uvs;
    }

    private void CalculateBounds()
    {
        bounds = new Rect(0, 0, 0, 0);
        if (meshLumps.Count > 0)
        {
            bounds.xMax = float.MinValue;
            bounds.xMin = float.MaxValue;

            bounds.yMax = float.MinValue;
            bounds.yMin = float.MaxValue;
        }
         
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
            float relativeY = Mathf.Abs(yPos - position.y);

            if (relativeY < radius)
            {
                // If y is inside lump radius, calculate x
                float x = Mathf.Abs(position.x) + Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(relativeY, 2));
                return Mathf.Abs(x);
            }
            // If y is outside of the lump radius, return 0
            return 0;

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
        if (meshFilter.sharedMesh != null)
        {
            foreach (Vector3 vert in meshFilter.sharedMesh.vertices)
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
