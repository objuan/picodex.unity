using ProceduralPrimitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MeshCollapser : MonoBehaviour// TraumaInducer
{
    public Plane plane;
    public enum CollapseMode
    {
        Bottom,
        Center
    }
    // public Actor actor;
    public CollapseMode mode;

    public float speed = 1;
    public float noise = 1;
    public float collapseTime = 10;
    public float collapsePerc = 0.5f;

    Vector3[] source_vertices;
    Vector3[] source_vertices_w;
    Vector3[] vertices;
    Vector3[] target;

    float time;
    Mesh mesh;

    void Start()
    {
        // la mesh e' una sfera
        mesh = GetComponent<MeshFilter>().mesh;

        source_vertices = new Vector3[mesh.vertices.Length];
        vertices = new Vector3[source_vertices.Length];
        target = new Vector3[source_vertices.Length];

        mesh.vertices.CopyTo(source_vertices, 0);

        source_vertices_w = mesh.vertices.Select(X => transform.TransformPoint(X)).ToArray();
        //source_vertices_w = mesh.vertices.Select(X=>X).ToArray();

        if (mode == CollapseMode.Bottom)
            CollapseBottom();

        time = Time.time;
    }

    void CollapseBottom()
    {
        var trx = GetComponent<MeshFilter>().transform;
      //  var ray = GetComponent<Veicle>().radius;
        var pos = trx.position;

        for (int i = 0; i < source_vertices.Length; i++)
        {
            // target[i] = plane.ClosestPointOnPlane(source_vertices_w[i]) + pos.UpDirection * 0.05f;
            var prj = plane.ClosestPointOnPlane(source_vertices_w[i]);

            target[i] = prj + plane.normal * (source_vertices_w[i] - prj).magnitude * collapsePerc;// UnityEngine.Random.Range(0.05f, 0.1f);

            target[i] = transform.InverseTransformPoint(target[i]);
        }
    }

    private void Update()
    {
        if (Time.time - time > collapseTime)
        {
            mesh.vertices = source_vertices;
            mesh.RecalculateNormals();

            GameObject.Destroy(this);
        }
        else
        {
            for (int i = 0; i < source_vertices.Length; i++)
            {
                vertices[i] = Vector3.Lerp(source_vertices[i], target[i], (Time.time - time) / collapseTime);
            }
            mesh.vertices = vertices;
            // mesh.vertices = vertices.Select(X => transform.InverseTransformPoint(X)).ToArray();
            mesh.RecalculateNormals();
        }
    }

}

