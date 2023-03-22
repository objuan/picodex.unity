using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class DynamicTexture
{
	public Texture2D texture;

	Sprite _sprite = null;

	public Sprite sprite
	{
		get
		{
			if (_sprite==null)
			{
				_sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.0f), 1.0f);
			}
			return _sprite;
		}
	}

	public static DynamicTexture FromPath(string path)
	{
		DynamicTexture txt = new DynamicTexture();

		try
		{
			byte[] bytes = BetterStreamingAssets.ReadAllBytes(path);
			//txt.texture = new Texture2D(width, height);
			txt.texture = new Texture2D(2,2, TextureFormat.RGBA32, false);
			txt.texture.filterMode = FilterMode.Trilinear;
			txt.texture.LoadImage(bytes);

			return txt;
		}
		catch (Exception e)
		{
			Debug.LogError(e);
			return null;
		}
		//myObject.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
	}
}

