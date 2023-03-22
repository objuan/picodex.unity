using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class MeshUtils
{
    /// <summary>
    /// Returns a mesh with reserved triangles to turn back the face culling.
    /// This is usefull when a mesh needs to have a negative scale.
    /// </summary>
    /// <param name="mesh"></param>
    /// <returns></returns>
    public static int[] GetReversedTriangles(Mesh mesh)
    {
        var res = mesh.triangles.ToArray();
        var triangleCount = res.Length / 3;
        for (var i = 0; i < triangleCount; i++)
        {
            var tmp = res[i * 3];
            res[i * 3] = res[i * 3 + 1];
            res[i * 3 + 1] = tmp;
        }
        return res;
    }

    /// <summary>
    /// Returns a mesh similar to the given source plus given optionnal parameters.
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="source"></param>
    /// <param name="triangles"></param>
    /// <param name="vertices"></param>
    /// <param name="normals"></param>
    /// <param name="uv"></param>
    /// <param name="uv2"></param>
    /// <param name="uv3"></param>
    /// <param name="uv4"></param>
    /// <param name="uv5"></param>
    /// <param name="uv6"></param>
    /// <param name="uv7"></param>
    /// <param name="uv8"></param>
    public static void Update(Mesh mesh,
        Mesh source,
        IEnumerable<int> triangles = null,
        IEnumerable<Vector3> vertices = null,
        IEnumerable<Vector3> normals = null,
        IEnumerable<Vector2> uv = null,
        IEnumerable<Vector2> uv2 = null,
        IEnumerable<Vector2> uv3 = null,
        IEnumerable<Vector2> uv4 = null,
        IEnumerable<Vector2> uv5 = null,
        IEnumerable<Vector2> uv6 = null,
        IEnumerable<Vector2> uv7 = null,
        IEnumerable<Vector2> uv8 = null)
    {
        mesh.hideFlags = source.hideFlags;
#if UNITY_2017_3_OR_NEWER
        mesh.indexFormat = source.indexFormat;
#endif

        mesh.triangles = new int[0];
        mesh.vertices = vertices == null ? source.vertices : vertices.ToArray();
        mesh.normals = normals == null ? source.normals : normals.ToArray();
        mesh.uv = uv == null ? source.uv : uv.ToArray();
        mesh.uv2 = uv2 == null ? source.uv2 : uv2.ToArray();
        mesh.uv3 = uv3 == null ? source.uv3 : uv3.ToArray();
        mesh.uv4 = uv4 == null ? source.uv4 : uv4.ToArray();
#if UNITY_2018_2_OR_NEWER
        mesh.uv5 = uv5 == null ? source.uv5 : uv5.ToArray();
        mesh.uv6 = uv6 == null ? source.uv6 : uv6.ToArray();
        mesh.uv7 = uv7 == null ? source.uv7 : uv7.ToArray();
        mesh.uv8 = uv8 == null ? source.uv8 : uv8.ToArray();
#endif
        mesh.triangles = triangles == null ? source.triangles : triangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
    }

    public static void MoveMesh(Mesh mesh, Vector3 offset)
    {
        var v = mesh.vertices;
        for (int i = 0; i < v.Length; i++)
        {
            v[i] = v[i] + offset;
        }
        mesh.vertices = v;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
     
    public static Mesh Merge(GameObject meshesRoot)
    {
        var meshFilters = meshesRoot.GetComponentsInChildren<MeshFilter>();
        var combines = new CombineInstance[meshFilters.Length];
      //  var materialList = new List<Material>();
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combines[i].mesh = meshFilters[i].sharedMesh;
            combines[i].transform = Matrix4x4.TRS(meshFilters[i].transform.position - meshesRoot.transform.position,
                meshFilters[i].transform.rotation, meshFilters[i].transform.lossyScale);

            //var materials = meshFilters[i].GetComponent<MeshRenderer>().sharedMaterials;
            //foreach (var material in materials)
            //{
            //    materialList.Add(material);
            //}
        }
        var newMesh = new Mesh();
        newMesh.CombineMeshes(combines, true);

        return newMesh;


    }

    public static Mesh Merge(params Mesh[] meshes)
    {
        var combines = new CombineInstance[meshes.Length];
        //  var materialList = new List<Material>();
        for (int i = 0; i < meshes.Length; i++)
        {
            combines[i].mesh = meshes[i];
            combines[i].transform = Matrix4x4.identity;
        }
        var newMesh = new Mesh();
        newMesh.CombineMeshes(combines, true);

        return newMesh;


    }

    public static Mesh Rotate(Mesh mesh, Vector3 rot)
    {
        Quaternion q = Quaternion.Euler(rot);

        var v = mesh.vertices;
        for (int i = 0; i < v.Length; i++)
        {
            v[i] = q * v[i];
        }
        mesh.vertices = v;
        mesh.RecalculateNormals();
        //mesh.RecalculateBounds();
        // mesh.RecalculateTangents();
        return mesh;
    }

    public static void SetPivot(MeshFilter meshFilter, Vector3 pivot)
    {
        //Vector3 diff = Vector3.Scale(mesh.bounds.extents, last_p - p); //Calculate difference in 3d position
        //obj.transform.position -= Vector3.Scale(diff, obj.transform.localScale); //Move object position
        //                                                                         //Iterate over all vertices and move them in the opposite direction of the object position movement

        Vector3 diff = -pivot;

      //  meshFilter.transform.position = pivot;

        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] verts = mesh.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] += diff;
        }
        mesh.vertices = verts; //Assign the vertex array back to the mesh
        mesh.RecalculateBounds(); //Recalculate bounds of the mesh, for the renderer's sake
                                  //The 'center' parameter of certain colliders needs to be adjusted
                                  //when the transform position is modified

        var col = meshFilter.GetComponent<Collider>();
        if (col)
        {
            if (col is BoxCollider)
            {
                ((BoxCollider)col).center += diff;
            }
            else if (col is CapsuleCollider)
            {
                ((CapsuleCollider)col).center += diff;
            }
            else if (col is SphereCollider)
            {
                ((SphereCollider)col).center += diff;
            }
        }
    }
}

 
