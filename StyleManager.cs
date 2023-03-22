using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//namespace Assets.Scripts.Utils
[ExecuteInEditMode]
public  class StyleManager : MonoBehaviour
{
	public Material fakePreviewMaterial;

	public Texture txt_cross;
	public Texture txt_folder;
	public Texture txt_folder_minus;
	public Texture txt_folder_plus;
	public Texture txt_edit;
	public Texture txt_save;
	public Texture txt_ok;
	public Texture txt_arrow_dual;
	public Texture txt_plus;
	public Texture txt_minus;
	public Texture txt_settings;
	public Texture txt_play;
	public Texture txt_stop;
	public Texture txt_undo;
	public Texture txt_eye;
	public Texture txt_eye_slash;
	public Texture txt_menu;

	public Texture2D txt_context_circle;

	static GUIStyle _horizontalScrollbar = null;
	static GUIStyle _label = null;
	static GUIStyle _label_selected = null;
	static GUIStyle _label_centered = null;
	static GUIStyle _trasparent_panel = null;
	static GUIStyle _panel = null;
	static GUIStyle _gui_panel = null;
	static GUIStyle _sub_panel = null;
	static GUIStyle _bold = null;
	static GUIStyle _html = null;
	static GUIStyle _button_selected = null;
	static GUIStyle _button_normal = null;
	static GUIStyle _button_noborder = null;
	static GUIStyle _toggle_normal = null;
	static GUIStyle _toggle_selected = null;
	static GUIStyle _line= null;

	static int fontSize = 10;
	static Color fontColor = Color.white;
	static Color selectionColor = new Color(0, 0.5f, 0, 1);

	private void OnEnable()
	{
		_horizontalScrollbar = null;
		   _label = null;
		 _label_selected = null;
		_label_centered = null;
		 _panel = null;
		 _gui_panel = null;
		 _bold = null;
		 _html = null;
		 _button_selected = null;
		_button_normal = null;
		_toggle_normal= null;
		_toggle_selected = null;
		_trasparent_panel = null;
		_line = null;
	}
	public static GUIStyle horizontalScrollbar
	{
		get
		{
			if (_horizontalScrollbar == null)
			{
				_horizontalScrollbar = new GUIStyle("horizontalScrollbar");
			}
			return _horizontalScrollbar;
		}
	}


	public static GUIStyle html
	{
		get
		{
			if (_html == null)
			{
				_html = new GUIStyle("label");
				_html.richText = true;
			}
			return _html;
		}
	}

	public static GUIStyle label
	{
		get
		{
			if (_label == null)
			{
				_label = new GUIStyle("label");
				_label.normal.textColor = fontColor;
				//_label.fontSize = fontSize;
			}
			return _label;
		}
	}
	public static GUIStyle label_centered
	{
		get
		{
			if (_label_centered == null)
			{
				_label_centered = new GUIStyle("label");
				_label_centered.normal.textColor = fontColor;
				//_label_centered.fontSize = fontSize;
				_label_centered.alignment = TextAnchor.MiddleCenter;
			}
			return _label_centered;
		}

	}
	public static GUIStyle label_selected
	{
		get
		{
			if (_label_selected == null)
			{
				_label_selected = new GUIStyle("label");
				_label_selected.normal.textColor = fontColor;
				//_label_selected.fontSize = fontSize;

				_label_selected.normal.background = Texture2DEx.MakeTex(10, 10, selectionColor);
			}
			return _label_selected;
		}
	}
	public static GUIStyle bold
	{
		get
		{
			if (_bold == null)
			{
				_bold = new GUIStyle("label");
				_bold.fontStyle = FontStyle.Bold;
				_bold.normal.textColor = fontColor;
				_bold.fontSize = fontSize;
			}
			return _bold;
		}
	}
	public static GUIStyle panel
	{
		get
		{
			if (_panel == null)
			{
				_panel = new GUIStyle("box");
			}
			return _panel;
		}
	}

	public static GUIStyle sub_panel
	{
		get
		{
			if (_sub_panel == null)
			{
				_sub_panel = new GUIStyle();
		//		_sub_panel.fontStyle = FontStyle.Bold;
				_sub_panel.normal.textColor = Color.white;
				_sub_panel.normal.background = Texture2DEx.MakeTex(10, 10, new Color(.3f, .3f, .3f, 1f));
				//_bold.fontSize = fontSize;
			}
			return _sub_panel;
		}
	}
	public static GUIStyle trasparent_panel
	{
		get
		{
			if (_trasparent_panel == null)
			{
				_trasparent_panel = new GUIStyle();
				_trasparent_panel.normal.textColor = Color.white;
				_trasparent_panel.normal.background = Texture2DEx.MakeTex(10, 10, new Color(0,0,0,0));
				//_bold.fontSize = fontSize;
			}
			return _trasparent_panel;
		}
	}
	//private static Texture2D MakeTex(int width, int height, Color col)
	//{
	//	Color[] pix = new Color[width * height];

	//	for (int i = 0; i < pix.Length; i++)
	//		pix[i] = col;

	//	Texture2D result = new Texture2D(width, height);
	//	result.SetPixels(pix);
	//	result.Apply();

	//	return result;

	//}
	public static GUIStyle gui_panel
	{
		get
		{
			if (_gui_panel == null)
			{
				_gui_panel = new GUIStyle();
				_gui_panel.fontStyle = FontStyle.Bold;
				_gui_panel.normal.textColor = Color.white;
				_gui_panel.normal.background = Texture2DEx.MakeTex(10, 10, new Color(.1f, .1f, .1f, 0.6f));
				//_bold.fontSize = fontSize;
			}
			return _gui_panel;
		}
	}

	public static GUIStyle button_noborder
	{
		get
		{
			if (_button_noborder == null)
			{
				_button_noborder = new GUIStyle("button");
				_button_noborder.border = new RectOffset(0, 0, 0, 0);
			}
			return _button_noborder;
		}
	}
	public static GUIStyle button_normal
	{
		get
		{
			if (_button_normal == null)
			{
				_button_normal = new GUIStyle("button");
				////	_button_normal.fontStyle = FontStyle.Bold;
				//	_button_normal.normal.textColor = Color.white;
				//	_button_normal.normal.background = Texture2DEx.MakeTex(10, 10, new Color(0,0,0,0));
				//_bold.fontSize = fontSize;
			}
			return _button_normal;
		}
	}
	public static GUIStyle button_selected
	{
		get
		{
			if (_button_selected == null)
			{
				_button_selected = new GUIStyle("button");
				_button_selected.fontStyle = FontStyle.Bold;
	
				_button_selected.normal.textColor = Color.white;
				_button_selected.normal.background = Texture2DEx.MakeTex(10, 10, selectionColor);

				_button_selected.normal.textColor = Color.white;
				_button_selected.onHover.background = _button_selected.normal.background;

			}
			return _button_selected;
		}
	}
	public static GUIStyle toggle_normal
	{
		get
		{
			if (_toggle_normal == null)
			{
				_toggle_normal = new GUIStyle("toggle");
				//	_button_normal.fontStyle = FontStyle.Bold;
				//_toggle_selected.normal.textColor = Color.cyan;
				//_toggle_selected.normal.background = Texture2DEx.MakeTex(10, 10, new Color(0, 0, 0, 0));
				//_bold.fontSize = fontSize;
			}
			return _toggle_normal;
		}
	}

	public static GUIStyle toggle_selected
	{
		get
		{
			if (_toggle_selected == null)
			{
				_toggle_selected = new GUIStyle("toggle");
				_toggle_selected.fontStyle = FontStyle.Bold;
				//_toggle_selected.active.textColor = Color.cyan;
				//_toggle_selected.normal.background = Texture2DEx.MakeTex(10, 10, new Color(0, 0, 0, 0));
				//_bold.fontSize = fontSize;
			}
			return _toggle_selected;
		}
	}

	public static GUIStyle line
	{
		get
		{
			if (_line == null)
			{
				_line.normal.textColor = Color.white;
				_line.normal.background = Texture2DEx.MakeTex(10, 10, new Color(0,0, 0, 1));
			}
			return _line;
		}
	}
}

