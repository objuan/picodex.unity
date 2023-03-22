using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ArrowMesh
{
	public static Mesh Create(float len,float rangeDRG,float ray )
	{
		List<Vector3> vertices = new List<Vector3>();
		List<int> indices = new List<int>();
		var range = rangeDRG * Mathf.Deg2Rad;

		int n = 10;
		float step = range / 10;
		float a = -range / 2;

	//	vertices.Add(new Vector3(0, 0, 0));

		for (int i = 0; i < n; i++, a+= step)
		{
			float x = Mathf.Sin(a) * ray;
			float z = Mathf.Cos(a) * ray;

			vertices.Add(new Vector3(x, 0, z));

			// tri
			if (i > 0)
			{
				indices.Add(n);
				indices.Add(1 + i);
				indices.Add(1 + i - 1);
				
			}
		}

		int idx = vertices.Count;

		Vector3 left = new Vector3(vertices[0].x , 0, ray + len);
		Vector3 right = new Vector3(vertices.Last().x, 0, ray + len);
		vertices.Add(left);
		vertices.Add(right);

		indices.Add(idx-1);
		indices.Add(idx);
		indices.Add(idx+1);

		// punta 
		idx = vertices.Count;

		left = new Vector3(vertices[0].x*2, 0, ray + len);
		right = new Vector3(vertices.Last().x*2, 0, ray + len);
		float size = right.x - left.x;
		var top = new Vector3(0, 0, ray + len+ size);

		vertices.Add(left);
		vertices.Add(right);
		vertices.Add(top);

		indices.Add(idx );
		indices.Add(idx+2);
		indices.Add(idx + 1);

		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = indices.ToArray();
		mesh.RecalculateNormals();
		return mesh;
		
	}
}

