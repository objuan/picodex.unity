using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Text;

using Input = picodex.Input;

namespace picodex
{
    public class TestTouch : MonoBehaviour
    {
		private Material lineMaterial;
		void Awake()
		{
			lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
				"SubShader { Pass {" +
				"   BindChannels { Bind \"Color\",color }" +
				"   Blend SrcAlpha OneMinusSrcAlpha" +
				"   ZWrite Off Cull Off Fog { Mode Off }" +
				"} } }");
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		}

		private void Update()
		{
			for(int i=0;i< Input.touchCount;i++)
			{
				var touch = Input.GetTouch(i);
			}
		
		}

		private void OnGUI()
		{
		
		}


		void OnPostRender()
		{
			for (int i = 0; i < Input.touchCount; i++)
			{
				var touch = Input.GetTouch(i);

				DrarwCircle(touch.position, touch.radius * 20);
			}

		}

		void DrarwCircle(Vector2 center,float ray)
		{ 
			Camera camera = Camera.main;
			const float DEG2RAD = 3.14159f / 180;
			float radius = ray;
			GL.Begin(GL.LINES);
			lineMaterial.SetPass(0);
			GL.Color(Color.red);
			float x = center.x;
			float y = center.y;
			for (int i = 0; i < 360; i++)
			{
				float degInRad = i * DEG2RAD;
				GL.Vertex(camera.ScreenToWorldPoint(new Vector3(x, y, camera.nearClipPlane)));
				GL.Vertex(camera.ScreenToWorldPoint(new Vector3(x + Mathf.Cos(degInRad) * radius, y + Mathf.Sin(degInRad) * radius, camera.nearClipPlane + 0.00001f)));
			}
			GL.End();
		}
	}

}