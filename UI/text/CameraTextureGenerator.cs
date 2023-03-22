
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTextureGenerator : MonoBehaviour
{
    List<ProceduralTexture> txtList = new List<ProceduralTexture>();

    Camera camera;

    private void Start()
    {
        camera = GetComponent<Camera>();

        StartCoroutine(Process());

    }

    public void TakeTexture(ProceduralTexture txtGenerator)
    {
        txtList.Add(txtGenerator);
    }


    WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

    public IEnumerator Process()
    {
        while (true)
        {
            yield return frameEnd;

            if (txtList.Count > 0)
            {
                var txt = txtList[0];

                //  Debug.Log("Build "+ txt);

                txtList.RemoveAt(0);

                txt.ProcessAtEndFrame();
                //camera.Render();
                //RenderTexture currentRT = RenderTexture.active;

                //RenderTexture.active = camera.targetTexture;
                //txt.textTexure.ReadPixels(new Rect(0, 0, txt.width, txt.height), 0, 0,false);
                //txt.textTexure.Apply();
                //RenderTexture.active = currentRT;

            }
        }
    }

}
