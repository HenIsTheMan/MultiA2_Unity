﻿using UnityEngine;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;

public class UDPSendNew : MonoBehaviour
{

	// prefs
	public string IP = "127.0.0.1";  // define in init
    public int port;  // define in init
    public Text textInput;

    // "connection" things
    IPEndPoint remoteEndPoint;
    UdpClient client;

    void OnDisable()
    {
        client.Close();
    }
    public void Start()
    {
        init();
    }
   
    public void OnSendBtnDown()
    {
        sendString(textInput.text);
    }

    // OnGUI

    // init
    public void init()
    {
     

      
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();
        client.EnableBroadcast = true;

        sendString(-1 + " /NewClientJoined " + "Hey");

        _ = StartCoroutine(nameof(Test));

        try
        {
            client.BeginReceive(new AsyncCallback(Receive), null);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        // status
    }

    private System.Collections.IEnumerator Test() {
        yield return new WaitForSeconds(2);

        sendString(1 + " / Hello World i am ??");
    }

    // sendData
    private void sendString(string message)
    {
        try
        {
               
            byte[] data = Encoding.UTF8.GetBytes(message);

      
            client.Send(data, data.Length, remoteEndPoint);
          
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

    private void Receive(IAsyncResult res)
    {
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, port);
        byte[] received = client.EndReceive(res, ref RemoteIpEndPoint);

        //Process codes

        Debug.Log(Encoding.UTF8.GetString(received));
        client.BeginReceive(new AsyncCallback(Receive), null);
    }

    // endless test
    private void sendEndless(string testStr)
    {
        do
        {
            sendString(testStr);


        }
        while (true);

    }

}
