using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//namespace Assets.Scripts.Utils


//public class GameCollider
//{
//    public static RaycastCommand(Ray ray, Bounds box)
//    {

//            float tmin, tmax, tymin, tymax, tzmin, tzmax;

//            tmin = (bounds[r.sign[0]].x - r.orig.x) * r.invdir.x;
//            tmax = (bounds[1 - r.sign[0]].x - r.orig.x) * r.invdir.x;
//            tymin = (bounds[r.sign[1]].y - r.orig.y) * r.invdir.y;
//            tymax = (bounds[1 - r.sign[1]].y - r.orig.y) * r.invdir.y;

//            if ((tmin > tymax) || (tymin > tmax))
//                return false;
//            if (tymin > tmin)
//                tmin = tymin;
//            if (tymax < tmax)
//                tmax = tymax;

//            tzmin = (bounds[r.sign[2]].z - r.orig.z) * r.invdir.z;
//            tzmax = (bounds[1 - r.sign[2]].z - r.orig.z) * r.invdir.z;

//            if ((tmin > tzmax) || (tzmin > tmax))
//                return false;
//            if (tzmin > tmin)
//                tmin = tzmin;
//            if (tzmax < tmax)
//                tmax = tzmax;

//            return true;
//        }
//    }
//}

/// <summary>
/// Given four points in 3D space, solves for a sphere such that all four points
/// lie on the sphere's surface.
/// </summary>
/// <remarks>
/// Translated from Javascript on http://www.convertalot.com/sphere_solver.html, originally
/// linked to by http://stackoverflow.com/questions/13600739/calculate-centre-of-sphere-whose-surface-contains-4-points-c.
/// </remarks>
public class CircumcentreSolver
{
    private const float ZERO = 0;
    private double m_X0, m_Y0, m_Z0;
    private double m_Radius;
    private double[,] P =
            {
                { ZERO, ZERO, ZERO },
                { ZERO, ZERO, ZERO },
                { ZERO, ZERO, ZERO },
                { ZERO, ZERO, ZERO }
            };

    /// <summary>
    /// The centre of the resulting sphere.
    /// </summary>
    public double[] Centre
    {
        get { return new double[] { this.m_X0, this.m_Y0, this.m_Z0 }; }
    }
    public Vector3 CentreP
    {
        get { return new Vector3((float)this.m_X0, (float)this.m_Y0, (float)this.m_Z0); }
    }
    /// <summary>
    /// The radius of the resulting sphere.
    /// </summary>
    public double Radius
    {
        get { return this.m_Radius; }
    }

    /// <summary>
    /// Whether the result was a valid sphere.
    /// </summary>
    public bool Valid
    {
        get { return this.m_Radius != 0; }
    }

    /// <summary>
    /// Computes the centre of a sphere such that all four specified points in
    /// 3D space lie on the sphere's surface.
    /// </summary>
    /// <param name="a">The first point (array of 3 doubles for X, Y, Z).</param>
    /// <param name="b">The second point (array of 3 doubles for X, Y, Z).</param>
    /// <param name="c">The third point (array of 3 doubles for X, Y, Z).</param>
    /// <param name="d">The fourth point (array of 3 doubles for X, Y, Z).</param>
    public CircumcentreSolver(double[] a, double[] b, double[] c, double[] d)
    {
        this.Compute(a, b, c, d);
    }
    public CircumcentreSolver(Vector3[] p)
    {
        this.Compute(new double[] { p[0].x, p[0].y, p[0].z }, new double[] { p[1].x, p[1].y, p[1].z }, new double[] { p[2].x, p[2].y, p[2].z }, new double[] { p[3].x, p[3].y, p[3].z });
    }
    /// <summary>
    /// Evaluate the determinant.
    /// </summary>
    private void Compute(double[] a, double[] b, double[] c, double[] d)
    {
        P[0, 0] = a[0];
        P[0, 1] = a[1];
        P[0, 2] = a[2];
        P[1, 0] = b[0];
        P[1, 1] = b[1];
        P[1, 2] = b[2];
        P[2, 0] = c[0];
        P[2, 1] = c[1];
        P[2, 2] = c[2];
        P[3, 0] = d[0];
        P[3, 1] = d[1];
        P[3, 2] = d[2];

        // Compute result sphere.
        this.Sphere();
    }

    private void Sphere()
    {
        double r, m11, m12, m13, m14, m15;
        double[,] a =
                {
                    { ZERO, ZERO, ZERO, ZERO },
                    { ZERO, ZERO, ZERO, ZERO },
                    { ZERO, ZERO, ZERO, ZERO },
                    { ZERO, ZERO, ZERO, ZERO }
                };

        // Find minor 1, 1.
        for (int i = 0; i < 4; i++)
        {
            a[i, 0] = P[i, 0];
            a[i, 1] = P[i, 1];
            a[i, 2] = P[i, 2];
            a[i, 3] = 1;
        }
        m11 = this.Determinant(a, 4);

        // Find minor 1, 2.
        for (int i = 0; i < 4; i++)
        {
            a[i, 0] = P[i, 0] * P[i, 0] + P[i, 1] * P[i, 1] + P[i, 2] * P[i, 2];
            a[i, 1] = P[i, 1];
            a[i, 2] = P[i, 2];
            a[i, 3] = 1;
        }
        m12 = this.Determinant(a, 4);

        // Find minor 1, 3.
        for (int i = 0; i < 4; i++)
        {
            a[i, 0] = P[i, 0] * P[i, 0] + P[i, 1] * P[i, 1] + P[i, 2] * P[i, 2];
            a[i, 1] = P[i, 0];
            a[i, 2] = P[i, 2];
            a[i, 3] = 1;
        }
        m13 = this.Determinant(a, 4);

        // Find minor 1, 4.
        for (int i = 0; i < 4; i++)
        {
            a[i, 0] = P[i, 0] * P[i, 0] + P[i, 1] * P[i, 1] + P[i, 2] * P[i, 2];
            a[i, 1] = P[i, 0];
            a[i, 2] = P[i, 1];
            a[i, 3] = 1;
        }
        m14 = this.Determinant(a, 4);

        // Find minor 1, 5.
        for (int i = 0; i < 4; i++)
        {
            a[i, 0] = P[i, 0] * P[i, 0] + P[i, 1] * P[i, 1] + P[i, 2] * P[i, 2];
            a[i, 1] = P[i, 0];
            a[i, 2] = P[i, 1];
            a[i, 3] = P[i, 2];
        }
        m15 = this.Determinant(a, 4);

        // Calculate result.
        if (m11 == 0)
        {
            this.m_X0 = 0;
            this.m_Y0 = 0;
            this.m_Z0 = 0;
            this.m_Radius = 0;
        }
        else
        {
            this.m_X0 = 0.5 * m12 / m11;
            this.m_Y0 = -0.5 * m13 / m11;
            this.m_Z0 = 0.5 * m14 / m11;
            this.m_Radius = System.Math.Sqrt(this.m_X0 * this.m_X0 + this.m_Y0 * this.m_Y0 + this.m_Z0 * this.m_Z0 - m15 / m11);
        }
    }

    /// <summary>
    /// Recursive definition of determinate using expansion by minors.
    /// </summary>
    private double Determinant(double[,] a, int n)
    {
        int i, j, j1, j2;
        double d = 0;
        double[,] m =
                {
                    { ZERO, ZERO, ZERO, ZERO },
                    { ZERO, ZERO, ZERO, ZERO },
                    { ZERO, ZERO, ZERO, ZERO },
                    { ZERO, ZERO, ZERO, ZERO }
                };

        if (n == 2)
        {
            // Terminate recursion.
            d = a[0, 0] * a[1, 1] - a[1, 0] * a[0, 1];
        }
        else
        {
            d = 0;
            for (j1 = 0; j1 < n; j1++) // Do each column.
            {
                for (i = 1; i < n; i++) // Create minor.
                {
                    j2 = 0;
                    for (j = 0; j < n; j++)
                    {
                        if (j == j1) continue;
                        m[i - 1, j2] = a[i, j];
                        j2++;
                    }
                }

                // Sum (+/-)cofactor * minor.
                d = d + System.Math.Pow(-1.0, j1) * a[0, j1] * this.Determinant(m, n - 1);
            }
        }

        return d;
    }

}

public class MathHelper
{
    public static void OrderMinMax(ref iVector3 min, ref iVector3 max)
    {
        var m = min.Min(max);
        var M = min.Max(max);
        min = m;
        max = M;
    }
    public static void OrderMinMax(ref Vector3 min, ref Vector3 max)
    {
        var m = min.Min(max);
        var M = min.Max(max);
        min = m;
        max = M;
    }
    public static Vector3 SphericalToCartesian(float radius, float polar, float elevation)
    {
        float a = radius * Mathf.Cos(elevation);
        Vector3 outCart = new Vector3();
        outCart.x = a * Mathf.Cos(polar);
        outCart.y = radius * Mathf.Sin(elevation);
        outCart.z = a * Mathf.Sin(polar);
        return outCart;
    }


    public static void CartesianToSpherical(Vector3 cartCoords, out float outRadius, out float outPolar, out float outElevation)
    {
        if (cartCoords.x == 0)
            cartCoords.x = Mathf.Epsilon;
        outRadius = Mathf.Sqrt((cartCoords.x * cartCoords.x)
                        + (cartCoords.y * cartCoords.y)
                        + (cartCoords.z * cartCoords.z));
        outPolar = Mathf.Atan(cartCoords.z / cartCoords.x);
        if (cartCoords.x < 0)
            outPolar += Mathf.PI;
        outElevation = Mathf.Asin(cartCoords.y / outRadius);
    }

    /// <summary>
    /// Intersects a line and a circle.
    /// </summary>
    /// <param name="location">the location of the circle</param>
    /// <param name="radius">the radius of the circle</param>
    /// <param name="lineFrom">the starting point of the line</param>
    /// <param name="lineTo">the ending point of the line</param>
    /// <returns>true if the line and circle intersect each other</returns>
    public static bool IntersectLineCircle(Vector2 location, float radius, Vector2 lineFrom, Vector2 lineTo,ref float dist)
    {
        float ab2, acab, h2;
        Vector2 ac = location - lineFrom;
        Vector2 ab = lineTo - lineFrom;
        ab2 = Vector2.Dot( ab,  ab);
        acab = Vector2.Dot( ac,  ab);
        float t = acab / ab2;

        if (t < 0)
            t = 0;
        else if (t > 1)
            t = 1;

        Vector2 h = ((ab * t) + lineFrom) - location;
        h2 = Vector2.Dot( h,  h);
        dist = h2;
        return (h2 <= (radius * radius));
    }

    //public static Vector3[] IntersectionPointAll(Vector3 p1, Vector3 p2, Vector3 center, float radius)
    //{
    //    if ((p1 - center).magnitude < radius)
    //    {

    //    }

    //}

    public static Vector2[] IntersectionPoint(Vector2 p1, Vector2 p2, Vector2 center, float radius)
    {
        Vector2 dp = new Vector2();
        Vector2[] sect;
        float a, b, c;
        float bb4ac;
        float mu1;
        float mu2;

        //  get the distance between X and Z on the segment
        dp.x = p2.x - p1.x;
        dp.y = p2.y - p1.y;
        //   I don't get the math here
        a = dp.x * dp.x + dp.y * dp.y;
        b = 2 * (dp.x * (p1.x - center.x) + dp.y * (p1.y- center.y));
        c = center.x * center.x + center.y * center.y;
        c += p1.x * p1.x + p1.y * p1.y;
        c -= 2 * (center.x * p1.x + center.y * p1.y);
        c -= radius * radius;
        bb4ac = b * b - 4 * a * c;
        if (Mathf.Abs(a) < float.Epsilon || bb4ac < 0)
        {
            //  line does not intersect
            return new Vector2[] { };
        }
        mu1 = (-b + Mathf.Sqrt(bb4ac)) / (2 * a);
        mu2 = (-b - Mathf.Sqrt(bb4ac)) / (2 * a);
        sect = new Vector2[2];
        sect[0] = new Vector2(p1.x + mu1 * (p2.x - p1.x),  p1.y + mu1 * (p2.y - p1.y));
        sect[1] = new Vector2(p1.x + mu2 * (p2.x - p1.x),  p1.y + mu2 * (p2.y - p1.y));

        return sect;
    }

}


