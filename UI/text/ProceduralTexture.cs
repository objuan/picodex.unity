
using System;
using UnityEngine;
using UnityEngine.UI;


public class ProceduralTexture : MonoBehaviour
{
    public int width = 256;
    public int height = 256;
    public bool showReferencePlane=false;
    public Texture background;

    public Texture2D texture;

    protected CameraTextureGenerator camGen;
    protected Camera camera;
    protected float cam_h,cam_w;
    protected GameObject camObj;
    GameObject plane;

    protected void Awake()
    {
        camGen = GameObject.FindObjectOfType<CameraTextureGenerator>();
        if (camGen == null)
        {
            float aspect = (float)width / height;
            cam_h = 1;
            cam_w = cam_h * aspect;
            if (aspect > 1)
            {
                cam_w = 1;
                cam_h = cam_w / aspect;
            }

            camObj = new GameObject("bufferCamera");
            camera = camObj.AddComponent<Camera>();
            camObj.AddComponent<CameraTextureGenerator>();
            camObj.transform.position = new Vector3(10, 0, 0); //TODO
            camera.orthographic = true;
            camera.orthographicSize = 0.5f;
            camera.rect = new Rect(0, 0, width, height);
            // camera.cullingMask = (1 << LayerMask.NameToLayer("RenderText"));
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0, 0, 0, 0);

            var txt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32); // !
            txt.filterMode = FilterMode.Bilinear;
            camera.targetTexture = txt;

            var canvas = camObj.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1;

            //texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        }
        camGen = GameObject.FindObjectOfType<CameraTextureGenerator>();
        camObj = camGen.gameObject;
        camera = camObj.GetComponent<Camera>();
        if (! texture)
            texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
    }

    public void Initialize()
    {
        // plane 

        if (showReferencePlane)
        {
            plane = PrimitiveManager.CreatePlaneObject(camObj, 1, 1);
            plane.transform.position = camObj.transform.position + new Vector3(0, 0, 2);
            plane.transform.rotation = Quaternion.Euler(-90, 0, 0);
            plane.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Unlit/Transparent"));
            plane.GetComponent<MeshRenderer>().material.mainTexture = background;
            
        }
    }

	//private void OnApplicationQuit()
	//{
 //       if (plane)
 //           Destroy(plane);

 //   }

	protected virtual void OnProcessAtEndFrame()
    {
    }
    protected virtual void OnProcessEnd()
    {
    }

    public void ProcessAtEndFrame()
    {
        OnProcessAtEndFrame();
        camera.Render();
        RenderTexture currentRT = RenderTexture.active;

        RenderTexture.active = camera.targetTexture;
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
        texture.Apply();
        RenderTexture.active = currentRT;

        OnProcessEnd();
    }
}
