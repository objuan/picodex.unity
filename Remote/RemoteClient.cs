using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Text;


public class RemoteClient : TCPClient
{
    public int sensorsFrequencyFPS = 30;

    public Texture2D cameraImage;
    public MeshRenderer debugCameraMesh;

    private new void Start()
    {
        base.Start();
        StartCoroutine("SensorUpdate");
    }

    protected override void OnConnected()
    {
        SendNetworkMessage(new RemoteMessage(RemoteMessageType.SCREEN, Screen.width, Screen.height).Serialize());

        //image.ReadPixels(new Rect(0, 0, cameraBuffer.width, cameraBuffer.height), 0, 0);
        //image.Apply();
    }

    private void Update()
    {
        for (int i = 0; i < Input.touchCount; ++i)
        {
            var t = Input.GetTouch(i);

            SendNetworkMessage(new RemoteMessage(RemoteMessageType.TOUCH, t).Serialize());


        }
    }

    protected override void OnReceive(byte[] msg)
    {
        if (cameraImage == null)
        {
            cameraImage = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            //debugCameraMesh.material.mainTexture = cameraImage;
        }

       // Debug.Log("received " + msg.Length);
        if (cameraImage != null)
        {
            cameraImage.LoadImage(msg);
           // debugCameraMesh.material.mainTexture = cameraImage;
        }
    }

    IEnumerator SensorUpdate()
    {
        //Input.acceleration
        while (true)
        {
            try
            {
                if (connected)
                {
                   
                    //  Debug.Log(Input.acceleration);

                    SendNetworkMessage(new RemoteMessage(RemoteMessageType.ACCELERATION, Input.acceleration).Serialize());

                    SendNetworkMessage(new RemoteMessage(RemoteMessageType.ATTITUDE, Input.gyro.attitude).Serialize());
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            yield return new WaitForSeconds(1.0f / sensorsFrequencyFPS);

        }
    }

    void OnGUI()
    {
        // Compute a fontSize based on the size of the screen width.
        //  GUI.skin.label.fontSize = (int)(Screen.width / 25.0f);
        if (cameraImage!=null)
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), cameraImage, ScaleMode.ScaleToFit, false,(float)Screen.width / Screen.height);

        GUI.Label(new Rect(0, 0, 300, 40), loadAddress);

        GUI.Label(new Rect(100,0,300,40), connected ? "Connected" :"Disconnected" +" sc: "+Screen.width+"x"+Screen.height);

        GUI.Label(new Rect(280, 0, 300, 40), serverIP + " :" + serverPort+" #"+ attempt_lev1+"/"+ attempt_lev2);
    }
}