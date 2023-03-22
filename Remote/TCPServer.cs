using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

public delegate void TCPMessageHandler(object sender, string message);

public class TCPServer : MonoBehaviour
{
    #region private members 	
    /// <summary> 	
    /// TCPListener to listen for incomming TCP connection 	
    /// requests. 	
    /// </summary> 	
    private TcpListener tcpListener;
    /// <summary> 
    /// Background thread for TcpServer workload. 	
    /// </summary> 	
    private Thread tcpListenerThread;
    /// <summary> 	
    /// Create handle to connected tcp client. 	
    /// </summary> 	
    private TcpClient connectedTcpClient;
    #endregion

    public  string ip="";
    public  int port = 6666;
    bool ancora = true;

    public event TCPMessageHandler Receive;

    Queue<string> messageList = new Queue<string>();

    WaitForEndOfFrame frameEndWait = new WaitForEndOfFrame();

    IEnumerator SendMessages()
    {
        while (true)
        {
            yield return frameEndWait;
            picodex.Input.touchList.Clear();

            lock (messageList)
            {
                while (messageList.Count > 0)
                {
                    var clientMessage = messageList.Dequeue();
                    if (Receive != null)
                        Receive(this, clientMessage);
                    OnReceive(clientMessage);
                    //Debug.Log("(server)<< " + clientMessage);
                }
            }
            //yield return new WaitForSeconds(0.01f);
        }
    }

    protected void OnEnable()
    {
        if (ip == "")
        {
            ip = GetLocalIPAddress();
        }
        ancora = true;
        // Start TcpServer background thread 		
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();

        StartCoroutine("SendMessages");
    }

    protected void OnDisable()
    {
        try
        {
            Debug.Log("Server stop listening");
            tcpListener.Stop();
            ancora = false;
            tcpListenerThread.Abort();
        }
        catch (Exception socketException)
        {
            Debug.Log("SocketException " + socketException.ToString());
        }
    }

    protected virtual void OnReceive(string msg)
    {
    }


    /// <summary> 	
    /// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
    /// </summary> 	
    private void ListenForIncommingRequests()
    {
        try
        {
            // Create listener on localhost port 8052. 			
            tcpListener = new TcpListener(IPAddress.Parse(ip), port);
            tcpListener.Start();
            Debug.Log("Server is listening at "+ IPAddress.Parse(ip) + ":"+ port);
            Byte[] bytes = new Byte[1024];
            while (true)
            {
                try
                {
                    using (connectedTcpClient = tcpListener.AcceptTcpClient())
                    {
                        Debug.Log("Server connect to client " + connectedTcpClient.Client.RemoteEndPoint.ToString());
                        // Get a stream object for reading 					
                        using (NetworkStream stream = connectedTcpClient.GetStream())
                        {
                            Debug.Log("Server stream to client " + connectedTcpClient.Client.RemoteEndPoint.ToString());

                            int length;
                            // Read incomming stream into byte arrary. 						
                            while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                            {
                                var incommingData = new byte[length];
                                Array.Copy(bytes, 0, incommingData, 0, length);
                                // Convert byte array to string message. 							
                                string clientMessage = Encoding.ASCII.GetString(incommingData);

                                lock (messageList)
                                {
                                    messageList.Enqueue(clientMessage);
                                }
                            }
                        }
                    }
                }
                catch (Exception socketException)
                {
                    Debug.Log("SERVER SocketException " + socketException.ToString());
                }
            }
        }
        catch (Exception socketException)
        {
            Debug.Log("SERVER Exception " + socketException.ToString());
        }
    }

    /// <summary> 	
	/// Send message to client using socket connection. 	
	/// </summary> 	
	public void SendNetworkMessage(string msg)
    {
        if (connectedTcpClient == null)
        {
            return;
        }

        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = connectedTcpClient.GetStream();
            if (stream.CanWrite)
            {
               // string serverMessage = "This is a message from your server.";
                // Convert string message to byte array.                 
                byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(msg);
                // Write byte array to socketConnection stream.               
                stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                //Debug.Log("Server sent his message - should be received by client");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    public static byte[] jolly = new byte[] { 3,4 };

    public Task SendNetworkMessageAsync(byte[] serverMessageAsByteArray)
    {
        if (connectedTcpClient == null)
        {
            return Task.Run(() => { });
        }

        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = connectedTcpClient.GetStream();
            if (stream.CanWrite)
            {
              //  Debug.Log("sended " + serverMessageAsByteArray.Length);

                // Write byte array to socketConnection stream.
                //stream.Write(jolly,0, jolly.Length);
                byte[] snd = BitConverter.GetBytes((int)serverMessageAsByteArray.Length);
                byte[] bb = new byte[serverMessageAsByteArray.Length + 6];

                bb[0] = jolly[0]; bb[1] = jolly[1]; bb[2] = snd[0]; bb[3] = snd[1]; bb[4] = snd[2]; bb[5] = snd[3];


                Array.Copy(serverMessageAsByteArray, 0, bb, 6, serverMessageAsByteArray.Length);

               // return Task.Run(() => { });
                return stream.WriteAsync(bb, 0, bb.Length);
                //Debug.Log("Server sent his message - should be received by client");
            }
            else
                return Task.Run(() => { });
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
            return Task.Run(() => { });
        }
    }
    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
}