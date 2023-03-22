using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Text;


namespace picodex
{
    public class Input 
    {
        internal class TouchInfo
        {
            public Touch touch;
            public bool toRemove = false;
            public int removeFrame = 0;
        }

        public static Vector3 acceleration;

        static picodex.Input.TouchInfo currentTouch = null;
   //     static Vector2 _mousePosition = Vector2.zero;

        internal static List<TouchInfo> touchList = new List<TouchInfo>();

        public static int touchCount => touchList.Count;

        public static bool GetKey(KeyCode key)
		{
            return UnityEngine.Input.GetKey(key);
		}

        public static bool GetMouseButton(int index)
		{
          //  if (currentTouch == null)
                return UnityEngine.Input.GetMouseButton(index);
         //   else
          //      return true;
        }

        public static bool GetMouseButtonUp(int index)
        {
            //if (currentTouch == null)
                return UnityEngine.Input.GetMouseButtonUp(index);
           // else 
           //     return currentTouch.touch.phase == TouchPhase.Ended;
        }

        public static Vector2 mousePosition
        {
			get
			{
              //  if (currentTouch==null)
                    return UnityEngine.Input.mousePosition;
               // else
             //      return currentTouch.touch.position;
            }
           
        }
        public static void Clear()
		{
            touchList.Clear();

        }
        public static void Update()
        {

        }
        public static void LateUpdate()
        {
        //    _mousePosition = Vector2.zero;
        }
        public static Touch GetTouch(int index)
		{
            return touchList[index].touch;
        }

        internal static void OnTouchChanged(picodex.Input.TouchInfo touch)
        {
            if (touch.touch.fingerId==0)
                currentTouch = touch;
        }
        internal static void OnTouchAdded(picodex.Input.TouchInfo touch)
        {
            if (touch.touch.fingerId == 0)
                currentTouch = touch;
        }
        internal static void OnTouchRemoved(picodex.Input.TouchInfo touch)
        {
            if (touch.touch.fingerId == 0)
                currentTouch = null;
        }

        public static bool GetKeyDown(KeyCode key)
        {
            return UnityEngine.Input.GetKeyDown(key);
        }
        public static bool GetKeyUp(KeyCode key)
        {
            return UnityEngine.Input.GetKeyUp(key);
        }
    }

}