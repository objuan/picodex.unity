using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//namespace Assets.Scripts.Utils


public  class VectorLine
{
    class Accum
    {
        Vector3[] points = new Vector3[3];
        int c = 0;

        public Vector3 Point
        {
            get
            {
                Vector3 sum =Vector3.zero;
                foreach (var p in points)
                    sum += p;
                return sum / c;
            }
        }
        public void Add(Vector3 v)
        {
            points[c++] = v;
        }
    }

    public static void DrawLine(Vector3[] linePoints,Color color, float width,bool isClosed, Color? borderColor=null)
    {
        if (width == 1)
        {
            GL.Begin(GL.LINE_STRIP);
            GL.Color(color);

            for (int i = 0; i < linePoints.Length; i++)
            {
                var c = linePoints[i];
                GL.Vertex3(c.x, c.y, c.z);
            }
            GL.End();
        }
        else
        {
            Accum[] left = new Accum[linePoints.Length];
            Accum[] right = new Accum[linePoints.Length];
            for (int i = 0; i < linePoints.Length; i++)
            {
                left[i] = new Accum();
                right[i] = new Accum();
            }

            float w = (width * 0.5f);

            for (int i = 1; i < linePoints.Length; i++)
            {
                var v0 = linePoints[i - 1];
                var v1 = linePoints[i];
                var _perpendicular = Vector2.Perpendicular(new Vector2(v1.x - v0.x, v1.z - v0.z).normalized);
                var perpendicular = new Vector3(_perpendicular.x * w, 0, _perpendicular.y * w);

                left[i - 1].Add(v0 - perpendicular);
                left[i].Add(v1 - perpendicular);

                right[i - 1].Add(v0 + perpendicular);
                right[i].Add(v1 + perpendicular);
            }

            if (isClosed)
            {
                left[linePoints.Length - 1] = left[0];
                right[linePoints.Length - 1] = right[0];
            }

            GL.Begin(GL.TRIANGLE_STRIP);
            GL.Color(color);

            for (int i = 1; i < linePoints.Length; i++)
            {
                var v0 = left[i - 1].Point;
                var v1 = left[i].Point;
                var v2 = right[i - 1].Point;
                var v3 = right[i].Point;

                GL.Vertex3(v0.x, v0.y, v0.z);
                GL.Vertex3(v2.x, v2.y, v2.z);
                GL.Vertex3(v3.x, v3.y, v3.z);

                GL.Vertex3(v0.x, v0.y, v0.z);
                GL.Vertex3(v1.x, v1.y, v1.z);
                GL.Vertex3(v3.x, v3.y, v3.z);
            }
            GL.End();

            if (borderColor.HasValue)
            {
                GL.Begin(GL.LINE_STRIP);
                GL.Color(borderColor.Value);
                for (int i = 0; i < left.Length; i++)
                {
                    var c = left[i].Point;
                    GL.Vertex3(c.x, c.y, c.z);
                }
                GL.End();

                GL.Begin(GL.LINE_STRIP);
                GL.Color(borderColor.Value);
                for (int i = 0; i < right.Length; i++)
                {
                    var c = right[i].Point;
                    GL.Vertex3(c.x, c.y, c.z);
                }
                GL.End();
            }
        }
      
    }

}

