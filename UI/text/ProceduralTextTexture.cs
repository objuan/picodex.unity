
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshRenderer))]
public class ProceduralTextTexture : ProceduralTexture
{
    // IN
    public string text = "Prova";
 
    GameObject textProceduralPrefab;
    Text camTxt;

    protected new void Awake()
    {

        base.Awake();

		var t = Instantiate(textProceduralPrefab, camObj.transform.position + new Vector3(0, 0, 100), Quaternion.identity, camObj.transform);
		t.layer = LayerMask.NameToLayer("RenderText");
		t.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("RenderText");
		t.transform.GetChild(0).GetChild(0).gameObject.layer = LayerMask.NameToLayer("RenderText");
		camTxt = t.GetComponentInChildren<Text>();
		var canvas = t.GetComponentInChildren<Canvas>();
		((RectTransform)canvas.transform).sizeDelta = new Vector2(cam_w, cam_h);
		((RectTransform)camTxt.transform).sizeDelta = new Vector2(cam_w, cam_h);
		//camTxt.fontSize = 0.1f;

		camera = camGen.GetComponent<Camera>();

        GetComponent<MeshRenderer>().material.mainTexture = texture;
    }

    protected override void OnProcessAtEndFrame()
    {

        camTxt.text = text;

       
    }
}
