
using System;
using UnityEngine;

public class MyGL 
{

    static Material lineMaterial;

    static string lastPrimitive="";
    static Color lastColor = new Color(0, 0, 0, 0);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        lineMaterial = null;
        lastPrimitive = "";
        lastColor = new Color(0, 0, 0, 0);
    }

    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    public static void Begin(GameObject obj)
    {
        CreateLineMaterial();
        // Apply the line material
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        // Set transformation matrix for drawing to
        // match our transform
        GL.MultMatrix(obj.transform.localToWorldMatrix);

        lastPrimitive = "";
        lastColor =  new Color(0, 0, 0, 0);
        //Matrix4x4 localToWorld = obj.transform.localToWorldMatrix;

    }
    public static void Begin()
    {
        CreateLineMaterial();
        // Apply the line material
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.LoadIdentity();
        // Set transformation matrix for drawing to
        // match our transform
        //GL.MultMatrix(obj.transform.localToWorldMatrix);

        lastPrimitive = "";
        lastColor = new Color(0, 0, 0, 0);
        //Matrix4x4 localToWorld = obj.transform.localToWorldMatrix;

    }

    public static void Line(Vector3 a, Vector3 b, Color color) {
        if (lastPrimitive != "LINE")
        {
            lastPrimitive = "LINE";
            GL.Begin(GL.LINES);
        }
        else { 
        }
        if (!lastColor.Equals(color))
        {
            lastColor = color;
            GL.Color(color);
        }

        
        GL.Vertex(a);
        GL.Vertex(b);
    }

    public static void End()
    {
        GL.End();
        GL.PopMatrix();
    }

}
