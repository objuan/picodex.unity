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

[Serializable]
public class ListViewItem
{
	public int count;
	public string[] values;
	public Func<object>[] funs;
	public GameObject[] goList;
	public object Tag;

	public Action<ListViewItem> onUpdate;
	public event Action<ListViewItem> onClick;

	public ListViewItem()
	{
	}

	public ListViewItem(int count)
	{
		values = new string[count];
		goList = new GameObject[count];
		funs = new Func<object>[count];
	}

	public void FireClick()
	{
		if (onClick != null)
			onClick(this);
	}

	public bool GetBool(int column)
	{
		if (funs[column] != null)
			return (bool)funs[column].Invoke();
		else
			return values[column] == "1";
	}
	public string GetString(int column)
	{
		if (funs[column] != null)
			return (string)funs[column].Invoke();
		else
			return values[column] ;
	}
	public int GetInt(int column)
	{
		if (funs[column] != null)
			return (int)funs[column].Invoke();
		else
			return Convert.ToInt32(values[column]);
	}

	public void SetValue(int column, bool value)
	{
		values[column] = value ? ""+1 : ""+0;
	}

	public void SetValue(int column, string value)
	{
		values[column] = value;
	}

	public void SetValue(int column, int value)
	{
		values[column] = ""+value;
	}

	public void SetValue(int column, Func<object> fun)
	{
		funs[column] = fun;
	}

}

