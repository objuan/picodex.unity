
using Newtonsoft.Json;
using System;
using UnityEngine;


public class FastArray_Vector3
{
    public int count;
    public Vector3[] values;

    public FastArray_Vector3(int initSize)
    {
        values = new Vector3[initSize];
        count = 0;
    }

    public void Clear()
    {
        count = 0;
    }
    public void AddRange(Vector3[] value)
    {
        int c = value.Length;
        for (int i = 0; i < c; i++)
            values[count++] = value[i] ;
    }

    //public void AddRange(Vector3[] value, Vector3 offset)
    //{
    //    int c = value.Length;
    //    for (int i = 0; i < c; i++)
    //        values[count++] = value[i] + offset;
    //}
}



public class FastArray_Vector2
{
    public int count;
    public Vector2[] values;

    public FastArray_Vector2(int initSize)
    {
        values = new Vector2[initSize];
        count = 0;
    }

    public void Clear()
    {
        count = 0;
    }
    public void Add(Vector2 value)
    {
         values[count++] = value;
    }

    public void AddRange(Vector2[] value)
    {
        int c = value.Length;
        for (int i = 0; i < c; i++)
            values[count++] = value[i] ;
    }

    //public void AddRange(Vector2[] value, Vector2 offset)
    //{
    //    int c = value.Length;
    //    for (int i = 0; i < c; i++)
    //        values[count++] = value[i] + offset;
    //}
    public void AddRange(Vector2[] value, Vector2 scale,  Vector2 offset)
    {
        int c = value.Length;
        for (int i = 0; i < c; i++)
            values[count++] = value[i] * scale + offset;
    }
    public void AddRange(Vector2[] value, float sx,float sy,float ox,float oy)
    {
        int c = value.Length;
        for (int i = 0; i < c; i++)
        {
            values[count].x = value[i].x * sx + ox;
            values[count++].y = value[i].y * sy + oy;
        }
    }

}

public class FastArray_Color
{
    public int count;
    public Color[] values;

    public FastArray_Color(int initSize)
    {
        values = new Color[initSize];
        count = 0;
    }

    public void Clear()
    {
        count = 0;
    }
    public void Add(Color value)
    {
        values[count++] = value;
    }

    public void AddRange(Color[] value)
    {
        int c = value.Length;
        for (int i = 0; i < c; i++)
            values[count++] = value[i];
    }


}