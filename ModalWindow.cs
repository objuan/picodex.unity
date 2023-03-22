using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


public class ModalWindow
{
    private static int m_WinIDManager = 1337;
    protected int m_WinID = m_WinIDManager++;
    protected bool enabled = true;

    public event System.Action<ModalWindow> OnWindow;
    public string title;
    public Rect position;
    public bool ShouldClose = false;
    public int ID { get { return m_WinID; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        //  Debug.Log("Counter reset.");
        m_WinIDManager = 1337;
    }

    public ModalWindow(Rect aInitialPos, string aTitle)
    {
        position = aInitialPos;
        title = aTitle;
    }
    public ModalWindow(Rect aInitialPos, string aTitle, System.Action<ModalWindow> aCallback)
    {
        position = aInitialPos;
        title = aTitle;
        OnWindow += aCallback;
    }
    public virtual void OnGUI()
    {
        // For some strange reason Unity only disables the window border but not the content
        // so we save the enabled state and use it inside the window callback down below.
        enabled = GUI.enabled;
        position = GUI.Window(m_WinID, position, DrawWindow, title);
    }

    protected virtual void DrawWindow(int id)
    {
        // restore the enabled state
        GUI.enabled = enabled;
        if (OnWindow != null)
            OnWindow(this);
        DrawEnd();
    }

    protected virtual void DrawEnd()
    {
     
    }

    public virtual void Close()
    {
        ShouldClose = true;
    }
}

public class DialogModalWindow : ModalWindow
{
    private System.Action<DialogModalWindow,bool> onExit=null;

    public object returnValue;

    public DialogModalWindow(Rect aInitialPos, string aTitle, System.Action<DialogModalWindow> aCallback,System.Action<DialogModalWindow,bool> onExit) 
        : base(aInitialPos,aTitle, (w) => { aCallback((DialogModalWindow)w);  })
    {
        this.onExit = onExit;
    }

    protected override void DrawEnd()
    {
        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        //    OpenPopup(aTitle + "->subwindow");
        if (GUILayout.Button("Ok"))
		{
            onExit(this,true);
            Close();
        }

        if (GUILayout.Button("Close"))
        {
            onExit(this,false);
            Close();
        }
            
        GUILayout.EndHorizontal();
        GUI.DragWindow();
    }
}

public class DialogModalWindowUtils
{
    public static DialogModalWindow ShowConfirm(Rect aInitialPos, string text, System.Action<bool> onReturn)
    {
       return new  DialogModalWindow(aInitialPos, "Alert", (w) =>
        {
            GUILayout.Label(text);
        }, (w, ret) =>
        {
            onReturn(ret);
        });
    }
}

public class ModalSystem
{
    private List<ModalWindow> m_List = new List<ModalWindow>();
    public bool IsWindowOpen { get { return m_List.Count > 0; } }
    public ModalWindow Top
    {
        get { return IsWindowOpen ? m_List[m_List.Count - 1] : null; }
    }
    public void Draw()
    {
        // remove closed windows
        if (Event.current.type == EventType.Layout)
        {
            for (int i = m_List.Count - 1; i >= 0; i--)
                if (m_List[i].ShouldClose)
                    m_List.RemoveAt(i);
        }
        if (m_List.Count > 0)
        {
            // draw all windows
            for (int i = 0; i < m_List.Count; i++)
            {
                GUI.enabled = (i == m_List.Count - 1); // disable all except the last
                GUI.BringWindowToFront(m_List[i].ID); // order them from back to front
                GUI.FocusWindow(m_List[i].ID);       //               ||
                m_List[i].OnGUI();
            }
        }
    }
    public void Add(ModalWindow aWindow)
    {
        m_List.Add(aWindow);
    }
}

///// <summary>
///// Define a popup window that return a result.
///// Base class for IModal call implementation.
///// </summary>
//public abstract class _ModalWindow : EditorWindow
//{
//    public const float TITLEBAR = 18;

//    protected string Title = "ModalWindow";

//    public Vector2 Position = Vector2.zero;
//    public virtual Vector2 Size { get; } = new Vector2(300, 300);

//    void Start()
//    {
//        if (Position == Vector2.zero)
//        {
//            int winW = (int)Size.x;
//            int winH = (int)Size.y;
//            int winX = (Screen.currentResolution.width - winW) / 2;
//            int winY = (Screen.currentResolution.height - winH) / 2;
//            position = new Rect(winX, winY, winW, winH);
//        }
//        else
//            position = new Rect(Position.x, Position.y, Size.x, Size.y);
//    }


//    private void OnGUI()
//    {
//        titleContent = new GUIContent(Title);

//        GUILayout.BeginArea(new Rect(0, 0, position.width, position.height));
//        GUILayout.BeginHorizontal(EditorStyles.toolbar);

//        GUILayout.Label(Title, EditorStyles.boldLabel);

//        GUILayout.EndHorizontal();
//        GUILayout.EndArea();

//        GUILayout.Space(10);

//        Rect content = new Rect(0, TITLEBAR, position.width, position.height - TITLEBAR);
//        Start();

//        Draw(content);

//        GUILayout.BeginHorizontal();
//        if (GUILayout.Button("Ok"))
//        {
//            Close();
//        }

//        if (GUILayout.Button("Cancel"))
//            Close();
//        GUILayout.EndHorizontal();
//    }


//    protected abstract void Draw(Rect region);
//}

//public class AskString : _ModalWindow
//{
//    public System.Action<string> handler;
//    public string desc;
//    public string value;


//    public override Vector2 Size { get; } = new Vector2(300, 300);

//    protected override void Draw(Rect region)
//    {
//        value = EditorGUILayout.TextField(desc, value);
//    }
//    public static void ShowDialog(Vector2 pos, string title, string desc, string value, System.Action<string> handler)
//    {
//        var w = ScriptableObject.CreateInstance<AskString>();
//        w.desc = desc;
//        w.Position = pos;
//        w.Title = title;
//        w.value = value;
//        w.handler = handler;
//        w.ShowPopup();
//    }
//}

//public class EditorWindowTest : EditorWindow
//{
//    [MenuItem("Tools/TestWindow")]
//    public static void Init()
//    {
//        GetWindow<EditorWindowTest>();
//    }

//    ModalSystem modalWindows = new ModalSystem();

//    void OpenPopup(string aTitle)
//    {
//        var win = new ModalWindow(new Rect(30, 30, position.width - 60, position.height - 60), aTitle, (w) =>
//        {
//            if (GUILayout.Button("Open another popup"))
//                OpenPopup(aTitle + "->subwindow");
//            if (GUILayout.Button("Close"))
//                w.Close();
//            GUI.DragWindow();
//        });
//        modalWindows.Add(win);
//    }

//    void OnGUI()
//    {
//        GUI.enabled = !modalWindows.IsWindowOpen;
//        if (GUILayout.Button("Open Popup"))
//        {
//            OpenPopup("First");
//        }
//        if (GUILayout.Button("Some other GUI stuff"))
//        {
//            Debug.Log("Doing stuff...");
//        }
//        BeginWindows();
//        modalWindows.Draw();
//        EndWindows();
//    }
//}