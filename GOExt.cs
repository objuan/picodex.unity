using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//namespace Assets.Scripts.Utils

public static class GO
{
    public static T Instance<T>() where T : UnityEngine.Object
    {
        return GameObject.FindObjectOfType<T>();
    }

    public static GameObject New(GameObject parent,string name="default")
    {
        GameObject g = new GameObject();
        g.name = name;
        g.SetParentAtOrigin(parent);
        return g;
    }
    public static T New<T>(GameObject parent, string name = "default") where T : Component
    {
        GameObject g = new GameObject();
        g.name = name;
        g.SetParentAtOrigin(parent);
        var t = g.AddComponent<T>();
        return t;
    }
    //public static T Instance<T>(this GameObject go) where T : UnityEngine.Object
    //{
    //    return GameObject.FindObjectOfType<T>();
    //}

}

