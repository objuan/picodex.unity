
using UnityEngine;

public static class Vector3Extensions
{
	public static iVector3 Min(this iVector3 v, iVector3 a)
	{
		return new iVector3(Mathf.Min(v.x, a.x), Mathf.Min(v.y, a.y), Mathf.Min(v.z, a.z));
	}
	public static Vector3 Min(this Vector3 v, Vector3 a)
	{
		return new Vector3(Mathf.Min( v.x,a.x), Mathf.Min(v.y, a.y), Mathf.Min(v.z, a.z));
	}
	public static iVector3 Max(this iVector3 v, iVector3 a)
	{
		return new iVector3(Mathf.Max(v.x, a.x), Mathf.Max(v.y, a.y), Mathf.Max(v.z, a.z));
	}
	public static Vector3 Max(this Vector3 v, Vector3 a)
	{
		return new Vector3(Mathf.Max(v.x, a.x), Mathf.Max(v.y, a.y), Mathf.Max(v.z, a.z));
	}
	public static Vector3 Abs(this Vector3 v)
	{
		return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
	}

	public static Vector3 Inverse(this Vector3 v)
	{
		return new Vector3(1f / v.x, 1f / v.y, 1f / v.z);
	}


	public static Vector3 RotateXZ(this Vector3 v, float degrees)
	{
		float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
		float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

		float tx = v.x;
		float tz = v.z;
		v.x = (cos * tx) - (sin * tz);
		v.z = (sin * tx) + (cos * tz);
		return v;
	}
	public static Vector3 Div(this Vector3 v, float a)
	{
		return new Vector3(v.x / a, v.y / a, v.z / a);
	}
	public static Vector3 Div(this Vector3 v, Vector3 v2)
	{
		return new Vector3(v.x / v2.x, v.y / v2.y, v.z / v2.z);
	}
	public static Vector3 Mult(this Vector3 v, Vector3 v2)
	{
		return new Vector3(v.x * v2.x, v.y * v2.y, v.z * v2.z);
	}
	public static Vector2 Mult(this Vector2 v, Vector2 v2)
	{
		return new Vector3(v.x * v2.x, v.y * v2.y);
	}
	public static iVector3 Mult(this Vector3 v, iVector3 v2)
	{
		return new iVector3((int)(v.x * v2.x), (int)(v.y * v2.y), (int)(v.z * v2.z));
	}

	public static iVector3 ToiVector3(this Vector3 v)
	{
		return new iVector3(v.x,v.y,v.z);
	}
}



public static class Matrix4x4Extensions
{
	public static float[] ToFLoatArray(this Matrix4x4 m)
	{
		return new float[] { m.m00 , m.m10 , m.m20 , m.m30,
		m.m01 , m.m11 , m.m21 , m.m31,
		m.m02 , m.m12 , m.m22 , m.m32,
		m.m03 , m.m13 , m.m23 , m.m33};
	}


	public static Matrix4x4 FromFloatArray(this Matrix4x4 v, float[] arr)
	{
	
		var m =  new Matrix4x4(new Vector4(arr[0], arr[1], arr[2], arr[3]),
			new Vector4(arr[4], arr[5], arr[6], arr[7]),
			new Vector4(arr[8], arr[9], arr[10], arr[11]),
			new Vector4(arr[12], arr[13], arr[14], arr[15]));
		v = m;
		return m;
	 }
}