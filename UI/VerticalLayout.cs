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


public class LayoutColumn
{
	public int index;
	public string columnName;
	public int width = -1;

	public int render_width = -1;
	public GameObject go;

}
public class LayoutRow
{
	public VerticalLayout layout;
	public int index;
	public GameObject[] items ;

	public void Set(int index,string str)
	{
		items[index] = GameObject.Instantiate(layout.labelUI, layout.transform);
		items[index].GetComponent<Text>().text = str;
	}
	public void Set<T>(int index,T startValue, Action<T> onChanged)
	{
		items[index] = GameObject.Instantiate(layout.drowDownUI, layout.transform);
		Dropdown ddl = items[index].GetComponent<Dropdown>();
		ddl.ClearOptions();
		List<string> opts = new List<string>();
		foreach (var e in Enum.GetValues(typeof(T)))
		{
			opts.Add(e.ToString());
		}
		ddl.AddOptions(opts);
		ddl.value = opts.IndexOf(startValue.ToString());
		ddl.onValueChanged.AddListener((idx) =>
	   {
		   onChanged( (T)(Enum.Parse( typeof(T) , opts[ddl.value])));
	   });
	}

	public void Set(int index, GameObject go)
	{
		items[index] = go;
	}
}

public class VerticalLayout : MonoBehaviour
{
	[Header("UI")]
	public GameObject labelUI;
	public GameObject drowDownUI;

	bool mustRebuild = true;
	bool eventDisabled = true;
	List<LayoutColumn> columns = new List<LayoutColumn>();
	List<LayoutRow> rows= new List<LayoutRow>();

	public int rowH = 30;
	public int rowSpace = 2;

	public void AddColumn(LayoutColumn col)
	{
		columns.Add(col);
	}
	public void AddColumn(string name)
	{
		columns.Add(new LayoutColumn() {  columnName = name });
	}
	public void AddColumn(string name,int width)
	{
		columns.Add(new LayoutColumn() { columnName = name,  width = width  }) ;
	}
	public LayoutRow NewRow()
	{
		mustRebuild = true;
		rows.Add(new LayoutRow() { index = rows.Count, items = new GameObject[columns.Count],  layout=this  });
		return rows.Last();
	}

	public void AddRow(int index, GameObject obj)
	{
	
	}

	private void Update()
	{
		if (mustRebuild)
		{
			mustRebuild = false;
			Rebuild();
		}
	}

	public void Rebuild()
	{
		var bound = GetComponent<RectTransform>();
		eventDisabled = true;
		//transform.Clear();
		// layout
		int w = (int)bound.rect.width;

		//Debug.Log("w =" + w);
		int fixedW = 0;
		int freeCount = 0;
		foreach (var col in columns)
		{
			if (col.width != -1) { fixedW += col.width;  }
			else freeCount++;
		}
		foreach (var col in columns)
		{
			if (col.width == -1) col.render_width = (w - fixedW) / freeCount;
			else col.render_width = col.width;
		}

		int y = 0;
		int x = 0;
		
		// ------------
		x = 0;
		//foreach (var col in columns)
		//{
		//	if (col.type == ListViewColumnType.Toggle)
		//	{
		//		col.go = Instantiate(toggleUI, transform);
		//		col.isRowToggled = true;
		//		SetPos(col.go, new Rect(x, y, col.render_width, rowH));
		//		var toggle = col.go.GetComponent<Toggle>();
		//		toggle.isOn = col.isRowToggled;
		//		toggle.onValueChanged.AddListener((v) =>
		//		{
		//			col.isRowToggled = v;
		//			if (col.onColToggle != null) col.onColToggle.Invoke(v);
		//		});
		//	}
		//	x += col.render_width;
		//}


		////----------


		foreach (var row in rows)
		{
			x = 0;
			int c = 0;
			foreach (var col in columns)
			{
				row.items[c].GetComponent<RectTransform>().SetAnchor(AnchorPresets.TopLeft);
				row.items[c].GetComponent<RectTransform>().SetPivot(PivotPresets.TopLeft);
				SetPos(row.items[c], new Rect(x, y, col.render_width, rowH));
				x += col.render_width;
				c++;
			}

			y -= (rowH + rowSpace);
		}

		//UpdateValues();
		//eventDisabled = false;
	}
	void SetPos(GameObject go, Rect rect)
	{
		//	Debug.Log("set " + rect);
		go.GetComponent<RectTransform>().localPosition = new Vector2(rect.x, rect.y);
		go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
		go.GetComponent<RectTransform>().sizeDelta = new Vector2(rect.width, rect.height);
	}

	public void UpdateValues()
	{
		//foreach (var item in Items)
		//{
		//	item.onUpdate(item);

		//	for(int i=0;i< columns.Count;i++)
		//	{
		//		var col = columns[i];
		//		if (col.type == ListViewColumnType.Toggle || col.type == ListViewColumnType.Radio || col.type == ListViewColumnType.RadioNullable)
		//		{
		//			var toggle = item.goList[i].GetComponent<Toggle>();
		//			toggle.isOn = item.GetBool(i);
		//		}
		//		else if (col.type == ListViewColumnType.String)
		//		{
		//			var text = item.goList[i].GetComponent<Text>();
		//			text.text = item.GetString(i);
		//		}
		//		else if (col.type == ListViewColumnType.Button)
		//		{
		//			var text = item.goList[i].GetComponentInChildren<Text>();
		//			text.text = item.GetString(i);
		//		}
		//		else if (col.type == ListViewColumnType.Combo)
		//		{
		//			var ddl = item.goList[i].GetComponent<Dropdown>();
		//			ddl.value = item.GetInt(i);
		//		}
		//	}
		//}
	}


}

