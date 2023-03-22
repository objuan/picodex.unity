using UnityEngine;
using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using brickgame;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class UI_ID
{
    public string guid = Guid.NewGuid().ToString();
    public Rect rect;

    public UI_ID(string guid)
    {
        this.guid = guid;
    }
}

public enum TextureIcons
{
    None=0,
    Folder,
    FolderPlus,
    FolderMinus,
    Cross,
    Ok,
    Edit,
    Save,
    ArrowDual,
    Plus,
    Minus,
    Settings,
    Play,
    Stop,
    Undo,
    Eye,
    EyeSlash,
    Menu
}

public static partial class MyGUILayout
{


    class PopupState
    {
        public GUI.WindowFunction draw;
        public bool open = false;
        public Rect windowRect = new Rect(20, 20, 120, 200);
    }
    class Layout
    {
        public int childs = 0;
        //public string path="";
        //public int progressiveCount=0;
        //public int childCount=0;
        // public Dictionary<int, PopupState> popupList = new Dictionary<int, PopupState>();
        //public List< int> popupIdxList = new List<int> ();
    }

    public class FlashingLabel
    {
        public int count=0;
        public float time;
        public int phase = 0;
        public void Reset()
        {
            count = 0;
        }
        public bool Draw(string text, Color colorA,Color colorB,float period)
        {
            var old = GUI.color;

            if ((Time.time - time) > period)
            {
                time = Time.time;
                phase++; if (phase == 2) phase = 0;
                count++;
            }

            if (phase == 0)
            {
                var t = (Time.time - time) / period;
                GUI.color = Color.Lerp(colorA, colorB, t);
            }
            if (phase == 1)
            {
                var t = (Time.time - time) / period;
                GUI.color = Color.Lerp(colorB, colorA, t);
            }
            GUILayout.Label(text);
            GUI.color = old;

            return count <=1;
        }
    }

    //  static int progressiveCount = 0;
    static List<Layout> layouts = new List<Layout>();
    static List<PopupState> windows = new List<PopupState>();

    static Dictionary<string, PopupState> popupList = new Dictionary<string, PopupState>();
    static Dictionary<string, FlashingLabel> flashingMap = new Dictionary<string, FlashingLabel>();

    public static void BeginLayout()
    {
        layouts.Clear();

        //if (Event.current.type == EventType.Layout)
        //{
        //    Event.current.Use();  //Eat the event so it doesn't propagate through the editor.
        //}

        windows.Clear();

    }
    public static void EndLayout()
    {
        int idx = 0;
        foreach (var pupup in windows)
        {
            Debug.Log(idx);
            pupup.windowRect = GUILayout.Window(idx++, pupup.windowRect, pupup.draw, "");
        }
    }


    public static void BeginVertical()
    {
        //   Debug.Log("begin");
        var layout = new Layout() { };
        layouts.Add(layout);

        GUILayout.BeginVertical();
    }

    public static void EndVertical()
    {
        // Debug.Log("and");
        layouts.RemoveAt(layouts.Count - 1);
        GUILayout.EndVertical();
    }
    public static void BeginHorizontal()
    {
        var layout = new Layout();
        layouts.Add(layout);

        GUILayout.BeginHorizontal();
    }

    public static void EndHorizontal()
    {
        layouts.RemoveAt(layouts.Count - 1);
        GUILayout.EndHorizontal();
    }

    //public static void BeginChangeCheck()
    //{
    //    EditorGUI.BeginChangeCheck();
    //}
    //public static bool EndChangeCheck()
    //{
    //    return EditorGUI.EndChangeCheck();
    //}
    //public static void SetDirty(GameObject go)
    //{
    //    UnityEditor.EditorUtility.SetDirty(go);
    //}


    // =======================


    public static int Toolbar(int selected, string[] texts, params GUILayoutOption[] options)
    {
        return GUILayout.Toolbar(selected, texts, "toolbar", options);
    }

    public static void Space(int height = 1)
    {
        GUILayout.Label(" ", GUILayout.Height(height));
    }

    public static Texture GetIconTexture(TextureIcons icon)
    {
        Texture txt = null;
        if (icon == TextureIcons.Cross) txt = GO.Instance<StyleManager>().txt_cross;
        else if (icon == TextureIcons.Ok) txt = GO.Instance<StyleManager>().txt_ok;
        else if (icon == TextureIcons.Edit) txt = GO.Instance<StyleManager>().txt_edit;
        else if (icon == TextureIcons.Save) txt = GO.Instance<StyleManager>().txt_save;
        else if (icon == TextureIcons.Folder) txt = GO.Instance<StyleManager>().txt_folder;
        else if (icon == TextureIcons.FolderMinus) txt = GO.Instance<StyleManager>().txt_folder_minus;
        else if (icon == TextureIcons.FolderPlus) txt = GO.Instance<StyleManager>().txt_folder_plus;
        else if (icon == TextureIcons.ArrowDual) txt = GO.Instance<StyleManager>().txt_arrow_dual;
        else if (icon == TextureIcons.Plus) txt = GO.Instance<StyleManager>().txt_plus;
        else if (icon == TextureIcons.Minus) txt = GO.Instance<StyleManager>().txt_minus;
        else if (icon == TextureIcons.Settings) txt = GO.Instance<StyleManager>().txt_settings;
        else if (icon == TextureIcons.Play) txt = GO.Instance<StyleManager>().txt_play;
        else if (icon == TextureIcons.Stop) txt = GO.Instance<StyleManager>().txt_stop;
        else if (icon == TextureIcons.Undo) txt = GO.Instance<StyleManager>().txt_undo;
        else if (icon == TextureIcons.Eye) txt = GO.Instance<StyleManager>().txt_eye;
        else if (icon == TextureIcons.EyeSlash) txt = GO.Instance<StyleManager>().txt_eye_slash;
        else if (icon == TextureIcons.Menu) txt = GO.Instance<StyleManager>().txt_menu;
        return txt;
    }
    static int icon_padding = 5;

    public static Rect GetAlignedRect(Rect baseRect, TextAlignment align, TextureIcons icon)
    {
        return GetAlignedRect(baseRect, align, icon_padding, GetIconTexture(icon));
    }

    public static Rect GetAlignedRect(Rect baseRect, TextAlignment align, int padding, Texture txt)
    {
        Rect rect = Rect.zero;
        float aspect = txt.width / txt.height;
        int H = (int)baseRect.height - padding * 2;
        int W = (int)(H * aspect);
        if (align == TextAlignment.Left) rect = new Rect(baseRect.x + padding, baseRect.y + padding, W, H);
        else if (align == TextAlignment.Right) rect = new Rect(baseRect.x + baseRect.width - padding - W, baseRect.y + padding, W, H);
        return rect;
    }

    public static void DrawImage(Rect baseRect, TextAlignment align, TextureIcons icon)
    {
        DrawImage(baseRect, align, icon, Vector2.one);
    }
    public static void DrawImage(Rect baseRect, TextAlignment align, TextureIcons icon, Vector2 scale)
    {
        if (icon == TextureIcons.None) return;

        Texture txt = GetIconTexture(icon);

        Rect rect = GetAlignedRect(baseRect, align, icon_padding, txt);

        rect.width = scale.x * rect.width;
        rect.height = scale.y * rect.height;
        //Debug.Log(rect);

        // wscale it

        GUI.DrawTexture(rect, txt, ScaleMode.StretchToFill);
        // GUILayout.Label(" ", GUILayout.Height(height));
    }
    public static bool ButtonOK()
    {
        return GUILayout.Button(MyGUILayout.GetIconTexture(TextureIcons.Ok), GUILayout.Width(60));
    }
    public static bool ButtonCancel()
    {
        return GUILayout.Button(MyGUILayout.GetIconTexture(TextureIcons.Cross), GUILayout.Width(60));
    }


    static Dictionary<string, UI_ID> id_map = new Dictionary<string, UI_ID>();

    public static bool ButtonWithIcon(UI_ID id, string text, GUIStyle style, TextAlignment align, TextureIcons icon, params GUILayoutOption[] options)
    {
        //if (id == null) { id = new UI_ID(); }// id_map.Add(id.guid, id);
        if (!id_map.ContainsKey(id.guid))
            id_map.Add(id.guid, id);
        else
            id = id_map[id.guid];

        bool ok = GUILayout.Button(text, style, options);
        if (Event.current.type == EventType.Repaint)
            id.rect = GUILayoutUtility.GetLastRect();

        id_map[id.guid] = id;

        DrawImage(id.rect, align, icon, Vector2.one);
        return ok;
    }

    public static bool ButtonImage(string text, Texture txt, params GUILayoutOption[] options)
    {
        return GUILayout.Button(new GUIContent(txt, text), options);

    }
    public static bool ButtonImage(Rect rect, string text, Texture txt, params GUILayoutOption[] options)
    {
        return GUI.Button(rect, new GUIContent(txt, text));

    }
    public static bool ButtonImage(Rect rect, TextureIcons icon)
    {
        return GUI.Button(rect, new GUIContent(GetIconTexture(icon)),"button");

    }

    public static bool ButtonImage(string text, TextureIcons icon, params GUILayoutOption[] options)
    {
        return GUILayout.Button(new GUIContent(GetIconTexture(icon), text), options);

    }
    public static bool ButtonImage( TextureIcons icon, params GUILayoutOption[] options)
    {
        return GUILayout.Button(new GUIContent(GetIconTexture(icon)), options);

    }
    public static bool ButtonImage(TextureIcons icon,GUIStyle style, params GUILayoutOption[] options)
    {
        return GUILayout.Button(new GUIContent(GetIconTexture(icon)), style, options);

    }
    public static bool ToggleFolder(UI_ID id, bool toggle, params GUILayoutOption[] options)
    {
        if (!id_map.ContainsKey(id.guid))
            id_map.Add(id.guid, id);
        else
            id = id_map[id.guid];

        bool ok = GUILayout.Toggle(toggle, "", "button", options);
        if (Event.current.type == EventType.Repaint)
            id.rect = GUILayoutUtility.GetLastRect();

        id_map[id.guid] = id;

        DrawImage(id.rect, TextAlignment.Left, toggle ? TextureIcons.FolderMinus : TextureIcons.FolderPlus, new Vector2(1, 1));

        return ok;
        
    }

    public static bool Toggle( string text, bool toggle,params GUILayoutOption[] options)
    {
        UI_ID id = new UI_ID(text);
        //if (!id_map.ContainsKey(id.guid))
        //    id_map.Add(id.guid, id);
        //else
        //    id = id_map[id.guid];

        //bool ok = GUILayout.Toggle(toggle, text, "button", options);
        return ToggleWithIcon(id,text,toggle , "button", TextAlignment.Left , TextureIcons.Ok, new Vector2(0.5f, 0.5f), options);

    }
    public static bool ToggleWithIcon(UI_ID id, string text,bool toggle,  GUIStyle style, TextAlignment align, TextureIcons icon,Vector2 imageScale, params GUILayoutOption[] options)
    {
        if (!id_map.ContainsKey(id.guid))
            id_map.Add(id.guid, id);
        else
            id = id_map[id.guid];

        bool ok = GUILayout.Toggle(toggle,text, style, options);
        if (Event.current.type == EventType.Repaint)
            id.rect = GUILayoutUtility.GetLastRect();

        id_map[id.guid] = id;

        if (toggle)
            DrawImage(id.rect, align, icon, imageScale);
        return ok;
    }
    public static void DrawImageLastControl(TextAlignment align, TextureIcons icon)
    {
        DrawImage(GUILayoutUtility.GetLastRect(), align, icon, Vector2.one);
    }
    public static bool Button(string text,int width)
    {
        return GUILayout.Button(text, GUILayout.Width(width));
    }
    public static void Label(string text, int width)
    {
        GUILayout.Label(text, GUILayout.Width(width));
    }
    public static void Label(string text, Color color)
    {
        var old = GUI.color;
        GUI.color = color;
        GUILayout.Label(text);
        GUI.color = old;
    }
    public static FlashingLabel LabelFlashing(string guid,Rect rect, string text, Color colorA, Color colorB, float period)
    {
        if (!flashingMap.ContainsKey(guid))
        {
            flashingMap.Add(guid, new FlashingLabel() { time = 0, phase = 0 });
        }
        var flash = flashingMap[guid];
        var old = GUI.color;

        if ((Time.time - flash.time) > period)
        {
            flash.time = Time.time;
            flash.phase++; if (flash.phase == 2) flash.phase = 0;
        }

        if (flash.phase == 0)
        {
            var t = (Time.time - flash.time) / period;
            GUI.color = Color.Lerp(colorA, colorB, t);
        }
        if (flash.phase == 1)
        {
            var t = (Time.time - flash.time) / period;
            GUI.color = Color.Lerp(colorB, colorA, t);
        }
        GUI.Label(rect,text);
        GUI.color = old;
        return flash;
    }


    public static FlashingLabel LabelFlashing(string guid, string text, Color colorA, Color colorB,float period)
    {
        if (!flashingMap.ContainsKey(guid))
        {
            flashingMap.Add(guid, new FlashingLabel() {  time=0, phase=0});
        }
        var flash = flashingMap[guid];
        flash.Draw(text, colorA, colorB, period);
        //var old = GUI.color;

        //if ((Time.time - flash.time) > period)
        //{
        //    flash.time = Time.time;
        //    flash.phase++; if (flash.phase == 2) flash.phase = 0;
        //}

        //if (flash.phase == 0 )
        //{
        //    var t = (Time.time - flash.time) / period;
        //    GUI.color = Color.Lerp(colorA, colorB, t);
        //}
        //if (flash.phase == 1)
        //{
        //    var t = (Time.time - flash.time) / period;
        //    GUI.color = Color.Lerp(colorB, colorA, t);
        //}
        //GUILayout.Label(text);
        //GUI.color = old;
        return flash;
    }
    public static void ExpandWidth()
    {
        GUILayout.Label(" ", GUILayout.ExpandWidth(true));
    }
    public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
    {
        GUILayout.Label("", GUILayout.ExpandWidth(true), GUILayout.Height(thickness));
        var rect = GUILayoutUtility.GetLastRect();
        GUI.Box(rect,"",StyleManager.panel);
        //Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        //r.height = thickness;
        //r.y += padding / 2;
        //r.x -= 2;
        //r.width += 6;
        //EditorGUI.DrawRect(r, color);
    }

    private static Texture2D _staticRectTexture;
    private static GUIStyle _staticRectStyle;

    public static void DrawRectBordered(Rect rect, Color colorBorder,Color colorFill)
    {
       // Rect r1 = new Rect(rect.x+1 , rect.y+1, rect.width-2,rect.height-2);
        
        DrawFillRect(rect, colorFill);
        DrawRect(rect, colorBorder);
    }
    public static void DrawRect(Rect position, Color color)
    {
        if (_staticRectTexture == null)
        {
            _staticRectTexture = new Texture2D(1, 1);
        }
        if (_staticRectStyle == null)
        {
            _staticRectStyle = new GUIStyle();
        }

        _staticRectTexture.SetPixel(0, 0, color);
        _staticRectTexture.Apply();

        _staticRectStyle.normal.background = _staticRectTexture;

        GUI.Box(new Rect(position.x, position.yMin, position.width,1), GUIContent.none, _staticRectStyle);
        GUI.Box(new Rect(position.x, position.yMax, position.width, 1), GUIContent.none, _staticRectStyle);
        GUI.Box(new Rect(position.xMin, position.y, 1,position.height), GUIContent.none, _staticRectStyle);
        GUI.Box(new Rect(position.xMax, position.y, 1, position.height), GUIContent.none, _staticRectStyle);
        //GUI.Box(position, GUIContent.none, _staticRectStyle);
    }

    // Note that this function is only meant to be called from OnGUI() functions.
    public static void DrawFillRect(Rect position, Color color)
    {
        if (_staticRectTexture == null)
        {
            _staticRectTexture = new Texture2D(1, 1);
        }
        if (_staticRectStyle == null)
        {
            _staticRectStyle = new GUIStyle();
        }

        _staticRectTexture.SetPixel(0, 0, color);
        _staticRectTexture.Apply();

        _staticRectStyle.normal.background = _staticRectTexture;

        GUI.Box(position, GUIContent.none, _staticRectStyle);
    }

    public static System.Enum EnumPopup(System.Enum e, params GUILayoutOption[] options)
    {
#if UNITY_EDITOR
        return EditorGUILayout.EnumPopup(e, options);
#else
        var layout = layouts.Last();
        layout.childs++;


		string key = "EnumPopup_" + layouts.Count + "_" + e.GetType() + "_" + layout.childs;
		if (!popupList.ContainsKey(key))
			popupList.Add(key, new PopupState()
			{
				draw = (windowID) =>
				{
					var values = System.Enum.GetNames(e.GetType());
                    GUILayout.BeginVertical("Box");
                    //int selGridInt = GUILayout.SelectionGrid(0, values.ToArray(), 1, GUILayout.Width(100));

                    foreach (var s in values)
                    {
                        /* if (GUILayout.Button("s"))
                         {
                             Debug.Log("click " + s);
                         }
                        */
                        GUILayout.Label(s);
                    }

                    GUILayout.EndVertical();

                    if ( Event.current.type == EventType.MouseDown )
					{
						Event.current.Use();  //Eat the event so it doesn't propagate through the editor.
					}

				}
            });

		if (GUILayout.Button(e.ToString()))
		{
			popupList[key].open = !popupList[key].open;

		}
		if (popupList[key].open)
		{
			if (Event.current.type == EventType.Repaint)
			{
				var r = GUILayoutUtility.GetLastRect();
				popupList[key].windowRect = new Rect(r.x, r.y + 20, r.width, r.height * System.Enum.GetNames(e.GetType()).Count() + 5);
			}
            windows.Add(popupList[key]);
           //popupList[key].windowRect = GUILayout.Window(layout.childs, popupList[key].windowRect, popupList[key].draw, "aa");
        }

	

        return e;
#endif
    }

    public static int Popup(int selectedIndex, string[] displayedOptions, params GUILayoutOption[] options)
    {
#if UNITY_EDITOR
        return EditorGUILayout.Popup(selectedIndex, displayedOptions);
#else
    return 0;
#endif
    }

    public static int Popup(int selectedIndex, string[] displayedOptions, GUIStyle style, params GUILayoutOption[] options)
    {
#if UNITY_EDITOR
        return EditorGUILayout.Popup(selectedIndex, displayedOptions, style, options);
#else
    return 0;
#endif
    }

    public static void LabelField(string text, params GUILayoutOption[] options)
    {
        GUILayout.Label(text, options);
    }

    public static void LabelField(string text, GUIStyle style, params GUILayoutOption[] options)
    {
        GUILayout.Label(text, style, options);
    }

    public static int IntSlider(string field,int value, int leftValue, int rightValue, params GUILayoutOption[] options)
    {
#if UNITY_EDITOR
        return EditorGUILayout.IntSlider(field, value, leftValue, rightValue, options);
#else
    return 0;
#endif

    }


#if UNITY_EDITOR

    public static float FloatField(string name, int labelSize,float value,params GUILayoutOption[] p)
    {
        var old = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = labelSize;

        var val = EditorGUILayout.FloatField(name, value, p);

        EditorGUIUtility.labelWidth = old;
        return val;
    }

    public static void AddVector(string field, ref iVector3 vector, iVector3 min, iVector3 max, bool createLayout)
    {
        if (createLayout)
            GUILayout.BeginHorizontal();
        GUILayout.Label(field, GUILayout.Width(50));

        var old = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 20;

        vector.x = Mathf.Clamp(EditorGUILayout.IntField("X", vector.x, GUILayout.Width(60)), min.x, max.x);

        vector.y = Mathf.Clamp(EditorGUILayout.IntField("Y", vector.y, GUILayout.Width(60)), min.y, max.y);

        vector.z = Mathf.Clamp(EditorGUILayout.IntField("Z", vector.z, GUILayout.Width(60)), min.z, max.z);

        EditorGUIUtility.labelWidth = old;
        if (createLayout)
            GUILayout.EndHorizontal();
    }
    public static void AddVector(string field, ref Vector3 vector, Vector3 min, Vector3 max, bool createLayout)
    {
        if (createLayout)
            GUILayout.BeginHorizontal();
        GUILayout.Label(field, GUILayout.Width(50));

        var old = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 20;

        vector.x = Mathf.Clamp(EditorGUILayout.FloatField("X", vector.x, GUILayout.Width(60)), min.x, max.x);

        vector.y = Mathf.Clamp(EditorGUILayout.FloatField("Y", vector.y, GUILayout.Width(60)), min.y, max.y);

        vector.z = Mathf.Clamp(EditorGUILayout.FloatField("Z", vector.z, GUILayout.Width(60)), min.z, max.z);

        EditorGUIUtility.labelWidth = old;
        if (createLayout)
            GUILayout.EndHorizontal();
    }
    public static  void AddVector(string field, ref float[] vector, Vector3 min, Vector3 max, bool createLayout)
    {

        if (createLayout)
            GUILayout.BeginHorizontal();

        GUILayout.Label(field, GUILayout.Width(50));

        var old = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 20;

        vector[0] = Mathf.Clamp(EditorGUILayout.FloatField("X", vector[0], GUILayout.Width(60)), min.x, max.x);

        vector[1] = Mathf.Clamp(EditorGUILayout.FloatField("Y", vector[1], GUILayout.Width(60)), min.y, max.y);

        vector[2] = Mathf.Clamp(EditorGUILayout.FloatField("Z", vector[2], GUILayout.Width(60)), min.z, max.z);

        EditorGUIUtility.labelWidth = old;
        if (createLayout)
            GUILayout.EndHorizontal();
    }
#endif
#if !_UNITY_EDITOR
    private static int activeFloatField = -1;
    private static float activeFloatFieldLastValue = 0;
    private static string activeFloatFieldString = "";
#endif
    /// <summary>
    /// Float Field for ingame purposes. Behaves exactly like UnityEditor.EditorGUILayout.FloatField
    /// </summary>
    public static float FloatField(float value)
    {
#if _UNITY_EDITOR
        return UnityEditor.EditorGUILayout.FloatField(value);
#else

        // Get rect and control for this float field for identification
        Rect pos = GUILayoutUtility.GetRect (new GUIContent (value.ToString ()), GUI.skin.label, new GUILayoutOption[] { GUILayout.ExpandWidth (false), GUILayout.MinWidth (40) });
        int floatFieldID = GUIUtility.GetControlID ("FloatField".GetHashCode (), FocusType.Keyboard, pos) + 1;
        if (floatFieldID == 0)
            return value;
       
        bool recorded = activeFloatField == floatFieldID;
        bool active = floatFieldID == GUIUtility.keyboardControl;
       
        if (active && recorded && activeFloatFieldLastValue != value)
        { // Value has been modified externally
            activeFloatFieldLastValue = value;
            activeFloatFieldString = value.ToString ();
        }
       
        // Get stored string for the text field if this one is recorded
        string str = recorded? activeFloatFieldString : value.ToString ();
       
        // pass it in the text field
        string strValue = GUI.TextField (pos, str);
       
        // Update stored value if this one is recorded
        if (recorded)
            activeFloatFieldString = strValue;
       
        // Try Parse if value got changed. If the string could not be parsed, ignore it and keep last value
        bool parsed = true;
        if (strValue != value.ToString ())
        {
            float newValue;
            parsed = float.TryParse (strValue, out newValue);
            if (parsed)
                value = activeFloatFieldLastValue = newValue;
        }
       
        if (active && !recorded)
        { // Gained focus this frame
            activeFloatField = floatFieldID;
            activeFloatFieldString = strValue;
            activeFloatFieldLastValue = value;
        }
        else if (!active && recorded)
        { // Lost focus this frame
            activeFloatField = -1;
            if (!parsed)
                value = strValue.ForceParse ();
        }
       
        return value;
#endif
    }

    /// <summary>
    /// Float Field for ingame purposes. Behaves exactly like UnityEditor.EditorGUILayout.FloatField
    /// </summary>
    public static float FloatField(GUIContent label, float value)
    {
#if _UNITY_EDITOR
        return UnityEditor.EditorGUILayout.FloatField(label, value);
#else
        GUILayout.BeginHorizontal ();
        GUILayout.Label (label, label != GUIContent.none? GUILayout.ExpandWidth (true) : GUILayout.ExpandWidth (false));
        value = FloatField (value);
        GUILayout.EndHorizontal ();
        return value;
#endif
    }

    /// <summary>
    /// Forces to parse to float by cleaning string if necessary
    /// </summary>
    public static float ForceParse(this string str)
    {
        // try parse
        float value;
        if (float.TryParse(str, out value))
            return value;

        // Clean string if it could not be parsed
        bool recordedDecimalPoint = false;
        List<char> strVal = new List<char>(str);
        for (int cnt = 0; cnt < strVal.Count; cnt++)
        {
            UnicodeCategory type = CharUnicodeInfo.GetUnicodeCategory(str[cnt]);
            if (type != UnicodeCategory.DecimalDigitNumber)
            {
                strVal.RemoveRange(cnt, strVal.Count - cnt);
                break;
            }
            else if (str[cnt] == '.')
            {
                if (recordedDecimalPoint)
                {
                    strVal.RemoveRange(cnt, strVal.Count - cnt);
                    break;
                }
                recordedDecimalPoint = true;
            }
        }

        // Parse again
        if (strVal.Count == 0)
            return 0;
        str = new string(strVal.ToArray());
        if (!float.TryParse(str, out value))
            Debug.LogError("Could not parse " + str);
        return value;
    }

    public static void AddAnimator<T>(string name, ScriptValue<T> f_value)
    {
#if UNITY_EDITOR
        GUILayout.BeginHorizontal();
        if (typeof(T) == typeof(float))
            f_value.SetValue(EditorGUILayout.FloatField(name, f_value.toFloat()));
        else
        {

            GUILayout.Label(name);
            f_value.SetValue(EditorGUILayout.Toggle(f_value.toBool()));
        }

        var open = f_value.mode != ScriptValue_AnimatorMode.Fixed;


        if (GUILayout.Button(!open ? "+" : "-", (!open) ? StyleManager.button_normal : StyleManager.button_selected, GUILayout.Width(30)))
        {
            if (open)
                f_value.mode = ScriptValue_AnimatorMode.Fixed;
            else
                f_value.mode = ScriptValue_AnimatorMode.PingPong;
        }
        GUILayout.EndHorizontal();

        if (f_value.mode != ScriptValue_AnimatorMode.Fixed)
        {
            GUILayout.BeginHorizontal(StyleManager.sub_panel);
            GUILayout.Label(" ", GUILayout.Width(10));
            f_value.animEnabled = GUILayout.Toggle(f_value.animEnabled, "", GUILayout.Width(20));

            GUILayout.Label("Mode", GUILayout.Width(40));
            f_value.mode = (ScriptValue_AnimatorMode)EditorGUILayout.EnumPopup(f_value.mode);

            GUILayout.Label("Snap", GUILayout.Width(40));
            f_value.snap = (ScriptValue_SnapMode)EditorGUILayout.EnumPopup(f_value.snap);

            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                f_value.keyList.Add(new ScriptValue_AnimKey<T>() { time = 0, value = f_value.value });

            }

            GUILayout.EndHorizontal();
            int ii = 1;
            foreach (var key in f_value.keyList.ToArray())
            {
                GUILayout.BeginHorizontal(StyleManager.sub_panel);
                GUILayout.Label(" ", GUILayout.Width(40));
                GUILayout.Label("" + ii, GUILayout.Width(40));
                GUILayout.Label("Time", GUILayout.Width(40));
                key.time = EditorGUILayout.FloatField(key.time, GUILayout.Width(50));
                GUILayout.Label("Value", GUILayout.Width(40));

                if (typeof(T) == typeof(float))
                    key.SetValue(EditorGUILayout.FloatField(key.toFloat(), GUILayout.Width(50)));
                if (typeof(T) == typeof(bool))
                    key.SetValue(EditorGUILayout.Toggle(key.toBool(), GUILayout.Width(50)));

                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    f_value.keyList.Remove(key);

                }
                GUILayout.EndHorizontal();
                ii++;
            }
        }
#endif

    }

}