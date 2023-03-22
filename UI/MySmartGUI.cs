using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using brickgame;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class TableLayout
{
    int[] columnsSizes;

    public TableLayout(params int[] columnsSizes)
    {
        this.columnsSizes = columnsSizes;
    }

    public void AddRow(string text,Action action)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(text , GUILayout.Width(columnsSizes[0]));
        action.Invoke();
        GUILayout.EndHorizontal();
    }
    public void AddRow(string text, string value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(text, GUILayout.Width(columnsSizes[0]));
        GUILayout.Label(value);
        GUILayout.EndHorizontal();
    }
    public void AddSeparator()
    {
        MyGUILayout.DrawUILine(Color.black, 1, 4);
    }

    public void AddLabel(string text)
    {
    }
    public void AddCol()
    {
    }
    public void NewRow()
    {
    }
}

public class TextField<T>
{
    T value;
    T min;
    T max;
    T step;
    bool hasRange = false;
    GUILayoutOption[] options;

    public TextField(T step, params GUILayoutOption[] options)
    {
        this.step = step;
        this.options = options;
    }
    public TextField(T step, T min, T max, params GUILayoutOption[] options)
    {
        this.step = step;
        this.options = options;
        this.min = min;
        this.max = max;
        hasRange = true;
    }
    float toFloat(T val)
	{
        return (float)Convert.ChangeType(val, typeof(float));
    }
    int toInt(T val)
    {
        return (int)Convert.ChangeType(val, typeof(int));
    }
    void Assign(float newValue)
    {
        if (hasRange)
        {
            if (newValue >= toFloat(min) && newValue <= toFloat(max))
                this.value = (T)Convert.ChangeType(newValue, typeof(T));
        }
        else
            this.value = (T)Convert.ChangeType(newValue, typeof(T));
    }
    void Assign(int newValue)
    {
        if (hasRange)
        {
            if (newValue >= toInt(min) && newValue <= toInt(max))
                this.value = (T)Convert.ChangeType(newValue, typeof(T));
        }
        else
            this.value = (T)Convert.ChangeType(newValue, typeof(T));
    }
    public T Draw(T value)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<", GUILayout.Width(30)))
        {
            if (typeof(T) == typeof(float)) Assign(toFloat(value) - toFloat(step));
            if (typeof(T) == typeof(int)) Assign(toInt(value) - toInt(step));

        }
        else
        {
            string textValue = GUILayout.TextField(value.ToString(), options);
            if (typeof(T) == typeof(float))
            {
                float newValue = 0;
                if (textValue.Split(new char[] { '.', ',' }, System.StringSplitOptions.None).Length > 2) // if there are two dots, remove the second one and every fellow digit
                    textValue.Remove(textValue.LastIndexOf('.'));
                if (float.TryParse(textValue, out newValue) && !textValue.EndsWith("."))
                {
                    Assign(newValue);
                }
            }
            if (typeof(T) == typeof(int))
            {
                int newValue = 0;
                if (int.TryParse(textValue, out newValue))
                {
                    Assign(newValue);
                }
            }
            if (GUILayout.Button(">", GUILayout.Width(30)))
            {
                if (typeof(T) == typeof(float)) Assign(toFloat(value) + toFloat(step));
                if (typeof(T) == typeof(int)) Assign(toInt(value) + toInt(step));

            }
        }
        GUILayout.EndHorizontal();
        return this.value;
    }
}


public class DualStateButton
{
    string text_o;
    string text_c;
    bool openSelected = true;
  //  public bool open = false;
    GUILayoutOption[] options;

    public DualStateButton(string text_o, string text_c, bool openSelected, params GUILayoutOption[] options)
    {
        this.text_o = text_o;
        this.text_c = text_c;
        this.options = options;
        this.openSelected = openSelected;
    }

    public bool Draw(bool open)
    {
        if (GUILayout.Button((open) ? text_o : text_c, (openSelected && open) ? "button" : "button", options))
        {
            open = !open;
        }
        return open;
    }
}

public class ToggleButton
{
    string text;
    public bool open = false;
    public bool allowNotSelected;
    GUILayoutOption[] options;
    Rect controlRect;
    public ToggleButton(string text, bool allowNotSelected, params GUILayoutOption[] options)
    {
        this.text = text;
        this.options = options;
        this.allowNotSelected = allowNotSelected;
    }

    public bool Draw()
    {
        //  open = GUILayout.Toggle(open, text, "switch",options);
        open = GUILayout.Toggle(open, text,"button_text", options);
        if (open)
        {
            if (Event.current.type == EventType.Repaint)
                controlRect = GUILayoutUtility.GetLastRect();

            MyGUILayout.DrawImage(controlRect, TextAlignment.Left, TextureIcons.Ok);
       }
        return open;
    }
}
public class RadioButtonGroup
{
    
    public RadioButton selected;
    public List<RadioButton> buttons = new List<RadioButton>();
    public void Draw()
    {
        foreach (var b in buttons)
            b.Draw();
    }
}
public class RadioButton
{
    public object Tag;
    string text;
    RadioButtonGroup group;
    GUILayoutOption[] options;
    Rect controlRect;
    public RadioButton(string text, RadioButtonGroup group, params GUILayoutOption[] options)
    {
        this.text = text;
        this.group = group;
        this.options = options;
       group.buttons.Add(this);
    }

    public void Draw()
    {
        bool open = group.selected == this;
        var newOpen =  GUILayout.Toggle(group.selected == this, text,"button",options);
        if (newOpen != open)
        {
            if (newOpen)
                this.group.selected = this;
            else
                this.group.selected = null;
        }
        if (open)
        {
            if (Event.current.type == EventType.Repaint)
                controlRect = GUILayoutUtility.GetLastRect();

            MyGUILayout.DrawImage(controlRect, TextAlignment.Left, TextureIcons.Ok, new Vector2(0.5f, 0.5f));
        }
    }

}

public class ComboBox
{
    public string style= "button_text";
    public Color backColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public object Tag;
    string selected;
    string new_selected=null;
    string[] items;
    GUILayoutOption[] options;
    int guid;
    static int _guid=0;
    public GUI.WindowFunction draw;
    public bool open = false;
    public Rect controlRect;
    public Rect windowRect = new Rect(20, 20, 120, 200);
    bool drawToken = false;
    public bool openDown = true;
    Camera camera;

    public ComboBox( Type enumType, params GUILayoutOption[] options) : this ( (string[])Enum.GetNames(enumType), options)
    {
    }

    public ComboBox( string[] items ,  params GUILayoutOption[] options)
    {
        guid = _guid++;
        this.camera = Camera.main;
        this.items = items;
        this.options = options;

   
    }

    public T Draw<T>(T selected) where T : Enum
    {
         return (T)Enum.Parse(selected.GetType(),Draw(selected.ToString()));
    }
    public int Draw(int  selectedIndex)
    {
        if (selectedIndex >= 0 && selectedIndex < items.Length)
        {
            var newVal = Draw(items[selectedIndex]);
            return items.ToList().IndexOf(newVal);
        }
        return selectedIndex;
    }

    public string Draw(string old_selected)
    {
        selected = old_selected ;

        //  if (GUILayout.Button(selected, "button_text", options))
        open = GUILayout.Toggle(open, selected , style, options);

        MyGUILayout.DrawImageLastControl( TextAlignment.Right, !open ? TextureIcons.FolderPlus : TextureIcons.FolderMinus);

        if (Event.current.type == EventType.Repaint)
        {
            controlRect = GUIUtility.GUIToScreenRect(GUILayoutUtility.GetLastRect());
            // scalo
            float W, H;
            if (camera != null)
            {
                //controlRect.x = controlRect.x * camera.lensShift.x;
                //controlRect.y = controlRect.y * camera.lensShift.y;

                // sopra
                W = camera.lensShift.x * camera.pixelWidth;
                H = camera.lensShift.y * camera.pixelHeight;
            }
            else
            {
                W = Screen.width;
                H = Screen.height;
            }

            if (openDown)
            {
                windowRect = new Rect(controlRect.x, controlRect.y + controlRect.height, controlRect.width + 10, controlRect.height * items.Count() + 10);
            }
            else
                windowRect = new Rect(controlRect.x, controlRect.y - controlRect.height * items.Count(), controlRect.width + 10, controlRect.height * items.Count() + 10);

            if (windowRect.yMax > H)
				windowRect.position -= new Vector2(0, windowRect.yMax - H + 10);
			if (windowRect.xMax > W)
				windowRect.position -= new Vector2(windowRect.xMax - W + 10, 0);
		}
        
        if (new_selected != null)
        {
            selected = new_selected;
            new_selected = null;
        }
        drawToken = true;
        return selected;

    }

    public void DrawPopup()
    {
        if (!drawToken) return;
        drawToken = false;
        // in  screen corrdinate, le devo scalare
        // var rect = MyGUILayout.GetAlignedRect(controlRect, TextAlignment.Right, !open ? TextureIcons.FolderPlus : TextureIcons.FolderMinus);
     
        if (open)
        {

            MyGUILayout.DrawFillRect(windowRect, backColor);
            GUILayout.BeginArea(windowRect);
            //   draw(0);
            var values = items;

            GUILayout.BeginVertical();
            //int selGridInt = GUILayout.SelectionGrid(0, values.ToArray(), 1, GUILayout.Width(100));
            foreach (var s in values)
            {
                if (GUILayout.Button(s, style))
                {
                    Debug.Log("click " + s);
                    Event.current.Use();
                    open = false;
                    this.new_selected = s;
                }
            }

            //GUI.Box(windowRect, "pupup");

            GUILayout.EndVertical();

            GUILayout.EndArea();
        }
    }
}

public class PupupWindow
{
    public string buttonTxt;
    public GUI.WindowFunction draw;
    public bool open = false;
    public Rect windowRect = new Rect(20, 20, 120, 200);
    GUILayoutOption[] options=null;
    string style = "button";
    Camera camera;
    int w = 200;
    int h = 200;
    public bool modal = false;
    List<PupupWindow> childs = new List<PupupWindow>();

    public bool isOpen => open;

    public PupupWindow(string buttonTxt,int width,int height, GUI.WindowFunction draw, string style = null, params GUILayoutOption[] options)
    {

        this.buttonTxt = buttonTxt;
        this.camera = Camera.main;
        this.open = false;
        this.draw = draw;
        this.options = options;
        if (style != null)
            this.style = style;
        w = width;
        h = height;
    }

    public void RegisterModalPopup(PupupWindow pupup)
    {
        childs.Add(pupup);
    }
    public virtual void OnOpen()
    {
    }

    public virtual void OnDraw()
    {
    }

    protected void DrawHeader(string title)
    {   // header
        GUILayout.BeginHorizontal();
        GUILayout.Label(title, "box", GUILayout.ExpandWidth(true));
        //  GUILayout.Label("", GUILayout.ExpandWidth(true));
        if (MyGUILayout.ButtonCancel())
            Close();
        GUILayout.EndHorizontal();
    }
  
    public bool Draw()
    {
        if (buttonTxt!=null)
        {
            if (GUILayout.Button(buttonTxt, style,  options))
            {
                open = !open;
            }
        }

        if (Event.current.type == EventType.Repaint)
        {
            if (camera != null)
            {
                float sx = (float)((float)camera.pixelWidth * camera.lensShift.x) / 2;
                float sy = (float)((float)camera.pixelHeight * camera.lensShift.y) / 2;
                windowRect = new Rect(sx - w / 2, sy - h / 2, w, h);
            }
            else
            {
                float sx = Screen.height / 2;
                float sy = Screen.width / 2;
                windowRect = new Rect(sx - w / 2, sy - h / 2, w, h);
            }
        }

        //foreach (var child in childs) // stacco gli eventi se ho un figlio aperto
        //{
        //    child.Draw();
        //}

        return open;
    }
    public void Open()    {  open = true; OnOpen();   }

    public void Close() { open = false; }
    public void Toggle() { open = !open; if (open) OnOpen(); }

    public virtual void DrawPopup(Rect? screenRect=null)
    {
        if (open)
        {
            if (screenRect.HasValue)
            {
                if (camera != null)
                {
                    var W = camera.lensShift.x * camera.pixelWidth;
                    var H = camera.lensShift.y * camera.pixelHeight;
                    windowRect = screenRect.Value;
                    //if (windowRect.yMax > H)
                    //    windowRect.position+= new Vector2(0,Mathf.Max(0, windowRect.yMax- H - 10));
                    //if (windowRect.xMax > W)
                    //    windowRect.position -= new Vector2(windowRect.xMax - W + 10, 0);
                }
            }
       
            GUILayout.BeginArea(windowRect, "popup");

            var oldType = Event.current.type;
            if (modal && !( Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout))
                Event.current.type = EventType.ContextClick;

            if (draw!=null)
                draw(0);

            var old = Event.current.type;
            foreach (var child in childs) // stacco gli eventi se ho un figlio aperto
            {
                if (child.isOpen && (Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint))
                    Event.current.type = EventType.ContextClick;
            }

            OnDraw();

            foreach (var child in childs)
            {
                // child.Draw();

                if (child.isOpen && (Event.current.type == EventType.ContextClick))
                    Event.current.type = old;
            }

            if (modal && Event.current.type == EventType.ContextClick)
                Event.current.type = oldType;


        

            GUILayout.EndArea();

            if (modal)
            {
                MyGUILayout.DrawFillRect(windowRect, new Color(0.1f, 0.1f, 0.1f, 0.5f));
            }

            foreach (var child in childs)
            {
                child.DrawPopup();
            }
        }
    }
}


public class TextInputWindow : PupupWindow
{
    string fixedButtonTxt;
    public  string win_title;
    public string win_label;
    string value = "";
    string selectedValue = "";
   
    // params
    public bool showButton;

    public TextInputWindow(bool showButton, string win_title, string win_label, params GUILayoutOption[] options) : this(showButton,null, win_title, win_label,null, options)
    {
    }

    public TextInputWindow(bool showButton,string fixedButtonTxt, string win_title, string win_label, string style=null,params GUILayoutOption[] options) : base(fixedButtonTxt, 300, 200, null,style, options)
    {
        this.showButton = showButton;
        this.win_label = win_label;
        this.win_title = win_title;
        this.fixedButtonTxt = fixedButtonTxt;
        draw = (windowID) =>
        {
            GUILayout.BeginVertical("popup");
            // header
            DrawHeader(this.win_title);

            //

            GUILayout.BeginHorizontal();
            GUILayout.Label(this.win_label);
            selectedValue = GUILayout.TextField(selectedValue);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            if (MyGUILayout.ButtonCancel())
            {
                Close();
            }
            if (MyGUILayout.ButtonOK())
            {
                this.value = selectedValue.Trim();
               
            }
            GUILayout.EndHorizontal();


            GUILayout.EndVertical();
        };

    }
    public string Draw(string actualValue)
    {
        if (showButton)
        {
            if (fixedButtonTxt != null)
                buttonTxt = fixedButtonTxt;
            else
                buttonTxt = actualValue;
        }
        else
            buttonTxt = null;

        // prima volta
        if (open && selectedValue == "")
        {
            value = "";
            selectedValue = actualValue;
        }
        if (!open)
            this.value = "";

        //  this.value = value;
        var retValue = this.value;

        if (this.value != "")
        {
            // clear
            value = "";
            selectedValue = "";
            Close();
        }
        base.Draw();

        return retValue;
    }
}

public enum DialogResponse
{
    Null,
    OK,
    Cancel,
}

public class DialogBox : PupupWindow
{
    bool showButton;
    public string win_title;
    public string text;
    int returnValue = 0;
    string fixedButtonTxt;
    GUILayoutOption[] options;

    public static DialogBox Confirm(string win_title, string text)
    {
        return new DialogBox(false, null, win_title, text);
    }

    public DialogBox(bool showButton,string fixedButtonTxt,string win_title, string text, params GUILayoutOption[] options) : base(fixedButtonTxt,300,200,null,null, options)
    {
        this.fixedButtonTxt = fixedButtonTxt;
        this.showButton = showButton;
        this.win_title = win_title;
        this.text = text;

        draw = (windowID) =>
        {
            GUILayout.BeginVertical("popup");
            // header
            // header
            DrawHeader(this.win_title);

            //

            GUILayout.BeginHorizontal();
            GUILayout.Label(this.text, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.ExpandWidth(true));
       
            if (MyGUILayout.ButtonCancel())
            {
                returnValue = -1;
            }
            if (MyGUILayout.ButtonOK())
             {
                returnValue = 1;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        };
    }

    public new DialogResponse Draw()
    {
        if (showButton)
        {
            if (fixedButtonTxt != null)
                buttonTxt = fixedButtonTxt;
        }
        else
            buttonTxt = null;

        // prima volta
        if (!open)
            this.returnValue = 0;

        //  this.value = value;
        var retValue = this.returnValue;

        if (this.returnValue != 0)
        {
            // clear
            returnValue = 0;
            Close();
        }
        base.Draw();

        return retValue == 1 ? DialogResponse.OK : ((retValue == 0) ?  DialogResponse.Null : DialogResponse.Cancel) ;
    }
}
