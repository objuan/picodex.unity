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


public enum ListViewColumnType
{
	String,
	Toggle,
	RadioNullable,
	Radio,
	Combo,
	Button
}

// http://mateodon.blogspot.com/2015/04/unity-ui-easy-tabs-no-scripting.html
public class ListViewColumn
{
	public int index;
	public string columnName;
	public ListViewColumnType type;
	public Type combo_type;
	public int width=-1;

	public int render_width = -1;
	public GameObject go;
	public ToggleGroup group;

	//clink on row
	public Action<bool> onColToggle;
	// 
	public Action<int, bool> onRowToggle;

	public Action<int, bool> onRowClick;
	public bool isRowToggled=true;
	//
	public Action<int, int> onRowCombo;
}

public class ListView: MonoBehaviour
{

	class RowEventHook
	{
		public ListViewColumn col;
		public ListViewItem item;
		Action<RowEventHook, int> evt;

		public RowEventHook(ListViewColumn col, ListViewItem item, Action<RowEventHook, int> evt) { this.col = col; this.item = item; this.evt = evt; }
		public void Click()
		{
			this.evt.Invoke(this, 1);
		}
		public void Click(bool value)
		{
			this.evt.Invoke(this, value ? 1 : 0);
		}
		public void Click(int value)
		{
			this.evt.Invoke(this, value);
		}
	}


	public List<ListViewColumn> columns = new List<ListViewColumn>();
	public List<ListViewItem> Items = new List<ListViewItem>();
	//public Action<ListViewItem> onUpdate;

	bool mustRebuild = true;

	public bool changable = false;
	public int rowH = 60;
	public int rowSpace = 5;
	public bool showHeader = false;

	public event Action onAdd;
	public event Action onDelete;

	public int selectedIndex;

	[Header("UI")]
	public GameObject buttonUI;
	public GameObject labelUI;
	public GameObject toggleUI;
	public GameObject radioUI;
	public GameObject dropDownUI;

	bool eventDisabled;

	private void Start()
    {
      
    }

    public ListViewColumn Column(int index){
			return columns[index];
	}

	public ListViewColumn Column(string name)
	{
		return columns.FirstOrDefault(X => X.columnName == name);
	}

	public void Initialize(string[] columnNames, ListViewColumnType[] types)
    {
		for (int i = 0; i < columnNames.Length; i++)
		{
			columns.Add(new ListViewColumn() { columnName = columnNames[i], type = types[i], index=i });
		}
	}

    public void Clear()
	{
		mustRebuild = true;
		Items.Clear();
	}

	public ListViewItem AddItem(Action<ListViewItem> onUpdate)
	{
		mustRebuild = true;
		ListViewItem item = new ListViewItem(columns.Count);
		item.onUpdate = onUpdate;
		Items.Add(item);
		return item;
	}

    private void Update()
    {
		if (mustRebuild)
		{
			mustRebuild = false;
			Rebuild();
		}
    }

	void SetPos(GameObject go, Rect rect)
	{
	//	Debug.Log("set " + rect);
		go.GetComponent<RectTransform>().localPosition = new Vector2(rect.x, rect.y);
		go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
		go.GetComponent<RectTransform>().sizeDelta = new Vector2(rect.width, rect.height);
	}

	public void Rebuild()
	{
		var bound = GetComponent<RectTransform>();
		eventDisabled = true;
		transform.Clear();
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
		if (showHeader)
		{
			foreach (var col in columns)
			{
				col.go = Instantiate(labelUI, transform);
				col.go.GetComponent<Text>().text = col.columnName;
			
				SetPos(col.go, new Rect(x, y, col.render_width, rowH));
				x += col.render_width;
			}
			y -= (rowH+ rowSpace);
		}
		// ------------
		x = 0;
		foreach (var col in columns)
		{
			if (col.type == ListViewColumnType.Toggle)
			{
				col.go = Instantiate(toggleUI, transform);
				col.isRowToggled = true;
				SetPos(col.go, new Rect(x, y, col.render_width, rowH));
				var toggle = col.go.GetComponent<Toggle>();
				toggle.isOn = col.isRowToggled;
				toggle.onValueChanged.AddListener((v) =>
				{
					col.isRowToggled = v;
					if (col.onColToggle != null) col.onColToggle.Invoke(v);
				});
			}
			x += col.render_width;
		}

		if (changable)
		{
			// add 
			var add = Instantiate(buttonUI, transform);
			add.GetComponentInChildren<Text>().text = "+";
			SetPos(add, new Rect(w - 140, y, rowH, rowH));
			add.GetComponent<Button>().onClick.AddListener(() =>
		   {
			   if (onAdd != null) onAdd();
			   Rebuild();
		   });

			var del = Instantiate(buttonUI, transform);
			del.GetComponentInChildren<Text>().text = "-";
			SetPos(del, new Rect(w - rowH - 5, y, rowH, rowH));
			del.GetComponent<Button>().onClick.AddListener(() =>
			{
				if (onDelete != null) onDelete();
				Rebuild();
			});

			y -= (rowH + rowSpace);
		}


		//----------
		

		foreach (var item in Items)
        {
            x = 0;
			int c = 0;
            foreach (var col in columns)
			{
				if (col.type == ListViewColumnType.Button)
				{
					item.goList[c] = Instantiate(buttonUI, transform);
					var btn = item.goList[c].GetComponent<Button>();
					var hook = new RowEventHook(col, item, (h, value) =>
					{
						if (!eventDisabled)
						{
							if (h.col.onRowClick != null) h.col.onRowClick(Items.IndexOf(h.item), value == 1);
							item.FireClick();
						}
					});
					btn.onClick.AddListener(hook.Click);
				}
				else if (col.type == ListViewColumnType.Toggle )
                {
					item.goList[c] = Instantiate(toggleUI, transform);
					var toggle = item.goList[c].GetComponent<Toggle>();
					var hook = new RowEventHook(col,item, (h,value) =>
					{
						if (!eventDisabled)
							if (h.col.onRowToggle != null) h.col.onRowToggle(Items.IndexOf(h.item), value==1);
					});
					toggle.onValueChanged.AddListener(hook.Click);
                }
				else if(col.type == ListViewColumnType.RadioNullable)
				{
					item.goList[c] = Instantiate(radioUI, transform);
					var toggle = item.goList[c].GetComponent<Toggle>();
					var hook = new RowEventHook(col, item, (h, value) =>
					{
						if (!eventDisabled)
						{
							if (value==1)
							{
								eventDisabled = true;
								// disabilito gli altri 
								foreach (var it in Items.Where(X => X != h.item))
								{
									it.goList[h.col.index].GetComponent<Toggle>().isOn = false;
								}
								eventDisabled = false;
							}
							else
							{
							}
							if (h.col.onRowToggle != null) h.col.onRowToggle(Items.IndexOf(h.item), value == 1);
						}
					});
					toggle.onValueChanged.AddListener(hook.Click);
				}
				else if(col.type == ListViewColumnType.Radio )
				{
					item.goList[c] = Instantiate(radioUI, transform);
					var toggle = item.goList[c].GetComponent<Toggle>();
					
					if (col.group == null)
                        col.group = gameObject.AddComponent<ToggleGroup>();
					toggle.group = col.group;

					var hook = new RowEventHook(col, item, (h, value) =>
					{
						if (!eventDisabled && value==1)
							if (h.col.onRowToggle != null) h.col.onRowToggle(Items.IndexOf(h.item), value==1);
					});
					toggle.onValueChanged.AddListener(hook.Click);

				}
				else if (col.type == ListViewColumnType.String)
                {
					item.goList[c] = Instantiate(labelUI, transform);
                }
                else if (col.type == ListViewColumnType.Combo)
                {
					item.goList[c] = Instantiate(dropDownUI, transform);
					var ddl = item.goList[c].GetComponent<Dropdown>();
					ddl.options.Clear();
					foreach (var o in Enum.GetValues(col.combo_type))
					{
						ddl.options.Add(new Dropdown.OptionData(o.ToString()));
					}
					var hook = new RowEventHook(col, item, (h, value) =>
					{
						if (!eventDisabled && col.onRowCombo != null) col.onRowCombo(Items.IndexOf(h.item), value);
					});

					ddl.onValueChanged.AddListener(hook.Click);
				}

                SetPos(item.goList[c], new Rect(x, y, col.render_width, rowH));
                x += col.render_width;
				c++;
            }
			
			y -= (rowH + rowSpace);
        }

		UpdateValues();
		eventDisabled = false;
	}

	public void UpdateValues()
	{
		foreach (var item in Items)
		{
			item.onUpdate(item);

			for(int i=0;i< columns.Count;i++)
			{
				var col = columns[i];
				if (col.type == ListViewColumnType.Toggle || col.type == ListViewColumnType.Radio || col.type == ListViewColumnType.RadioNullable)
				{
					var toggle = item.goList[i].GetComponent<Toggle>();
					toggle.isOn = item.GetBool(i);
				}
				else if (col.type == ListViewColumnType.String)
				{
					var text = item.goList[i].GetComponent<Text>();
					text.text = item.GetString(i);
				}
				else if (col.type == ListViewColumnType.Button)
				{
					var text = item.goList[i].GetComponentInChildren<Text>();
					text.text = item.GetString(i);
				}
				else if (col.type == ListViewColumnType.Combo)
				{
					var ddl = item.goList[i].GetComponent<Dropdown>();
					ddl.value = item.GetInt(i);
				}
			}
		}
	}


}

