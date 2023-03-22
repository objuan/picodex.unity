using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Text;


namespace picodex
{
    public class TestTurning : MonoBehaviour
    {
		private void Update()
		{
			transform.Rotate(0, 1, 0, Space.Self);
		}
	}

}