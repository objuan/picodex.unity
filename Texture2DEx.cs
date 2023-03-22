using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//namespace Assets.Scripts.Utils


public static class Texture2DEx
{
	public static Texture2D GetRectangle( this Texture2D txt,Rect bound)
	{
        int w = txt.width;
        int h = txt.height;
        int w2 = (int) bound.width;
        int h2 = (int)bound.height;
        Color[] source = txt.GetPixels();
        Color[] dest = new Color[w2 * h2];
        int sx = (int)(bound.x);
        int sy = (int)(bound.y);
        for (int y = 0, i = 0; y < h2; y++)
        {
            for (int x = 0; x < w2; x++)
            {
                dest[x + y * w2] = source[sx + x + (sy + y) * w];
            }
        }

        var sprite = new Texture2D(w2, h2);
        sprite.SetPixels(dest);
        sprite.Apply();
        return sprite;
    }

    public static Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;

    }
    public static Texture2D MakeTex(int width, int height, Color32 col)
    {
        Color32[] pix = new Color32[width * height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels32(pix);
        result.Apply();

        return result;

    }
}

