
 using UnityEngine;

using System.Reflection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Holds extension methods for a Unity <see cref="GameObject"/>.
/// </summary>
public static class GameObjectExtensions
{
    static List<MonoBehaviour> monoList = new List<MonoBehaviour>();

    public static T GetAdd<T>(this GameObject go) where T : Component
    {
        var g = go.GetComponent<T>();
        if (g == null) g = go.AddComponent<T>();
        return g;
    }
    public static void Remove<T>(this GameObject go) where T : Component
    {
        var g = go.GetComponent<T>();
        if (g != null) GameObject.Destroy(g);
    }
    public static void RemoveImmediate<T>(this GameObject go) where T : Component
    {
        var g = go.GetComponent<T>();
        if (g != null) GameObject.DestroyImmediate(g);
    }
    public static bool Contains<T>(this GameObject go) where T : Component
    {
       return  go.GetComponent<T>() != null;
    }

    public static bool Contains<T>(this MonoBehaviour go) where T : Component
    {
        return go.GetComponent<T>() != null;
    }

    public static void BroadcastMessageExt(this GameObject targetObj, string methodName, object value = null, SendMessageOptions options = SendMessageOptions.RequireReceiver)
    {
        targetObj.GetComponentsInChildren<MonoBehaviour>(true, monoList);
        for (int i = 0; i < monoList.Count; i++)
        {
            try
            {
                Type type = monoList[i].GetType();

                MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance |
                                                BindingFlags.NonPublic |
                                                 BindingFlags.Public |
                                                 BindingFlags.Static);

                method.Invoke(monoList[i], new object[] { value });
            }
            catch (Exception e)
            {
                //Re-create the Error thrown by the original SendMessage function
                if (options == SendMessageOptions.RequireReceiver)
                    Debug.LogError("SendMessage " + methodName + " has no receiver!");

                //Debug.LogError(e.Message);
            }
        }
    }

    public static void SetParentAtOrigin(this GameObject gameObject, GameObject parent)
    {
        gameObject.transform.parent = parent.transform;
        gameObject.transform.ZeroLocal();
    }


    /// <summary>
    /// Adds a <see cref="MeshFilter"/> to the <see cref="GameObject"/> and assigns it the given <see cref="Mesh"/>.
    /// </summary>
    /// <param name="gameObject">The <see cref="GameObject"/> to add the <see cref="MeshFilter"/> to.</param>
    /// <param name="mesh">The <see cref="Mesh"/> to assign.</param>
    /// <returns>The newly created and added <see cref="MeshFilter"/>.</returns>
    public static MeshFilter AddMeshFilter(this GameObject gameObject, Mesh mesh)
    {
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        return meshFilter;
    }

    /// <summary>
    /// Adds a <see cref="MeshRenderer"/> to the <see cref="GameObject"/> and disables shadows on it.
    /// </summary>
    /// <param name="gameObject">The <see cref="GameObject"/> to add the <see cref="MeshRenderer"/> to.</param>
    /// <returns>The newly created and added <see cref="MeshRenderer"/>.</returns>
    public static MeshRenderer AddMeshRenderer(this GameObject gameObject)
    {
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        meshRenderer.receiveShadows = false;

        return meshRenderer;
    }

    /// <summary>
    /// Adds a <see cref="MeshRenderer"/> to the <see cref="GameObject"/>, disables shadows on it, and assigns it the given <see cref="Material"/>.
    /// </summary>
    /// <param name="gameObject">The <see cref="GameObject"/> to add the <see cref="MeshRenderer"/> to.</param>
    /// <param name="material">The <see cref="Material"/> to assign.</param>
    /// <returns>The newly created and added <see cref="MeshRenderer"/>.</returns>
    public static MeshRenderer AddMeshRenderer(this GameObject gameObject, Material material)
    {
        MeshRenderer meshRenderer = gameObject.AddMeshRenderer();
        meshRenderer.sharedMaterial = material;

        return meshRenderer;
    }

    /// <summary>
    /// Adds a <see cref="MeshRenderer"/> to the <see cref="GameObject"/>, disables shadows on it, assigns it the given <see cref="Material"/>, adds 
    /// a <see cref="MeshFilter"/> (if there isn't one), and assigns it the given <see cref="Mesh"/>.
    /// </summary>
    /// <param name="gameObject">The <see cref="GameObject"/> to add the <see cref="MeshRenderer"/> to.</param>
    /// <param name="material">The <see cref="Material"/> to assign.</param>
    /// <param name="mesh">The <see cref="Mesh"/> to assign.</param>
    /// <returns>The newly created and added <see cref="MeshRenderer"/>.</returns>
    public static MeshRenderer AddMeshRenderer(this GameObject gameObject, Material material, Mesh mesh)
    {
        // add a MeshFilter automatically, if there isn't already one
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            gameObject.AddMeshFilter(mesh);
        }
        else
        {
            meshFilter.sharedMesh = mesh;
        }

        return gameObject.AddMeshRenderer(material);
    }

    /// <summary>
    /// Creates a <see cref="GameObject"/> that has a box <see cref="Mesh"/>, a <see cref="MeshRenderer"/>.
    /// </summary>
    /// <param name="gameObject">The <see cref="GameObject"/> to add the <see cref="MeshRenderer"/> to.</param>
    /// <param name="radius">Radius of the circle. Value should be greater than or equal to 0.0f.</param>
    /// <param name="segments">The number of segments making up the circle. Value should be greater than or equal to 3.</param>
    /// <param name="startAngle">The starting angle of the circle.  Usually 0.</param>
    /// <param name="angularSize">The angular size of the circle.  2 pi is a full circle. Pi is a half circle.</param>
    public static void CreateCircle(this GameObject gameObject, float radius, int segments, float startAngle,
        float angularSize)
    {
        Mesh mesh = new Mesh();
        mesh.CreateCircle(radius, segments, startAngle, angularSize);

        //gameObject.name = "Circle";

        Shader shader = Shader.Find("Diffuse");
        gameObject.AddMeshRenderer(new Material(shader), mesh);
    }

    public static void CreatePlane(this GameObject gameObject, float width, float height, int widthSegments,
        int heightSegments)
    {
        Mesh mesh = new Mesh();
        mesh.CreatePlane(width, height, widthSegments, heightSegments);

        //gameObject.name = "Plane";

        Shader shader = Shader.Find("Diffuse");
        gameObject.AddMeshRenderer(new Material(shader), mesh);
    }

    public static void CreateBox(this GameObject gameObject, float width, float height, float depth,
        int widthSegments, int heightSegments, int depthSegments)
    {
        Mesh mesh = new Mesh();
        mesh.CreateBox(width, height, depth, widthSegments, heightSegments, depthSegments);

        //gameObject.name = "Box";

        Shader shader = Shader.Find("Diffuse");
        gameObject.AddMeshRenderer(new Material(shader), mesh);
    }

    public static  Bounds getRenderBounds(this GameObject objeto)
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        Renderer render = objeto.GetComponent<Renderer>();
        if (render != null)
        {
            return render.bounds;
        }
        return bounds;
    }

    public static Bounds GetBounds(this GameObject objeto)
    {
        Bounds bounds;
        Renderer childRender;
        bounds = getRenderBounds(objeto);
        if (bounds.extents.x == 0)
        {
            bounds = new Bounds(objeto.transform.position, Vector3.zero);
            foreach (Transform child in objeto.transform)
            {
                childRender = child.GetComponent<Renderer>();
                if (childRender)
                {
                    bounds.Encapsulate(childRender.bounds);
                }
                else
                {
                    bounds.Encapsulate(GetBounds(child.gameObject));
                }
            }
        }
        return bounds;
    }

    /// <summary>
    /// Gets the components only in immediate children of parent.
    /// </summary>
    /// <returns>The components only in children.</returns>
    /// <param name="script">MonoBehaviour Script, e.g. "this".</param>
    /// <param name="isRecursive">If set to <c>true</c> recursive search of children is performed.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public static T[] GetComponentsInParentRec<T>(this GameObject go) where T : class
    {
      
         var v = go.GetComponentsInParent<T>().ToList();

         if (go.transform.parent!=null)
            v.AddRange(go.transform.parent.gameObject.GetComponentsInParentRec<T>());

        return v.ToArray();
    }
    public static GameObject FindChilds(this GameObject parent, string name)
    {

        var resultTransform = parent.transform.Find(name);

        // Search through each of parent's children.
        if (resultTransform != null)
        {
            return resultTransform.gameObject;
        }

        // Perform the search recursively for each child of parent.
        foreach (Transform childTransform in parent.transform)
        {
            var result = FindChilds(childTransform.gameObject, name);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

}
