using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//namespace Assets.Scripts.Utils

public enum AnchorPresets
{
    TopLeft,
    TopCenter,
    TopRight,

    MiddleLeft,
    MiddleCenter,
    MiddleRight,

    BottomLeft,
    BottonCenter,
    BottomRight,
    BottomStretch,

    VertStretchLeft,
    VertStretchRight,
    VertStretchCenter,

    HorStretchTop,
    HorStretchMiddle,
    HorStretchBottom,

    StretchAll
}

public enum PivotPresets
{
    TopLeft,
    TopCenter,
    TopRight,

    MiddleLeft,
    MiddleCenter,
    MiddleRight,

    BottomLeft,
    BottomCenter,
    BottomRight,
}


public static class RecangleExt
{
    static Camera GetCamera(Canvas canvas)
    {
        return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? Camera.main : Camera.main;// this.Camera;
       
    }

    public static bool MousePick(this RectTransform transform, Vector2 moysePos, ref Vector2 localPos)
    {
        var canvas = transform.GetComponentInParent<Canvas>();
        Camera camera = GetCamera(canvas);

        if (RectTransformUtility.RectangleContainsScreenPoint(transform, moysePos, camera))
        {
            Vector2 movePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                  transform,
                  moysePos, camera,
                  out movePos);


            localPos = movePos - new Vector2(canvas.transform.position.x, canvas.transform.position.y);
            return true;
        }
        else
            return false;
    }

    public static bool MousePick(this RectTransform transform, ref Vector2 localPos)
    {
        var canvas = transform.GetComponentInParent<Canvas>();
        Camera camera = GetCamera(canvas);

        if (RectTransformUtility.RectangleContainsScreenPoint(transform, Input.mousePosition, camera))
        {
            Vector2 movePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                  transform,
                  Input.mousePosition, camera,
                  out movePos); 


            localPos = movePos - new Vector2(canvas.transform.position.x, canvas.transform.position.y);
            return true;
        }
        else 
            return false;
    }

    public static Rect ToGuiBound(this RectTransform transform)
    {
        var worldCorners = new Vector3[4];
        transform.GetWorldCorners(worldCorners);
        var result = new Rect( // ho invertito la Y
                      worldCorners[0].x,
                      Screen.height -  worldCorners[2].y,
                      worldCorners[2].x - worldCorners[0].x,
                      worldCorners[2].y - worldCorners[0].y);
        return result;

        //var canvas = transform.GetComponentInParent<Canvas>();

        //RectTransformUtility.WorldToScreenPoint();

        //Vector2 lt;
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(transform, Input.mousePosition, GetCamera(), out lt);

        //var lt = GetCamera(canvas).WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y, transform.position.z));
        //var rb = GetCamera(canvas).WorldToScreenPoint(new Vector3(transform.position.x+10, transform.position.y+10, transform.position.z));
        ////var lt = Camera.main.WorldToScreenPoint(new Vector2( transform.rect.left , transform.rect.top));
        ////var rb = Camera.main.WorldToScreenPoint(new Vector2(transform.rect.right, transform.rect.bottom));

        //return new Rect(lt,  rb-lt) ;
    }

    public static Rect ToScreenRect(this RectTransform transform)
    {
        var worldCorners = new Vector3[4];
        transform.GetWorldCorners(worldCorners);
        var result = new Rect(
                      worldCorners[0].x,
                      Screen.height - worldCorners[2].y,
                      worldCorners[2].x - worldCorners[0].x,
                      worldCorners[2].y - worldCorners[0].y);
        return result;
        //var worldCorners = new Vector3[4];
        //transform.GetWorldCorners(worldCorners);

        //var canvas = transform.GetComponentInParent<Canvas>();

        //for (int i = 0; i < 4; i++)
        //    worldCorners[i] = GetCamera(canvas).WorldToScreenPoint(worldCorners[i]);

        //var result = new Rect( // ho invertito la Y
        //              worldCorners[0].x,
        //              Screen.height - worldCorners[2].y,
        //              worldCorners[2].x - worldCorners[0].x,
        //              worldCorners[2].y - worldCorners[0].y);
        //return result;

        //  GetCamera
        //var canvas = transform.GetComponentInParent<Canvas>();

        //RectTransformUtility.WorldToScreenPoint();

        //Vector2 lt;
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(transform, Input.mousePosition, GetCamera(), out lt);

        //var lt = GetCamera(canvas).WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y, transform.position.z));
        //var rb = GetCamera(canvas).WorldToScreenPoint(new Vector3(transform.position.x + 10, transform.position.y + 10, transform.position.z));
        //var lt = Camera.main.WorldToScreenPoint(new Vector2( transform.rect.left , transform.rect.top));
        //var rb = Camera.main.WorldToScreenPoint(new Vector2(transform.rect.right, transform.rect.bottom));

        //return new Rect(lt,  rb-lt) ;
    }

    public static void SetAnchor(this RectTransform source, AnchorPresets allign, int offsetX = 0, int offsetY = 0)
    {
        source.anchoredPosition = new Vector3(offsetX, offsetY, 0);

        switch (allign)
        {
            case (AnchorPresets.TopLeft):
                {
                    source.anchorMin = new Vector2(0, 1);
                    source.anchorMax = new Vector2(0, 1);
                    break;
                }
            case (AnchorPresets.TopCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 1);
                    source.anchorMax = new Vector2(0.5f, 1);
                    break;
                }
            case (AnchorPresets.TopRight):
                {
                    source.anchorMin = new Vector2(1, 1);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }

            case (AnchorPresets.MiddleLeft):
                {
                    source.anchorMin = new Vector2(0, 0.5f);
                    source.anchorMax = new Vector2(0, 0.5f);
                    break;
                }
            case (AnchorPresets.MiddleCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 0.5f);
                    source.anchorMax = new Vector2(0.5f, 0.5f);
                    break;
                }
            case (AnchorPresets.MiddleRight):
                {
                    source.anchorMin = new Vector2(1, 0.5f);
                    source.anchorMax = new Vector2(1, 0.5f);
                    break;
                }

            case (AnchorPresets.BottomLeft):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(0, 0);
                    break;
                }
            case (AnchorPresets.BottonCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 0);
                    source.anchorMax = new Vector2(0.5f, 0);
                    break;
                }
            case (AnchorPresets.BottomRight):
                {
                    source.anchorMin = new Vector2(1, 0);
                    source.anchorMax = new Vector2(1, 0);
                    break;
                }

            case (AnchorPresets.HorStretchTop):
                {
                    source.anchorMin = new Vector2(0, 1);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }
            case (AnchorPresets.HorStretchMiddle):
                {
                    source.anchorMin = new Vector2(0, 0.5f);
                    source.anchorMax = new Vector2(1, 0.5f);
                    break;
                }
            case (AnchorPresets.HorStretchBottom):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(1, 0);
                    break;
                }

            case (AnchorPresets.VertStretchLeft):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(0, 1);
                    break;
                }
            case (AnchorPresets.VertStretchCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 0);
                    source.anchorMax = new Vector2(0.5f, 1);
                    break;
                }
            case (AnchorPresets.VertStretchRight):
                {
                    source.anchorMin = new Vector2(1, 0);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }

            case (AnchorPresets.StretchAll):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }
        }
    }

    public static void SetPivot(this RectTransform source, PivotPresets preset)
    {

        switch (preset)
        {
            case (PivotPresets.TopLeft):
                {
                    source.pivot = new Vector2(0, 1);
                    break;
                }
            case (PivotPresets.TopCenter):
                {
                    source.pivot = new Vector2(0.5f, 1);
                    break;
                }
            case (PivotPresets.TopRight):
                {
                    source.pivot = new Vector2(1, 1);
                    break;
                }

            case (PivotPresets.MiddleLeft):
                {
                    source.pivot = new Vector2(0, 0.5f);
                    break;
                }
            case (PivotPresets.MiddleCenter):
                {
                    source.pivot = new Vector2(0.5f, 0.5f);
                    break;
                }
            case (PivotPresets.MiddleRight):
                {
                    source.pivot = new Vector2(1, 0.5f);
                    break;
                }

            case (PivotPresets.BottomLeft):
                {
                    source.pivot = new Vector2(0, 0);
                    break;
                }
            case (PivotPresets.BottomCenter):
                {
                    source.pivot = new Vector2(0.5f, 0);
                    break;
                }
            case (PivotPresets.BottomRight):
                {
                    source.pivot = new Vector2(1, 0);
                    break;
                }
        }
    }
}

