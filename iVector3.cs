
using Newtonsoft.Json;
using System;
using UnityEngine;

[System.Serializable]
public struct iVector3
{
    public static iVector3 zero = new iVector3(0,0,0);
    public static iVector3 one = new iVector3(1,1,1);

    public int x;
    public int y;
    public int z;

    public int this[int idx]
    {
		get
		{
            return idx==0 ? x : ((idx ==1) ? y : z);
		}
        set
        {
            if (idx == 0) x = value;
            else if (idx == 1) y = value;
            else z = value;
        }
    }

    [JsonIgnore]
    public float Length => Mathf.Sqrt( x*x + y*y + z*z);
    [JsonIgnore]
    public float LengthSquared => (x * x + y * y + z * z);

    public iVector3(float[] v)
    {
        this.x = (int)v[0];
        this.y = (int)v[1];
        this.z = (int)v[2];
    }
    public iVector3(int[] v)
    {
        this.x = (int)v[0];
        this.y = (int)v[1];
        this.z = (int)v[2];
    }
    public iVector3(int _x,int _y,int _z)
    {
        this.x = _x;
        this.y = _y;
        this.z = _z;
    }
    public iVector3(float _x, float _y, float _z)
    {
        this.x = (int) _x;
        this.y = (int)_y;
        this.z = (int)_z;
    }
    public iVector3(Vector3 v)
    {
        this.x = (int)v.x;
        this.y = (int)v.y;
        this.z = (int)v.z;
    }

    public static iVector3 operator +(iVector3 a, iVector3 b)
    {
        return new iVector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    public static iVector3 operator +(iVector3 a, float b)
    {
        return new iVector3(a.x + b, a.y + b, a.z + b);
    }
    public static iVector3 operator -(iVector3 a, iVector3 b)
    {
        return new iVector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }
    public static iVector3 operator -(iVector3 a, int b)
    {
        return new iVector3(a.x - b, a.y - b, a.z - b);
    }
    public static iVector3 Sub(iVector3 a, iVector3 b)
    {
        return new iVector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }
    public static iVector3 Add(iVector3 a, iVector3 b)
    {
        return new iVector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    public static iVector3 operator /(iVector3 a, float b)
    {
        return new iVector3(a.x /b, a.y / b, a.z  /b);
    }
    public static iVector3 operator *(iVector3 a, float b)
    {
        return new iVector3(a.x * b, a.y * b, a.z* b);
    }
    public static float Distance(iVector3 a, iVector3 b)
    {
        return (a-b).Length;
    }
    public static float DistanceSquared(iVector3 a, iVector3 b)
    {
        return (a-b).LengthSquared;
    }
    public iVector3 Abs()
    {
        return new iVector3(Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(z));
    }

    public override bool Equals(object obj)
    {
        iVector3 a = (iVector3)obj;
        return x == a.x && y == a.y && z == a.z;
    }
    public override string ToString()
    {
        return "" + x + "," + y + "," + z;
    }

    public override int GetHashCode()
    {
        return x + y << 8 + z << 16;
    }

    public  Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}



