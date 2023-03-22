namespace ProceduralPrimitives
{
    using System;
	using System.Collections.Generic;
	using UnityEngine;

   public static partial class Primitive
    {
		public static Mesh CreateArcMeshXZ(float ray_outher, float ray_inner, float _from, float _to, int steps,bool flip)
		{
			float from = Mathf.Deg2Rad *_from;
			float to = Mathf.Deg2Rad * _to;

			var vertices = new List<Vector3>();
			List<int> tris = new List<int>();
			List<Vector2> uvs = new List<Vector2>();

			float d = to - from;
			float step =  d / steps;
			float a = from;
			int idx = 0;
			for (int i = 0; i <= steps; i++, a += step, idx+=2)
			{
				float x = Mathf.Sin(a);
				float z = Mathf.Cos(a);
				Vector3 inner = new Vector3(x *  ray_inner,0, z *  ray_inner);
				Vector3 outer = new Vector3(x * ray_outher,0, z * ray_outher);

				vertices.Add(inner);
				vertices.Add(outer);
				float u = ((float)i / steps);
				uvs.Add(new Vector2( u, 0));
				uvs.Add(new Vector2(u, 1));

				if (i > 0)
				{
					if (!flip)
					{
						tris.Add(idx - 2);
						tris.Add(idx - 1);
						tris.Add(idx + 1);

						tris.Add(idx - 2);
						tris.Add(idx + 1);
						tris.Add(idx);
					}
					else
					{
						tris.Add(idx - 2);
						tris.Add(idx + 1);
						tris.Add(idx - 1);
						

						tris.Add(idx - 2);
						tris.Add(idx);
						tris.Add(idx + 1);
						
					}
				}
			}

			Mesh mesh = new Mesh();
			mesh.vertices = vertices.ToArray();
			mesh.triangles = tris.ToArray();
			mesh.uv = uvs.ToArray();
			return mesh;
		}

		public static Mesh CreateCircolarMesh(float ray_outher, float ray_inner,  float z_outer, float z_inner, int denti,bool flipZ )
		{
			if (denti == 0)  denti = 20;
			float  angle = 2f * Mathf.PI / denti;

			Vector3 center = new Vector3(0, 0, z_inner);
			var vertices = new List<Vector3>();
			List<int> tris = new List<int>();
			List<Vector2> uvs = new List<Vector2>();
			vertices.Add(center);
			uvs.Add(new Vector2(0,0));
			float a = 0;
			for (int i=0;i<= denti;i++,a+= angle)
			{
				Vector3 inner = new Vector3(Mathf.Sin(a) * ray_inner, Mathf.Cos(a) * ray_inner, z_inner);
				vertices.Add(inner);
				uvs.Add(new Vector2(inner.x, inner.y)); 
				if (a > 0)
				{
					tris.Add(0);
					if (flipZ)
					{
						tris.Add(vertices.Count - 2);
						tris.Add(vertices.Count - 1);
					}
					else
					{
						tris.Add(vertices.Count - 1);
						tris.Add(vertices.Count - 2);
					}
				}
			}
			Mesh mesh = new Mesh();
			mesh.vertices = vertices.ToArray();
			mesh.triangles = tris.ToArray();
			mesh.uv = uvs.ToArray();
			return mesh;

		}

		public static Mesh CreateCircolarDualMesh(float ray_outher, float ray_inner, float depth_outher, float depth_inner, int denti ,bool flipZ)
		{
			// inner

			Mesh mesh1 = CreateCircolarMesh(ray_outher, ray_inner, depth_outher, depth_inner, denti, flipZ);

			// out
			int d = 20;
			if (denti != 0) d = denti;
			float angle = 2f * Mathf.PI / d;

			var vertices = new List<Vector3>();
			var tris = new List<int>();
			var uvs = new List<Vector2>();

			var in_vertices = mesh1.vertices;
			vertices.AddRange(in_vertices);
			uvs.AddRange(mesh1.uv);

			float a = angle / 2;
			if (denti == 0) a = 0;

			for (int i = 1; i <= d + 1; i++, a += angle)
			{
				if (denti > 0)
				{
					Vector3 o = new Vector3(Mathf.Sin(a) * ray_outher, Mathf.Cos(a) * ray_outher, depth_outher);
					vertices.Add(o);
					uvs.Add(new Vector2(o.x, o.y));

					if (i <= d)
					{
						tris.Add(i);
						if (flipZ)
						{
							tris.Add(vertices.Count - 1);
							tris.Add(i + 1);
						}
						else
						{
							tris.Add(i + 1);
							tris.Add(vertices.Count - 1);
						}

					}
				}
				else
				{
					Vector3 o = new Vector3(Mathf.Sin(a) * ray_outher, Mathf.Cos(a) * ray_outher, depth_outher);
					vertices.Add(o);
					uvs.Add(new Vector2(o.x, o.y));

					if (i <= d)
					{
						tris.Add(i);
						if (flipZ)
						{
							tris.Add(vertices.Count - 1);
							tris.Add(i + 1);
						}
						else
						{
							tris.Add(i + 1);
							tris.Add(vertices.Count - 1);
						}
						// dual
						if (flipZ)
						{
							tris.Add(i + 1);
							tris.Add(vertices.Count - 1);
							tris.Add(vertices.Count);

							
						}
						else
						{
							tris.Add(i + 1);
							tris.Add(vertices.Count);
							tris.Add(vertices.Count - 1);
						}
					}
				}
			}

			Mesh mesh2 = new Mesh();
			mesh2.vertices = vertices.ToArray();
			mesh2.triangles = tris.ToArray();
			mesh2.uv = uvs.ToArray();

			var mesh = MeshUtils.Merge(mesh1, mesh2);
			return mesh;
		}

		public static Mesh CreateCircolarDualFilledMesh(float ray_outher, float ray_inner, float depth_outher, float depth_inner, int denti = 0)
		{
			if (denti > 0)
			{
				var d = denti;

				Mesh mesh1 = CreateCircolarDualMesh(ray_outher, ray_inner, depth_outher, depth_inner / 2, denti, false);
				Mesh mesh2 = CreateCircolarDualMesh(ray_outher, ray_inner, depth_outher, -depth_inner / 2, denti, true);


				var vertices = new List<Vector3>();
				var tris = new List<int>();
				var uvs = new List<Vector2>();

				var in_vertices1 = mesh1.vertices;
				var in_vertices2 = mesh2.vertices;

				for (int i = 0, ii = 1; i <= d; i++, ii++)
				{
					vertices.Add(in_vertices1[ii]);
					vertices.Add(in_vertices2[ii]);

					vertices.Add(in_vertices1[d * 2 + ii + 1]);

					tris.Add(i * 3);
					tris.Add(i * 3 + 1);
					tris.Add(i * 3 + 2);

					// 
					tris.Add(i * 3);

					int dd = i * 3 + 2 + 3;
					if (i == d)
					{
						dd = i * 3 + 1;
					}
					tris.Add(dd);
					tris.Add(i * 3 + 1);

				}

				Mesh mesh3 = new Mesh();
				mesh3.vertices = vertices.ToArray();
				mesh3.triangles = tris.ToArray();
				//mesh3.uv = uvs.ToArray();

				var mesh = MeshUtils.Merge(mesh1, mesh2, mesh3);

				mesh.RecalculateNormals();
				return mesh;
			}
			else
			{
				Mesh mesh1 = CreateCircolarDualMesh(ray_outher, ray_inner, depth_outher, depth_inner / 2, denti, false);
				Mesh mesh2 = CreateCircolarDualMesh(ray_outher, ray_inner, depth_outher, -depth_inner / 2, denti, true);

				var mesh = MeshUtils.Merge(mesh1, mesh2);

				mesh.RecalculateNormals();
				return mesh;
			}

		}

	}
}