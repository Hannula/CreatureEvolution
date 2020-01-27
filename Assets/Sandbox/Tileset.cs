using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Tileset
{
    public Texture2D texture;
    public Vector2 textureScale;
    public int columns;
    public string image;
    public int imageheight;
    public int imagewidth;
    public float margin;
    public string name;
    public float spacing;
    public int tilecount;
    public string tiledversion;
    public int tileheight;
    public int tilewidth;
    public string type;
    public float version;

    public void LoadTexture(string tilesetPath)
    {
        string path = Path.Combine(tilesetPath, image);

        // Try to load image file
        if (File.Exists(path))
        {
            // Read bytes
            byte[] imageBytes = File.ReadAllBytes(path);

            texture = new Texture2D(imagewidth, imageheight);
            // Load image to texture
            texture.LoadImage(imageBytes);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Repeat;
        }
        else
        {
            Debug.LogError("Image file not found at '" + path + "'.");
        }
        float tscale = 1f / columns;
        textureScale = new Vector2(tscale, tscale);
    }
}
