using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGenerator : MonoBehaviour
{
    public Texture2D GenerateTextureMap(float[][] valueMap)
    {
        // Create texture to write value map to
        int width, height;
        width = height = (int) Mathf.Floor(valueMap.Length / 2);    // TODO: How to get width and height separately?
        Texture2D tex = new Texture2D(width, height);

        // Loop over all pixels
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                // TODO
            }
        }

        return tex;
    }
}
