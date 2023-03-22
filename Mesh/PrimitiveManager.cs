using UnityEngine;
using System.Collections;
using UnityEditor;

//#if !(UNITY_EDITOR)
public class GameObjectUtility
{
	public static void SetParentAndAlign(GameObject go, GameObject parent)
	{
		if (parent != null)
		{
			go.transform.parent = parent.transform;
			go.transform.localPosition = new Vector3(0, 0, 0);
			go.transform.localScale = new Vector3(1, 1, 1);
			go.transform.localRotation = Quaternion.identity;
		}
	}
}
//#endif

public class PrimitiveManager
{
#if UNITY_EDITOR
	#region MENU
	[MenuItem("Road/3D Object/Box", false, 10)]
	static void AddBox(MenuCommand menuCommand)
	{
		GameObject go = CreateBoxObject(menuCommand.context as GameObject, new Vector3(1, 1, 1));
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
	}
	[MenuItem("Road/3D Object/Cone", false, 10)]
	static void AddCone(MenuCommand menuCommand)
	{
		GameObject go = CreateConeObject(menuCommand.context as GameObject, 16, 1, 1);
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
	}
	[MenuItem("Road/3D Object/Cylinder", false, 10)]
	static void AddCylinder(MenuCommand menuCommand)
	{
		GameObject go = CreateCylinderObject(menuCommand.context as GameObject, 0.5f, 1, 16);
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
	}
	[MenuItem("Road/3D Object/Arrow", false, 10)]
	static void AddArrow(MenuCommand menuCommand)
	{
		GameObject go = CreateArrowObject(menuCommand.context as GameObject, 5);
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
	}

	[MenuItem("Road/3D Object/Cube Sphere", false, 10)]
	static void AddCubeSphere(MenuCommand menuCommand)
	{
		GameObject go = CreateCubeSphereObject(menuCommand.context as GameObject, 1, 10);
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
	}
	[MenuItem("Road/3D Object/Rouded Cube ", false, 10)]
	static void CreateRoundedCubeObject(MenuCommand menuCommand)
	{
		GameObject go = CreateRoundedCubeObject(menuCommand.context as GameObject, new Vector3(1, 1, 1), 0.1f);
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
	}

	[MenuItem("Road/3D Object/Saw ", false, 10)]
	static void CreateCircolarDualMesh(MenuCommand menuCommand)
	{
		GameObject go = CreateCircolarDualMesh(menuCommand.context as GameObject, 1,0.8f, 0,0.2f,0);
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
	}
#endregion


#else

#endif
		#region BASE

		public static GameObject CreateObject(string name, GameObject parent)
	{
		GameObject go = new GameObject(name);
		GameObjectUtility.SetParentAndAlign(go, parent);
		go.AddComponent<MeshFilter>();
		go.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
		go.GetComponent<MeshRenderer>().receiveShadows = false;
		go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		return go;
	}
	#endregion

	public static GameObject CreateAxisObject(GameObject parent, float length,float scale = 0.05f, float radius_tube = 0.5f, float radius_cap = 1, int subdivisions = 16)
	{
		var axis = new GameObject("axis");
		GameObjectUtility.SetParentAndAlign(axis, parent);
		//var axis = PrimitiveManager.CreateObject("axis", parent);

		var a = PrimitiveManager.CreateArrowObject(axis, length, radius_tube, radius_cap, subdivisions);
		a.transform.localScale = new Vector3(scale, scale, scale);
		a.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -90));
		//	a.transform.localPosition = new Vector3(0, 0.1f, 0);

		a.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("BrickGame/ColorNoLit"));
		a.GetComponent<MeshRenderer>().sharedMaterial.color = Color.red;
		a.GetComponent<MeshRenderer>().receiveShadows = false;
		a.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

		a = PrimitiveManager.CreateArrowObject(axis, length, radius_tube, radius_cap, subdivisions);
		a.transform.localScale = new Vector3(scale, scale, scale);
		a.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
		//	a.transform.localPosition = new Vector3(0, 0.1f, 0);

		a.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("BrickGame/ColorNoLit"));
		a.GetComponent<MeshRenderer>().sharedMaterial.color = Color.green;
		a.GetComponent<MeshRenderer>().receiveShadows = false;
		a.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

		a = PrimitiveManager.CreateArrowObject(axis, length, radius_tube, radius_cap, subdivisions);
		a.transform.localScale = new Vector3(scale, scale, scale);
		a.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
		//a.transform.localPosition = new Vector3(0, 0.1f, 0);

		a.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("BrickGame/ColorNoLit"));
		a.GetComponent<MeshRenderer>().sharedMaterial.color = Color.blue;
		a.GetComponent<MeshRenderer>().receiveShadows = false;
		a.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

		return axis;
	}

	/// <summary>
	///  ARROW
	/// </summary>
	/// <param name="length"></param>
	/// <param name="radius_tube"></param>
	/// <param name="radius_cap"></param>
	/// <param name="subdivisions"></param>
	/// <returns></returns>
	public static GameObject CreateArrowObject(GameObject parent, float length, float radius_tube = 0.5f, float radius_cap = 1, int subdivisions = 16)
	{
		GameObject go = new GameObject("arrow_tmp");

		GameObject cone = CreateConeObject(go, subdivisions, radius_cap, radius_cap);
		GameObject cylinder = CreateCylinderObject(go, radius_tube, length, subdivisions);

		GameObjectUtility.SetParentAndAlign(cone, go);
		GameObjectUtility.SetParentAndAlign(cylinder, go);

		cylinder.transform.localPosition = new Vector3(0, length / 2, 0);
		cone.transform.localPosition = new Vector3(0, length, 0);

		// merge

		GameObject arrow = CreateObject("Arrow", parent);
		arrow.GetComponent<MeshFilter>().mesh = MeshUtils.Merge(go);

		Object.DestroyImmediate(go);
		return arrow;
	}

	/// <summary>
	///  BOX
	/// </summary>
	/// <returns></returns>
	public static GameObject CreateBoxObject(GameObject parent, Vector3 size)
	{
		GameObject go = CreateObject("box", parent);
		go.GetComponent<MeshFilter>().sharedMesh = CreateBoxMesh(size);
		return go;
	}

	public static Mesh CreateBoxMesh(Vector3 size)
	{
		return ProceduralPrimitives.Primitive.CreateBoxMesh(size.x, size.y, size.z);
	}

	// plane

	public static GameObject CreatePlaneObjectNoCollider(GameObject parent, float width, float depth, int widthDivisions=2, int depthDivisions=2)
	{
		var go =  ProceduralPrimitives.Primitive.CreatePlaneGameObjectNoCollider(width, depth, widthDivisions, depthDivisions);
		go.SetParentAtOrigin(parent);
		return go;
		//GameObject go = CreateObject("box", parent);
		//go.GetComponent<MeshFilter>().sharedMesh = CreatePlaneMesh(size);
		//return go;
	}
	public static GameObject CreatePlaneObject(GameObject parent, float width, float depth, int widthDivisions = 2, int depthDivisions = 2)
	{
		var go = ProceduralPrimitives.Primitive.CreatePlaneGameObject(width, depth, widthDivisions, depthDivisions);
		go.SetParentAtOrigin(parent);
		return go;
		//GameObject go = CreateObject("box", parent);
		//go.GetComponent<MeshFilter>().sharedMesh = CreatePlaneMesh(size);
		//return go;
	}
	public static Mesh CreatePlaneMesh(float width, float depth, int widthDivisions=2, int depthDivisions=2)
	{
		return ProceduralPrimitives.Primitive.CreatePlaneMesh(width, depth, widthDivisions, depthDivisions);
	}

	/// <summary>
	/// cylinder
	/// </summary>
	/// <returns></returns>
	public static GameObject CreateSphereObject(GameObject parent, float radius = 1, int slices = 16, int stacks = 16)
	{
		GameObject go = CreateObject("sphere", parent);
		go.GetComponent<MeshFilter>().sharedMesh = CreateSphereMesh(radius, slices, stacks);
		return go;
	}

	public static Mesh CreateSphereMesh(float radius = 1, int slices = 16, int stacks = 16)
	{
		return ProceduralPrimitives.Primitive.CreateSphereMesh(radius, slices, stacks);
	}

	/// <summary>
	/// cylinder
	/// </summary>
	/// <returns></returns>
	public static GameObject CreateCylinderObject(GameObject parent, float radius = 1, float height = 1, int subdivisions = 8)
	{
		GameObject go = CreateObject("cylinder", parent);
		go.GetComponent<MeshFilter>().sharedMesh = ProceduralPrimitives.Primitive.CreateCylinderMesh(radius, radius, height, subdivisions, 4);
		return go;
	}

	public static Mesh CreateCylinderMesh(float radius = 1, float height = 1, int slices = 16, int stacks = 4)
	{
		return ProceduralPrimitives.Primitive.CreateCylinderMesh(radius, radius, height, slices, stacks);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="radius"></param>
	/// <param name="height"></param>
	/// <param name="slices"></param>
	/// <param name="stacks"></param>
	/// <returns></returns>
	public static Mesh CreateCircleMesh(float radius = 1, int slices = 16)
	{
		return ProceduralPrimitives.Primitive.PolyMesh(radius, slices);
	}
	public static GameObject CreateCircleObject(GameObject parent, float radius = 1, int slices = 16)
	{
		GameObject go = CreateObject("circle", parent);
		go.GetComponent<MeshFilter>().sharedMesh = ProceduralPrimitives.Primitive.PolyMesh(radius, slices);
		return go;
	}

	public static GameObject CreateCylinderSideObject(GameObject parent, float radius = 1, float height = 1, int subdivisions = 8)
	{
		GameObject go = CreateObject("cylinderSide", parent);
		go.GetComponent<MeshFilter>().sharedMesh = ProceduralPrimitives.Primitive.CreateCylinderSideMesh(radius, radius, height, subdivisions, 4);
		return go;
	}

	public static Mesh CreateCylinderSideMesh(float radius = 1, float height = 1, int slices = 16, int stacks = 4)
	{
		return ProceduralPrimitives.Primitive.CreateCylinderSideMesh(radius, radius, height, slices, stacks);
	}

	/// <summary>
	/// CONE
	/// </summary>
	/// <param name="parent"></param>
	/// <param name="subdivisions"></param>
	/// <param name="radius"></param>
	/// <param name="height"></param>
	/// <returns></returns>
	public static GameObject CreateConeObject(GameObject parent, int subdivisions = 8, float radius = 1, float height = 1)
	{
		GameObject go = CreateObject("cone", parent);
		go.GetComponent<MeshFilter>().sharedMesh = CreateConeMesh(subdivisions, radius, height);
		return go;
	}

	public static Mesh CreateConeMesh(int subdivisions, float radius, float height)
	{
		Mesh mesh = new Mesh();

		Vector3[] vertices = new Vector3[subdivisions + 2];
		Vector2[] uv = new Vector2[vertices.Length];
		int[] triangles = new int[(subdivisions * 2) * 3];

		vertices[0] = Vector3.zero;
		uv[0] = new Vector2(0.5f, 0f);
		for (int i = 0, n = subdivisions - 1; i < subdivisions; i++)
		{
			float ratio = (float)i / n;
			float r = ratio * (Mathf.PI * 2f);
			float x = Mathf.Cos(r) * radius;
			float z = Mathf.Sin(r) * radius;
			vertices[i + 1] = new Vector3(x, 0f, z);

			//Debug.Log(ratio);
			uv[i + 1] = new Vector2(ratio, 0f);
		}
		vertices[subdivisions + 1] = new Vector3(0f, height, 0f);
		uv[subdivisions + 1] = new Vector2(0.5f, 1f);

		// construct bottom

		for (int i = 0, n = subdivisions - 1; i < n; i++)
		{
			int offset = i * 3;
			triangles[offset] = 0;
			triangles[offset + 1] = i + 1;
			triangles[offset + 2] = i + 2;
		}

		// construct sides

		int bottomOffset = subdivisions * 3;
		for (int i = 0, n = subdivisions - 1; i < n; i++)
		{
			int offset = i * 3 + bottomOffset;
			triangles[offset] = i + 1;
			triangles[offset + 1] = subdivisions + 1;
			triangles[offset + 2] = i + 2;
		}

		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

		return mesh;
	}

	// ==============

	public static GameObject CreateCubeSphereObject(GameObject parent, float radius = 1, int gridSize = 10)
	{
		GameObject go = CreateObject("cubeSphere", parent);
		go.GetComponent<MeshFilter>().sharedMesh = ProceduralPrimitives.Primitive.CreateCubeSphereMesh(radius, gridSize);
		var mat = go.GetComponent<MeshRenderer>().sharedMaterial;
		go.GetComponent<MeshRenderer>().materials = new Material[3] { mat, mat, mat };
		go.AddComponent<SphereCollider>();
		return go;
	}

	public static GameObject CreateRoundedCubeObject(GameObject parent, Vector3 size, float roundness = 0.1f,Material mat=null)
	{
		GameObject go = CreateObject("roundedCube", parent);
		ProceduralPrimitives.Primitive.CreateRoundedCubeObject(go, size, roundness);
		if (mat==null)
			 mat = go.GetComponent<MeshRenderer>().sharedMaterial;
		go.GetComponent<MeshRenderer>().materials = new Material[3] { mat, mat, mat };
		//go.AddComponent<SphereCollider>();
		return go;

		//RoundedCube cube = go.AddComponent<RoundedCube>();
		//var mat = go.GetComponent<MeshRenderer>().sharedMaterial;
		//go.GetComponent<MeshRenderer>().materials = new Material[3] { mat, mat, mat };
		//cube.xSize = (int)size.x;
		//cube.ySize = (int)size.y;
		//cube.zSize = (int)size.z;
		//cube.roundness = roundness;
		//go.transform.localScale = new Vector3(1f / size.x, 1f / size.y, 1f / size.z);

		//var mat = go.GetComponent<MeshRenderer>().sharedMaterial;
		//	go.GetComponent<MeshRenderer>().materials = new Material[3] { mat, mat, mat };
		//	go.AddComponent<SphereCollider>();
		return go;
	}

	// ====================


	public static GameObject CreateCircolarDualMesh(GameObject parent, float ray_outher, float ray_inner, float depth_outher, float depth_inner, int denti = 0)
	{
		GameObject go = CreateObject("circolar mesh", parent);
		var mesh = ProceduralPrimitives.Primitive.CreateCircolarDualFilledMesh(ray_outher, ray_inner, depth_outher, depth_inner, denti);
		go.GetComponent<MeshFilter>().sharedMesh = mesh;
		var mat = go.GetComponent<MeshRenderer>().sharedMaterial;
		go.GetComponent<MeshRenderer>().materials = new Material[1] { mat };
		return go;
	}

	public static Mesh CreateViewConeMesh(float aAngle, float aDistance, int aConeResolution = 30)
	{
		Vector3[] verts = new Vector3[aConeResolution + 1];
		Vector3[] normals = new Vector3[verts.Length];
		int[] tris = new int[aConeResolution * 3];
		Vector3 a = Quaternion.Euler(-aAngle, 0, 0) * Vector3.forward * aDistance;
		Vector3 n = Quaternion.Euler(-aAngle, 0, 0) * Vector3.up;
		Quaternion step = Quaternion.Euler(0, 0, 360f / aConeResolution);
		verts[0] = Vector3.zero;
		normals[0] = Vector3.back;
		for (int i = 0; i < aConeResolution; i++)
		{
			normals[i + 1] = n;
			verts[i + 1] = a;
			a = step * a;
			n = step * n;
			tris[i * 3] = 0;
			tris[i * 3 + 1] = (i + 1) % aConeResolution + 1;
			tris[i * 3 + 2] = i + 1;
		}
		Mesh m = new Mesh();
		m.vertices = verts;
		m.normals = normals;
		m.triangles = tris;
		m.RecalculateBounds();
		return m;
	}
}