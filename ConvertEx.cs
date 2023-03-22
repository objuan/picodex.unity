
using UnityEngine;


public class ConvertEx
{
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

    public static Vector3 CylinderToCartesian(float radius, float polar, float elevation)
    {
        return new Vector3(radius * Mathf.Cos(polar), elevation, radius * Mathf.Sin(polar));
    }

    public static void CartesianToCylinder(Vector3 cartCoords, out float outRadius, out float outPolar, out float outElevation)
    {
        outRadius = Mathf.Sqrt(cartCoords.x * cartCoords.x + cartCoords.z * cartCoords.z);
        outElevation = cartCoords.y;
        outPolar = 0;
        if (cartCoords.x == 0 && cartCoords.z == 0)
        {
            outPolar = 0;
        }
        else if (cartCoords.x >= 0)
        {
            outPolar = Mathf.Asin(cartCoords.z / outRadius);
        }
        else if (cartCoords.x < 0)
        {
            outPolar = -Mathf.Asin(cartCoords.z / outRadius) + Mathf.PI;
        }
    }
}


