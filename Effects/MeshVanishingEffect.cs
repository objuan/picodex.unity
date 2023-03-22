using System;
using UnityEngine;
using UnityEngine.UI;

using System.Collections;


public class MeshVanishingEffect : MonoBehaviour
{
    public float time = 1;

    IEnumerator Start()
    {
        float t = Time.time;
        var meshes = GetComponentsInChildren<MeshFilter>();
        /*
        while (Time.time - t < time)
        {
            float factor = 1f-  0.5f *((Time.time - t) / time);
            foreach(var mesh in meshes)
                mesh.transform.localScale = new Vector3(factor, factor, factor);
            yield return null;
        }
        */
        yield return new WaitForSeconds(time);
     
        Destroy(gameObject);
    }
}


