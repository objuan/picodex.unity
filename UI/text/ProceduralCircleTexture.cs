
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ProceduralCircleTexture : ProceduralTexture
{
    // IN
    public string text = "Prova";
    public float ray=60;
    public float angleStep = 40;
    public float fontScale = 0.01f;
    public string cachePath;

    public GameObject textProceduralPrefab;
    GameObject root;


    protected new void Awake()
    {
        base.Awake();
        if (!camObj.transform.Find("Root"))
        {
            root = new GameObject("Root");
            root.SetParentAtOrigin(camObj.gameObject);
        }
        else
            root = camObj.transform.Find("Root").gameObject;

       // Build();
    }

    Vector3 GetCircle(Vector3 center, float radius, float ang)
    {
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.z = center.z;
        return pos;

    }

    
    public void Build()
    {
        Initialize();
     
        Vector3 center = camObj.transform.position + new Vector3(0, 0, 1);
        List<GameObject> prefabs = new List<GameObject>();

        root.transform.Clear();
        float ang = 0;
        for(int i=0;i<text.Length;i++)
        {
            Vector3 pos = GetCircle(center, ray, ang);
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, (pos - center ).normalized);
            ang += angleStep;
            var t = Instantiate(textProceduralPrefab, pos, rot, root.transform);
         //   t.layer = (1 << LayerMask.NameToLayer("RenderText"));
            t.transform.localScale = new Vector3(fontScale, fontScale, fontScale);
            t.layer = LayerMask.NameToLayer("RenderText");
            prefabs.Add(t);
            char c = text[i];
            prefabs[i].GetComponentInChildren<Text>().text = c.ToString();
        }

        camera = camGen.GetComponent<Camera>();
        camGen.TakeTexture(this);
    }

    protected override void OnProcessEnd()
    {
        // save
        //string tumbName = "curve_text_" + text.Replace(" ", "_") + "_" + ray.ToString("#.#") + "_" + width + "_" + height;
        string fullPath = cachePath;// Application.dataPath+"/Resources/BrickGame/Textures/Sprites/" + tumbName + ".png";
        Debug.Log("SAVE " + fullPath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(fullPath, bytes);
       
    }
}
