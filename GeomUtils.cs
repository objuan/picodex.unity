
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public static class GeomUtils
{
    public static Vector3[] Vertices(this Bounds bounds)
    {
        Vector3[] o = new Vector3[8];
        var boundPoint1 = bounds.min;
        var  boundPoint2 = bounds.max;
        o[0] = boundPoint1;
        o[1] = boundPoint2;
        o[2] = new Vector3(boundPoint1.x, boundPoint1.y, boundPoint2.z);
        o[3] = new   Vector3(boundPoint1.x, boundPoint2.y, boundPoint1.z);
        o[4] = new   Vector3(boundPoint2.x, boundPoint1.y, boundPoint1.z);
        o[5] = new   Vector3(boundPoint1.x, boundPoint2.y, boundPoint2.z);
        o[6] = new   Vector3(boundPoint2.x, boundPoint1.y, boundPoint2.z);
        o[7] = new   Vector3(boundPoint2.x, boundPoint2.y, boundPoint1.z);
        return o;

    }
    public static Vector3[] VerticesScaled(this Bounds bound, Vector3 scale)
    {
        Vector3[] o = new Vector3[8];
        var c = bound.center;
        var size = new Vector3(bound.size.x  * scale.x, bound.size.y * scale.y, bound.size.z * scale.z);
        Bounds bounds = new Bounds(c,size);
        return bounds.Vertices();

    }

    public static Vector3 GetCentroid(Vector3[] pointArray)
    {
        float centroidX = 0.0f;
        float centroidY = 0.0f;
        float centroidZ = 0.0f;

        for (int i = 0; i < pointArray.Length; i++)
        {
            centroidX += pointArray[i].x;
            centroidY += pointArray[i].y;
            centroidZ += pointArray[i].z;
    }
        centroidX /= pointArray.Length;
        centroidY /= pointArray.Length;
        centroidZ /= pointArray.Length;

        return (new Vector3(centroidX, centroidY, centroidZ));
    }
}

 
