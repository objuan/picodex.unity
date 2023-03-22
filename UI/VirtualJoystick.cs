using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


    public enum VirtualJoystickType
    {
        Vertical,
        Horizontal
    }
    public enum VirtualJoystickTouchSide
    {
        Left,
        Right,
        Fixed
    }
//[ExecuteInEditMode]
[AddComponentMenu("Picodex/UI/VirtualJoystick")]
public class VirtualJoystick : MonoBehaviour
{
  
    public Vector2 value
    {
        get
        {
            if (type == VirtualJoystickType.Vertical)
                return new Vector2(0, (float)((isMovingFinger) ? padControllerRect.center.y : 0) / (padAreaSize * 0.5f));
            else if (type == VirtualJoystickType.Horizontal)
                return new Vector2((float)((isMovingFinger) ? padControllerRect.center.x : 0) / (padAreaSize * 0.5f), 0);
            else
                return Vector2.zero;
        }
    }

    // public 
    public bool showDebugInfo = false;

    public float padRay = 60;
    public float padAreaBorder = 2;
    public float padAreaSize = 150;
    public VirtualJoystickType type = VirtualJoystickType.Vertical;
    public VirtualJoystickTouchSide side = VirtualJoystickTouchSide.Left;

    public Texture2D joystick2D;  // Joystick's Image.
    public Material material;

    public Vector2 backPosition = Vector2.zero;
    public GUIStyle font = new GUIStyle();

    // ----------

    RawImage joystick;
    RawImage background;
    Text label;

    Texture2D padBackgroundTexture;

    Rect backRect;
    Rect valueLabelRect;

    Vector2 touchPosition;
    int fingerId;

    Rect padBackgroundRect = new Rect(0, 0, 100, 100);
    Rect padControllerRect = new Rect(0, 0, 50, 50);

    bool isMovingFinger = false;
    // Camera canvasCamera;
    RectTransform canvasRect;

    public void Awake()
    {
        // overlay mode//  canvasCamera = GameObject.FindObjectOfType<Canvas>().GetComponent
        //  canvasCamera = null;
        canvasRect = GameObject.FindObjectOfType<Canvas>().GetComponent<RectTransform>();

        fingerId = -1;
        this.padBackgroundTexture = new Texture2D(1, 1);
        this.padBackgroundTexture.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        this.padBackgroundTexture.Apply();

        padControllerRect = new Rect(0, 0, padRay * 2, padRay * 2);

        GameObject backOBJ = new GameObject("VJR-Joystick Back");
        backOBJ.transform.SetParent(this.transform, false);
        background = backOBJ.AddComponent<RawImage>();
        background.texture = padBackgroundTexture;
        background.material = material;

        GameObject frontOBJ = new GameObject("VJR-Joystick Front");
        frontOBJ.transform.SetParent(backOBJ.transform, false);
        joystick = frontOBJ.AddComponent<RawImage>();
        joystick.texture = joystick2D; // joystick.color = inactiveColor;
        joystick.material = material;

        GameObject labelOBJ = new GameObject("VJR-Joystick Label");
        labelOBJ.transform.SetParent(this.transform, false);
        label = labelOBJ.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;

        InitPad(backPosition);
        joystick.enabled = false;
        label.enabled = showDebugInfo;
    }

    private void InitPad(Vector2 pos)
    {
        backPosition = pos;
        backRect = new Rect();

        if (type == VirtualJoystickType.Vertical)
        {
            Vector2 size = new Vector2((padAreaBorder + padRay) * 2, padAreaSize + padRay * 2 + padAreaBorder * 2);
            //  Vector2 size = new Vector2((padAreaBorder + padRay) * 2, padAreaSize + padAreaBorder * 2);
            backRect = new Rect(pos.x - size.x / 2, pos.y - size.y / 2, size.x, size.y);

            valueLabelRect = new Rect(pos.x, pos.y + size.y / 2 + 20, 100, 20);
        }
        else if (type == VirtualJoystickType.Horizontal)
        {
            Vector2 size = new Vector2(padAreaSize + padRay * 2 + padAreaBorder * 2, (padAreaBorder + padRay) * 2);
            // Vector2 size = new Vector2(padAreaSize  + padAreaBorder * 2, (padAreaBorder + padRay) * 2);
            backRect = new Rect(pos.x - size.x / 2, pos.y - size.y / 2, size.x, size.y);

            valueLabelRect = new Rect(pos.x, pos.y + size.y / 2 + 20, size.x, 20);
        }

        //background.enabled = true;
        //joystick.enabled = true;
        label.enabled = showDebugInfo; ;
    }

    private void MovePad(Vector2 pos)
    {
        // in dimensioni reali


        touchPosition = pos;
        if (type == VirtualJoystickType.Vertical)
        {
            touchPosition.x = 0;
            touchPosition.y = Math.Min(touchPosition.y, +(padAreaSize * 0.5f));
            touchPosition.y = Math.Max(touchPosition.y, -(padAreaSize * 0.5f));

            padControllerRect.position = touchPosition - new Vector2(padRay, padRay);
        }
        else if (type == VirtualJoystickType.Horizontal)
        {
            touchPosition.y = 0;

            touchPosition.x = Math.Min(touchPosition.x, +(padAreaSize * 0.5f));
            touchPosition.x = Math.Max(touchPosition.x, -(padAreaSize * 0.5f));

            padControllerRect.position = touchPosition - new Vector2(padRay, padRay);
        }
        padControllerRect.size = new Vector2(padRay * 2, padRay * 2);
        joystick.GetComponent<RectTransform>().sizeDelta = padControllerRect.size;

    }

    bool IsGoodHit(Vector2 pos)
    {
        if (side == VirtualJoystickTouchSide.Left && (pos.x > Screen.width / 2 || pos.y < padAreaBorder || pos.y > Screen.height - padAreaBorder))
            return false;
        if (side == VirtualJoystickTouchSide.Right && (pos.x < Screen.width / 2 || pos.x < padAreaBorder || pos.x > Screen.width - padAreaBorder))
            return false;
        return true;
    }

    void Clear()
    {
        this.isMovingFinger = false;
        this.fingerId = -1;
        joystick.enabled = false;
        label.enabled = false;
    }

    void PressBegin(int touchFigerID, Vector2 touchPosition)
    {
        if (isMovingFinger) return;

        fingerId = touchFigerID;
        isMovingFinger = IsGoodHit(touchPosition);
        if (isMovingFinger) InitPad(touchPosition);
    }

    void PressEnd(int touchFigerID, Vector2 touchPosition)
    {
        if (fingerId == touchFigerID)
            Clear();
    }

    void PressMove(int touchFigerID, Vector2 touchPosition)
    {
        if (fingerId != touchFigerID) return;

        if (this.isMovingFinger && IsGoodHit(touchPosition))
        {
            bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(background.rectTransform, touchPosition, null, out touchPosition);
            MovePad(touchPosition);
        }
    }

    public void Update()
    {
        var touchPosition = Vector2.zero;

        foreach (Touch touch in Input.touches)
        {
            touchPosition = new Vector2(touch.position.x, touch.position.y);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    PressBegin(touch.fingerId, touchPosition);
                    break;

                case TouchPhase.Stationary:
                case TouchPhase.Moved:
                    PressMove(touch.fingerId, touchPosition);

                    break;

                case TouchPhase.Ended:
                    PressEnd(touch.fingerId, touchPosition);
                    break;
                case TouchPhase.Canceled:

                    break;
            }
        }

        if (touchPosition == Vector2.zero)
        {
            touchPosition = Input.mousePosition;

            //  touchPosition.y = Screen.height - touchPosition.y;

            if (Input.GetMouseButtonDown(0))
            {
                PressBegin(0, touchPosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                PressEnd(0, touchPosition);
            }
           // else if ()
            {
                PressMove(0, touchPosition);
            }
        }

        //Debug.Log("Pressed " + Pressed + " "+ pressPosition);

        // Debug.Log(pressed);

    }
    public void Update1()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER

        Vector2 touchPosition = Vector2.zero;
        touchPosition = Input.mousePosition;

        //  touchPosition.y = Screen.height - touchPosition.y;

        if (Input.GetMouseButtonDown(0))
        {
            isMovingFinger = IsGoodHit(Input.mousePosition);
            if (isMovingFinger) InitPad(touchPosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Clear();
        }
        else if (Input.GetMouseButton(0))
        {
        }

        if (this.isMovingFinger && IsGoodHit(Input.mousePosition))
        {
            bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(background.rectTransform, Input.mousePosition, null, out touchPosition);
            MovePad(touchPosition);
        }

#else
            foreach(Touch touch  in Input.touches)
            {
                Vector2 touchPosition = Vector2.zero;
                touchPosition = new Vector2(touch.position.x,  touch.position.y);
                int fingerId= touch.fingerId;
                if (this.fingerId != -1 && this.fingerId != fingerId)
                    continue;
                // touch
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        isMovingFinger = IsGoodHit(new Vector2(touch.position.x,  touch.position.y));
                        if (isMovingFinger)
                        {
                            InitPad(touchPosition);
                            this.fingerId=fingerId;
                         }
                        break;

                    case TouchPhase.Stationary:
                    case TouchPhase.Moved:
                        break;

                    case TouchPhase.Ended:
                        Clear();
                        break;
                    case TouchPhase.Canceled:
                         Clear();
                        break;

                }

                if (this.isMovingFinger && IsGoodHit(new Vector2(touch.position.x,  touch.position.y)))
                {
                    bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(background.rectTransform,new Vector2(touch.position.x,  touch.position.y), null, out touchPosition);
                    MovePad(touchPosition);
                }
            }
#endif
        // good hit ?? 




        //float padsDistance = Vector2.Distance(this.padBackgroundPosition, this.padControllerPosition);
        //if (padsDistance > padRay)
        //{
        //    Vector2 padDirection = this.padControllerPosition - this.padBackgroundPosition;
        //    float t = padRay / padsDistance;
        //    this.padBackgroundPosition = Vector2.Lerp(this.padControllerPosition, this.padBackgroundPosition, t);
        //}

        //Vector2 direction = (this.padControllerPosition - this.padBackgroundPosition);
        //float distance = Vector2.Distance(this.padControllerPosition, this.padBackgroundPosition);

        //if (padRadius / distance > 3.5f) this.movement = Vector2.zero;
        //else
        //{
        //    this.movement = direction.normalized;

        //    // if the joystick is not being fully pushed, divide the movement by two
        //    // (to make the player walk or run):
        //    if (padRadius / distance > 1.5f) this.movement /= 2.0f;
        //}


    }


    public void OnGUI()
    {
        background.GetComponent<RectTransform>().position = backRect.center;
        background.GetComponent<RectTransform>().sizeDelta = backRect.size;

        if (this.isMovingFinger)
        {
            joystick.GetComponent<RectTransform>().localPosition = padControllerRect.center;

            label.text = value.ToString();
            label.GetComponent<RectTransform>().position = valueLabelRect.center;
            label.GetComponent<RectTransform>().sizeDelta = valueLabelRect.size;

        }
        background.enabled = this.isMovingFinger;
        label.enabled = showDebugInfo && this.isMovingFinger;
        joystick.enabled = this.isMovingFinger;
    }
}

