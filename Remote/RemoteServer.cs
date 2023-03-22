using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Text;
using System.IO;

public class RemoteServer : TCPServer
{
    public Camera remoteCamera;
    public MeshRenderer debugCameraMesh;

    public bool sendCamera = true;
    public float cameraScale = 1;

    public bool userDeviceResolution = true;

    public int deviceWidth;
    public int deviceHeight;

    public RenderTexture cameraBuffer;
    WaitForEndOfFrame frameEndWait = new WaitForEndOfFrame();
    byte[] imageBuffer;

    int cameraFPS;
    List<picodex.Input.TouchInfo> toRemove = new List<picodex.Input.TouchInfo>();

    bool sending = false;

   // Texture2D screen_source;
    Texture2D half_source;

    //protected new void OnEnable()
    //{
    //    base.OnEnable();
    //    Debug.Log("OnEnable");
    //}
    //protected new void OnDisable()
    //{

    //    base.OnDisable();
    //    Debug.Log("OnDisable");
    //}
    private void Awake()
	{
        picodex.Input.Clear();

    }
	private void Start()
    {
        StartCoroutine("GrabImage");

        cameraBuffer = RenderTexture.GetTemporary((int)(remoteCamera.pixelWidth * cameraScale), (int)(remoteCamera.pixelHeight * cameraScale), 24);
        
        remoteCamera.gameObject.AddComponent<GrabScreen>().cameraBuffer = cameraBuffer;

        half_source = new Texture2D(cameraBuffer.width, cameraBuffer.height, TextureFormat.RGB24, false);

    }

    IEnumerator GrabImage()
	{
		while (true)
		{
            yield return frameEndWait;

            if (sendCamera && !sending)
            {
               // yield return frameEndWait;

                RTImageBuffer(remoteCamera);

                sending = true;
                var task = SendNetworkMessageAsync(imageBuffer);
                task.ContinueWith((o) =>
                {
                    sending = false;
                });
              
            }
/*
            toRemove.Clear();
            // remove touch
            foreach (var t in picodex.Input.touchList)
            {
                if (t.toRemove)
                    toRemove.Add(t);
            }
            foreach (var t in toRemove)
            {
                picodex.Input.touchList.Remove(t);
                picodex.Input.OnTouchRemoved(t);
            }
*/
        }
	}

	private void Update()
	{
        picodex.Input.Update();

    }
    private void LateUpdate()
    {
        picodex.Input.LateUpdate();

       /* if (sendCamera && !sending)
        {

            if (debugCameraMesh != null)
            {
                Texture txt = RTImage(remoteCamera);
                debugCameraMesh.material.mainTexture = txt;
            }


            //yield return StartCoroutine(RTImageBuffer(remoteCamera));

           // byte[] buff = RTImageBuffer(remoteCamera);
            
           // sending = true;
           // var task = SendNetworkMessageAsync(imageBuffer);
           // task.ContinueWith((o) =>
           //{
           //    sending = false;
           //});
            
           
        }
       
        toRemove.Clear();
        // remove touch
        foreach (var t in picodex.Input.touchList)
		{
            if (t.toRemove && t.removeFrame < Time.frameCount)
			{
                toRemove.Add(t);

            }
		}
        foreach (var t in toRemove)
        {
            picodex.Input.touchList.Remove(t);
            picodex.Input.OnTouchRemoved(t);
        }
       */
    }

    // ON END FRAME
    protected override void OnReceive(string message)
    {

        RemoteMessage msg = RemoteMessage.Parse(message);

       //  Debug.Log(msg.Serialize());

        if (msg.messageType == RemoteMessageType.SCREEN)
        {
            deviceWidth = (int)msg.GetFloat(0);
            deviceHeight = (int)msg.GetFloat(1);

            if (userDeviceResolution)
            {
                Debug.Log("Resizing camera ");

                remoteCamera.pixelRect = new Rect(0, 0, deviceWidth, deviceHeight);

                Screen.SetResolution(deviceWidth, deviceHeight,false);
            }
        }
        else if (msg.messageType == RemoteMessageType.TOUCH)
        {
            Touch t = msg.GetTouch();

            // scale position
            if (!userDeviceResolution)
            {
                //  Vector2 scale = new Vector2((float)Screen.width/deviceWidth * cameraScale, (float)Screen.height/deviceHeight * cameraScale);
                Vector2 scale = new Vector2((float)Screen.width / deviceWidth * 1, (float)Screen.height / deviceHeight * 1);
                t.position = new Vector2(t.position.x * scale.x, t.position.y * scale.y);
                t.radius = t.radius * scale.x;
            }

            var find = picodex.Input.touchList.FirstOrDefault(X => X.touch.fingerId == t.fingerId);

            if (find == null)
            {
                picodex.Input.touchList.Add(new picodex.Input.TouchInfo() { touch = t, removeFrame = Time.frameCount });
            //    picodex.Input.OnTouchAdded(picodex.Input.touchList.Last());
            }
            else
            {
                find.touch = t;
             //   if (t.phase == TouchPhase.Ended)
            //        find.toRemove = true;
           //     else
             //       picodex.Input.OnTouchChanged(find);
            }
           
          //  if (t.phase != TouchPhase.Stationary)
           //     Debug.Log("Touch  " + t.fingerId + " " + t.type + " " + t.phase + " " + t.position);
            
        }
        else if (msg.messageType == RemoteMessageType.ACCELERATION)
        {

            picodex.Input.acceleration = msg.GetVector();


            //Debug.Log("Acc  " + picodex.Input.acceleration);
        }
    }

    // Take a "screenshot" of a camera's Render Texture.
    Texture RTImage_old(Camera camera)
    {
        if (cameraBuffer==null)
            cameraBuffer = RenderTexture.GetTemporary(remoteCamera.pixelWidth, remoteCamera.pixelHeight, 24);
        

        // The Render Texture in RenderTexture.active is the one
        // that will be read by ReadPixels.
        var currentRT = RenderTexture.active;
        RenderTexture.active = cameraBuffer;

        camera.targetTexture = cameraBuffer;

        // Render the camera's view.
        camera.Render();
        
        // Make a new texture and read the active Render Texture into it.
        Texture2D image = new Texture2D(cameraBuffer.width, cameraBuffer.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, cameraBuffer.width, cameraBuffer.height), 0, 0);
        image.Apply();

        //byte[] bytes = image.EncodeToPNG();
        //Destroy(image);
        camera.targetTexture = null;

        // Replace the original active Render Texture.
        RenderTexture.active = currentRT;
        return cameraBuffer;

    }



    void RTImageBuffer(Camera camera)
    {
        //if (screen_source == null)
        //{
        //    screen_source = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, false);

        //    cameraBuffer = RenderTexture.GetTemporary((int)(remoteCamera.pixelWidth * cameraScale), (int)(remoteCamera.pixelHeight * cameraScale), 24);

        //    GetComponent<GrabScreen>().cameraBuffer = cameraBuffer;

        //}

        //  screen_source.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        //  screen_source.Apply();

        var currentRT1 = RenderTexture.active;
        RenderTexture.active = cameraBuffer;

        //  Graphics.CopyTexture(screen_source, half_source);
        // Texture2D image1 = new Texture2D(cameraBuffer.width, cameraBuffer.height, TextureFormat.RGB24, false);
        half_source.ReadPixels(new Rect(0, 0, cameraBuffer.width, cameraBuffer.height), 0, 0);
        half_source.Apply();
      
        imageBuffer = half_source.EncodeToPNG();
        //File.WriteAllBytes(@"D:\lavoro\Image.png", imageBuffer);

        RenderTexture.active = currentRT1;

        return;

        // yield return frameEndWait;
        var width = Screen.width;
        var height = Screen.height;
        var tex = new Texture2D((int)(remoteCamera.pixelWidth * cameraScale), (int)(remoteCamera.pixelHeight * cameraScale), TextureFormat.RGB24, false);

        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        imageBuffer = tex.EncodeToPNG();
        Destroy(tex);
        return;

        if (cameraBuffer == null)
        {
            cameraBuffer = RenderTexture.GetTemporary((int)(remoteCamera.pixelWidth * cameraScale), (int)(remoteCamera.pixelHeight * cameraScale), 24);

            //var go = new GameObject("clone");
            //go.SetParentAtOrigin(camera.gameObject);
            ////remoteCamera = cameraScale
            //remoteCamera = go.AddComponent<Camera>();
            //remoteCamera.name = "clone";
            //remoteCamera.CopyFrom(camera);
            //remoteCamera.targetTexture = cameraBuffer;

           // GO.Instance<Canvas>().worldCamera = remoteCamera;

        }
        // The Render Texture in RenderTexture.active is the one
        // that will be read by ReadPixels.
        var currentRT = RenderTexture.active;
        RenderTexture.active = cameraBuffer;

        remoteCamera.targetTexture = cameraBuffer;

        // Render the camera's view.
        //GO.Instance<Canvas>().worldCamera = remoteCamera;
        remoteCamera.Render();
       // GO.Instance<Canvas>().worldCamera = camera;

        // Make a new texture and read the active Render Texture into it.
        Texture2D image = new Texture2D(cameraBuffer.width, cameraBuffer.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, cameraBuffer.width, cameraBuffer.height), 0, 0);
        image.Apply();

        //var data = image.GetRawTextureData<byte>();

        imageBuffer = image.EncodeToPNG();

        // File.WriteAllBytes(@"D:\lavoro\Image.png", imageBuffer);

        //imageBuffer = image.EncodeToJPG();

        remoteCamera.targetTexture = null;
        Destroy(image);

        // Replace the original active Render Texture.
        RenderTexture.active = currentRT;
     
    }
}