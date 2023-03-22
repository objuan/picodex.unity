using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Text;
using System.IO;

public class TCPClient : MonoBehaviour
{
	#region private members 	
	private TcpClient socketConnection;
	private Thread clientReceiveThread;

	public string serverIP = "";
	public int serverPort = 6666;

	//public event TCPMessageHandler OnReceive;
	
	#endregion

	[HideInInspector]
	public bool connected = false;
	bool ancora;
	public int attempt_lev1 = 0;
	public int attempt_lev2 = 0;
	public string loadAddress;
	Queue<byte[]> messageList = new Queue<byte[]>();

	// Use this for initialization 	
	protected void Start()
	{
		ancora = true;
		ConnectToTcpServer();

		StartCoroutine("SendMessages");

		loadAddress =  LocalIPAddress();
		Debug.Log("loadAddress:" + loadAddress);

	}
	public static string LocalIPAddress()
	{
		Debug.Log("host:"+Dns.GetHostName());

		IPHostEntry host;
		string localIP = "0.0.0.0";
		host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (IPAddress ip in host.AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				localIP = ip.ToString();
				//if (localIP.StartsWith("192"))
				//	break;
			}
		}
		return localIP;
	}
	IEnumerator SendMessages()
	{
		while (true)
		{
			lock (messageList)
			{
				while (messageList.Count > 0)
				{
					var clientMessage = messageList.Dequeue();
					OnReceive(clientMessage);
					//Debug.Log("(server)<< " + clientMessage);
				}
			}
			yield return new WaitForSeconds(0.01f);
		}
	}

	protected virtual void OnReceive(byte[] msg)
	{
	}

	private void OnEnable()
    {
      
	}

    private void OnDisable()
    {
		try
		{
			ancora = false;
			socketConnection.Close();
		}
		catch (Exception)
		{
		}
	}

	protected virtual void OnConnected()
	{
	}

    // Update is called once per frame
    void Update()
	{
	
	}

	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	
	private void ConnectToTcpServer()
	{
		try
		{
			clientReceiveThread = new Thread(new ThreadStart(ListenForData));
			clientReceiveThread.IsBackground = true;
			clientReceiveThread.Start();
		}
		catch (Exception e)
		{
			Debug.Log("On client connect exception " + e);
		}
	}
	/// <summary> 	
	/// Runs in background clientReceiveThread; Listens for incomming data. 	
	/// </summary>     
	private void ListenForData()
	{
		Byte[] bytes = new Byte[2048*10];
		while (ancora)
		{
			try
			{
				attempt_lev1++;
				Debug.Log("Client connecting " + serverIP+" :" + serverPort);
				//socketConnection = new TcpClient( serverIP, serverPort);
				socketConnection = new TcpClient();// serverIP, serverPort);
				socketConnection.NoDelay = true;
				var result = socketConnection.BeginConnect(serverIP, serverPort, null, null);
				var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));

				if (!success)
				{
				//	throw new Exception("Failed to connect.");
				}
				
				while (true)
				{
					Debug.Log("Client try ");
					byte[] incommingData=null;
					int incomingIndex = 0;
					connected = true;
					attempt_lev2++;
					OnConnected();
					// Get a stream object for reading 				
					using (NetworkStream stream = socketConnection.GetStream())
					{
						stream.WriteTimeout = 5000; //  <------- 1 second timeout
						stream.ReadTimeout = 5000; //  <------- 1 second timeout
						int length = 0;
						// Read incomming stream into byte arrary. 					
						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
						{
							//Debug.Log(">> " + length);
							if (bytes[0] == TCPServer.jolly[0] && bytes[1] == TCPServer.jolly[1])
							{
								int len = BitConverter.ToInt32(bytes, 2);
							//	Debug.Log("receive " + len);

								incommingData = new byte[len];

								if (len > length-6)
								{
									incomingIndex = length - 6;

									Array.Copy(bytes, 6, incommingData, 0, length - 6);
								}
								else
								{
									// tronco
									Array.Copy(bytes, 6, incommingData, 0, len);

								//	Debug.Log("done1 ");

									lock (messageList)
									{
										messageList.Enqueue(incommingData);
									}
									incommingData = null;
								}

							}
							else if (incommingData != null)
							{
								int max = Math.Min(incommingData.Length - incomingIndex, length);
							//	Debug.Log("fill " + incomingIndex + " => " + max + "(" + (incomingIndex + length) + ")");

								Array.Copy(bytes, 0, incommingData, incomingIndex, max);
								incomingIndex += max;

								//Debug.Log("DD " + incomingIndex);

								if (incomingIndex == incommingData.Length)
								{
								//	Debug.Log("done ");

									lock (messageList)
									{
										messageList.Enqueue(incommingData);
									}
									incommingData = null;
								}
							}
							/*	//receiveArray.a
								var incommingData = new byte[length];
								Array.Copy(bytes, 0, incommingData, 0, length);
								// Convert byte array to string message. 						
								//string serverMessage = Encoding.ASCII.GetString(incommingData);
								//	Debug.Log("(client) <<  " + serverMessage);

								lock (messageList)
								{
									messageList.Enqueue(incommingData);
								}
							*/
						}
					}
				}
			}
			catch (Exception socketException)
			{
				Debug.Log("Socket exception: " + socketException);
				connected = false;
				try
				{
					socketConnection.Close();
				}
				catch (Exception)
				{
				}
			}

			Thread.Sleep(100);
		}
	}
	/// <summary> 	
	/// Send message to server using socket connection. 	
	/// </summary> 	
	public void SendNetworkMessage(string message)
	{
		if (!connected)
			return;

		if (socketConnection == null)
		{
			return;
		}
		try
		{
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite)
			{
				
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(message);
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				//Debug.Log("Client sent his message - should be received by server");
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}
}