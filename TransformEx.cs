using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//namespace Assets.Scripts.Utils


public static class MatrixExtensions
{
    public static Quaternion ExtractRotation(this Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }

    public static Vector3 ExtractPosition(this Matrix4x4 matrix)
    {
        Vector3 position;
        position.x = matrix.m03;
        position.y = matrix.m13;
        position.z = matrix.m23;
        return position;
    }

    public static Vector3 ExtractScale(this Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }
}


public static class TransformEx
	{
		public static void FromMatrix(this Transform transform, Matrix4x4 matrix)
		{
			transform.localScale = matrix.ExtractScale();
			transform.rotation = matrix.ExtractRotation();
			transform.position = matrix.ExtractPosition();
		}

	public static Transform Clear(this Transform transform)
		{
			int childs = transform.childCount;
			for (int i = childs - 1; i >= 0; i--)
			{
				GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
			}
			return transform;
		}

		public static void ZeroLocal(this Transform transform)
		{
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
		}

		public static Transform FindInChildren(this Transform transform,string name)
		{
			var f = transform.Find(name);
			if (f != null) return f;
			else
			{
				for (int i = 0; i < transform.childCount; i++)
				{
					var t = transform.GetChild(i).FindInChildren(name);
					if (t != null) return t;
				}
			}
			return null;
		}
    public static Transform FindChilds(this Transform parent, string name)
    {

        var resultTransform = parent.transform.Find(name);

        // Search through each of parent's children.
        if (resultTransform != null)
        {
            return resultTransform;
        }

        // Perform the search recursively for each child of parent.
        foreach (Transform childTransform in parent.transform)
        {
            var result = FindChilds(childTransform, name);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}

public static class GameObjectEx
{
    public static bool IsOrChildOf(this GameObject go, GameObject who)
    {
        GameObject g = go;
        while (g != who && g != null)
            g = g.transform.parent.gameObject;
        return g != null;
    }
    public static bool IsChildOf(this GameObject go, GameObject who)
    {
        GameObject g = go.transform.parent.gameObject;
        while (g != who && g != null)
            g = g.transform.parent.gameObject;
        return g != null;
    }

}

