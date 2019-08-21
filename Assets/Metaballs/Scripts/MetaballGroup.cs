using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MetaballGroup : MonoBehaviour {

    public SpriteRenderer spriteRenderer;

    private List<Metaball> metaballs;

    [Range(0,1f)]
    public float threshold = 0.25f;

    public float pixelsPerUnit = 10f;

    public Vector2 boundsMin;
    public Vector2 boundsMax;

    private Texture2D texture;

    private float boundsWidth;
    private float boundsHeight;

    void Start () {

        if (!spriteRenderer)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Update content
        InvokeRepeating("UpdateContent", 0.1f, 1f);
    }
	
    void UpdateContent()
    {
        // Get a list of metaball that is contained by this metaball group
        metaballs = GetComponentsInChildren<Metaball>().ToList();

        UpdateTexture(metaballs);
    }

    void RecalculateBounds()
    {
        ResetBounds();
        // Update bounds to match the metaballs
        foreach (Metaball metaball in metaballs)
        {
            // Min bounds
            if (boundsMin.x > metaball.transform.position.x - metaball.radius)
            {
                boundsMin.x = metaball.transform.position.x - metaball.radius;
            }

            if (boundsMin.y > metaball.transform.position.y - metaball.radius)
            {
                boundsMin.y = metaball.transform.position.y - metaball.radius;
            }

            // Max bounds
            if (boundsMax.x < metaball.transform.position.x + metaball.radius)
            {
                boundsMax.x = metaball.transform.position.x + metaball.radius;
            }

            if (boundsMax.y < metaball.transform.position.y + metaball.radius)
            {
                boundsMax.y = metaball.transform.position.y + metaball.radius;
            }
        }
    }

    /// <summary>
    /// Reset bounds to negative max values
    /// </summary>
    public void ResetBounds()
    {
        boundsMin = new Vector2(float.MaxValue, float.MaxValue);
        boundsMax = new Vector2(float.MinValue, float.MinValue);
    }

    /// <summary>
    /// Update texture bounds to match the metaballs
    /// </summary>
    /// <param name="metaballs"></param>
    public void UpdateTexture(List<Metaball> metaballs)
    {
        if (metaballs.Count > 0)
        {
            RecalculateBounds();

            boundsWidth = boundsMax.x - boundsMin.x;
            boundsHeight = boundsMax.y - boundsMin.y;

            Vector2 textureSize = new Vector2(boundsWidth * pixelsPerUnit, boundsHeight * pixelsPerUnit);

            // Create new texture
            texture = new Texture2D(Mathf.CeilToInt(textureSize.x), Mathf.CeilToInt(textureSize.y),TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            Color[] pixelColors = new Color[texture.width * texture.height];

            float xStep = boundsWidth / texture.width;
            float yStep = boundsHeight / texture.height;

            int i = 0;
            // Calculate color for every pixel on the texture
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Color col = GetPointColor(boundsMin.x + x * xStep, boundsMin.y + y * yStep );
                    pixelColors[i++] = col;
                }
            }
            Debug.Log(i);

            texture.SetPixels(pixelColors);
            texture.Apply();
        }
        Sprite spr = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2((transform.position.x - boundsMin.x) / boundsWidth, (transform.position.y - boundsMin.y) / boundsHeight), pixelsPerUnit);
        spriteRenderer.sprite = spr;
        Debug.Log(texture.width + ", " + texture.height);

    }

    private Color GetPointColor(Vector2 worldPosition)
    {
        float value = 0f;
        Color col = new Color(0f, 0f, 0f, 0f);

        foreach (Metaball metaball in metaballs)
        {
            value += metaball.ValueAt(worldPosition);
        }
        if (value > threshold)
        {
            col.a = 1f;
        }
        else
        {
            col.a = 0.5f;
        }

        return col;
    }

    private Color GetPointColor(float x, float y)
    {
        return GetPointColor(new Vector2(x, y));
    }

}
