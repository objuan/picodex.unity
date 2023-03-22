using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ImageList : MonoBehaviour
{
	[NonSerialized]
	Func<string[]> imagePathFun;
	[NonSerialized]
	List<DynamicTexture> list = new List<DynamicTexture>();

	[NonSerialized]
	public int cols = 2;

	[NonSerialized]
	public int rows = 0;

	ImageListItem _selected=null;

	public event EventHandler SelectedChanged;


	public ImageListItem selected
	{
		get { return _selected; }
		set
		{
			if (_selected != value)
			{
				if (_selected != null) _selected.isSelected = false;

				_selected = value;

				if (_selected!=null)
					_selected.isSelected = true;

				OnSelectedChanged();

				if (SelectedChanged!=null) SelectedChanged(this,EventArgs.Empty);
			}
		}
	}

	public IEnumerator Setup(int rows,int cols , Func<string[]> imagePathFun)
	{
		this.rows = rows;
		this.cols = cols;
		this.imagePathFun = imagePathFun;

		yield return CoReload();
		//Reload();
	}

	public void Select(string imagePath)
	{
		foreach (var item in GetComponentsInChildren<ImageListItem>())
		{
			if (item.path == imagePath)
				selected = item;
		}
	}

	protected  virtual  void OnSelectedChanged() {
	}


	public void Reload()
	{

		StartCoroutine(CoReload());
	}

	IEnumerator CoReload()
	{
		var images = imagePathFun.Invoke();

		var rect = GetComponent<RectTransform>().ToGuiBound();

		int w = 0;
		if (rows == 0)
		{
			w = (int)(rect.width / cols);
			rows = (int)(images.Length / cols) + 1;
			if ((images.Length % cols) == 0) rows = rows - 1;
		}

		int h = w;
		float x = 0;
		float y = 0;
		int col = 0;

		((RectTransform)transform).sizeDelta = new Vector3(rect.width, h * rows, 1);

		// vwerso il basso
		y = -h;
		foreach (var path in images)
		{
			Debug.Log(path);
			DynamicTexture txt = DynamicTexture.FromPath(path);
			if (txt != null)
			{
				list.Add(txt);

				GameObject go = new GameObject();
				go.SetParentAtOrigin(gameObject);
				go.AddComponent<RawImage>().texture = txt.texture;
				go.AddComponent<ImageListItem>().path = path;
				((RectTransform)go.transform).SetAnchor(AnchorPresets.BottomLeft);
				((RectTransform)go.transform).SetPivot(PivotPresets.BottomLeft);
				((RectTransform)go.transform).sizeDelta = new Vector3(w, h, 1);
				((RectTransform)go.transform).localPosition = new Vector3(x, y, 0);

				col++; x += w;
				if (col == cols)
				{
					x = 0; col = 0;
					y -= h;
				}
			}

			yield return null;
		}

		((RectTransform)transform).SetAnchor(AnchorPresets.BottomLeft, 0, 0);

	}

	public void OnPointerClick(ImageListItem item) // 3
	{
		selected=item;
	}
}

