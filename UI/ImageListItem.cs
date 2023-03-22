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


public class ImageListItem : MonoBehaviour, IPointerClickHandler
{
	public string path;

	bool _isSelected;

	RawImage image;

	public bool isSelected
	{
		get { return _isSelected; }
		set
		{
			if (_isSelected != value)
			{
				_isSelected = value;

			}
		}
	}

	void Start()
	{
		image = GetComponent<RawImage>();
	}
	private void Update()
	{
		if (_isSelected)
		{
			image.color = Color.Lerp(Color.white, Color.yellow, Mathf.PingPong(Time.time, 1));
		}
		else
			image.color = Color.white;
	}

	public void OnPointerClick(PointerEventData eventData) // 3
	{
	//print("I was clicked" + path);

		GetComponentInParent<ImageList>().OnPointerClick(this);
	}

	public void OnDrag(PointerEventData eventData)
	{
		print("I'm being dragged!");

	}

	public void OnPointerEnter(PointerEventData eventData)
	{

	}

	public void OnPointerExit(PointerEventData eventData)
	{

	}


}

