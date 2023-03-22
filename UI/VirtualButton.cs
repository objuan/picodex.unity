using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using UnityEngine;
using UnityEngine.UI;




//[ExecuteInEditMode]
[AddComponentMenu("Picodex/UI/VirtualButton")]
public class VirtualButton : MonoBehaviour
{
    public Color NormalColor = Color.white;
    public Color PressColor = Color.yellow;
    public VirtualJoystickTouchSide side = VirtualJoystickTouchSide.Right;

    public float touchAreaPerc = 0.3f;
    public float padAreaBorder = 10;

    int fingerId = 0;
    Vector2 pressPosition;
    bool _pressed = false;


    RectTransform rect;

    public event EventHandler PressChanged;

    public bool Pressed
    {
        get => _pressed; set
        {
            if (_pressed != value)
            {
                _pressed = value;
                if (_pressed)
                {
                    GetComponent<Image>().color = PressColor;
                }
                else
                    GetComponent<Image>().color = NormalColor;
                if (PressChanged != null)
                    PressChanged(this, EventArgs.Empty);
            }
        }
    }

    private void Start()
    {
        rect = this.GetComponent<RectTransform>();

        if (side != VirtualJoystickTouchSide.Fixed)
        {
            SetVisible(false);
        }
    }

    void SetVisible(bool visible)
    {
        this.gameObject.GetComponent<Image>().enabled = visible;
        this.gameObject.GetComponentInChildren<Text>().enabled = visible;
    }

    bool IsGoodHit(Vector2 pos)
    {
        if (side == VirtualJoystickTouchSide.Left && (pos.x > Screen.width * touchAreaPerc || pos.y < padAreaBorder || pos.y > Screen.height - padAreaBorder))
            return false;
        if (side == VirtualJoystickTouchSide.Right && (pos.x < Screen.width- Screen.width * touchAreaPerc || pos.y < padAreaBorder || pos.y > Screen.height - padAreaBorder))
            return false;
        if (side == VirtualJoystickTouchSide.Fixed)
            return true;
        return true;
    }


    void PressBegin(int touchFigerID, Vector2 touchPosition)
    {
        if (!Pressed)
        {
            if (IsGoodHit(touchPosition))
            {
                if (side == VirtualJoystickTouchSide.Fixed)
                {
                    bool isOver = RectTransformUtility.RectangleContainsScreenPoint(rect, touchPosition);
                    if (isOver)
                    {
                        Pressed = true;
                        pressPosition = touchPosition;
                        fingerId = touchFigerID;
                    }
                }
                else
                {
                    var backRect = new Rect();
                    float padRay = rect.rect.width;

                    Vector2 size = new Vector2(padRay, padRay);
         
                    backRect = new Rect(touchPosition.x - size.x / 2, touchPosition.y - size.y / 2, size.x, size.y);

                    GetComponent<RectTransform>().position = backRect.center;

                    SetVisible(true);

                    Pressed = true;
                    pressPosition = touchPosition;
                    fingerId = touchFigerID;
                }
            }
        }
    }
    void PressEnd(int touchFigerID, Vector2 touchPosition)
    {
        if (Pressed && touchFigerID == fingerId)
        {
            Pressed = false;

            if (side != VirtualJoystickTouchSide.Fixed)
            {
                SetVisible(false);
            }
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
                    PressBegin(touch.fingerId,touchPosition);
                    //if (!Pressed)
                    //{
                    //    bool isOver = RectTransformUtility.RectangleContainsScreenPoint(rect, touchPosition);
                    //    if (isOver)
                    //    {
                    //        Pressed = true;
                    //        pressPosition = touchPosition;
                    //        fingerId = touch.fingerId;
                    //    }
                    //}
                    break;

                case TouchPhase.Stationary:
                case TouchPhase.Moved:
                    break;

                case TouchPhase.Ended:
                    PressEnd(touch.fingerId, touchPosition);
                    //if (Pressed && touch.fingerId == fingerId)
                    //{
                    //    Pressed = false;
                    //}
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
                //bool isOver = RectTransformUtility.RectangleContainsScreenPoint(rect, touchPosition);
                //if (isOver)
                //{
                //    fingerId = 0;
                //    pressPosition = touchPosition;
                //    Pressed = true;
                //}
            }
            else if (Input.GetMouseButtonUp(0))
            {
                PressEnd(0, touchPosition);
                //if (Pressed)
                //{
                //    Pressed = false;
                //}
            }
            else if (Input.GetMouseButton(0))
            {
            }

        }

        //Debug.Log("Pressed " + Pressed + " "+ pressPosition);

        // Debug.Log(pressed);
    }



}

