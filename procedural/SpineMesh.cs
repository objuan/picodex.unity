using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ProceduralPrimitives
{
	public static partial class Primitive
	{
		/// <summary>
		/// X oriented
		/// </summary>
		/// <param name="len"></param>
		/// <param name="ray"></param>
		/// <returns></returns>
		public static Mesh CreateSpine(float _len, float ray)
		{
			float len = _len - ray * 2;

			Mesh tube = PrimitiveManager.CreateCylinderMesh(ray, len,10,2);
			Mesh point = PrimitiveManager.CreateConeMesh(10, ray, ray*2);

			var combines = new CombineInstance[2];
			combines[0].mesh = tube;
			combines[0].transform = Matrix4x4.TRS(new Vector3(len / 2, 0,0), Quaternion.Euler(0,0,90),Vector3.one);
		
			combines[1].mesh = point;
			combines[1].transform =   Matrix4x4.TRS(new Vector3(len  , 0, 0), Quaternion.Euler(0, 0, -90), Vector3.one);

			var newMesh = new Mesh();
			newMesh.CombineMeshes(combines, true);

			return newMesh;

		}

		public static Mesh CreatePugno(float _len, float ray)
		{
			float len = _len - ray*2;
			Mesh tube = PrimitiveManager.CreateCylinderMesh(ray, len, 10, 2);
			Mesh point = PrimitiveManager.CreateSphereMesh( ray*2, 8,8);

			var combines = new CombineInstance[2];
			combines[0].mesh = tube;
			combines[0].transform = Matrix4x4.TRS(new Vector3(len / 2, 0, 0), Quaternion.Euler(0, 0, 90), Vector3.one);

			combines[1].mesh = point;
			combines[1].transform = Matrix4x4.TRS(new Vector3(len, 0, 0), Quaternion.identity, Vector3.one);

			var newMesh = new Mesh();
			newMesh.CombineMeshes(combines, true);

			return newMesh;

		}
	}
}

