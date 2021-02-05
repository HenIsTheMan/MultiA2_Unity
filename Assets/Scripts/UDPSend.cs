using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using System.Threading;

public class UDPSend : MonoBehaviour
{
    private static int localPort;

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
    public void OnClientPressed()
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
      
        // define
        port = 8051;

      
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();
        client.EnableBroadcast = true;

        // status
      
    }

  

    // sendData
    private void sendString(string message)
    {
        try
        {
            //if (message != "")
            //{

         
            byte[] data = Encoding.UTF8.GetBytes(message);

      
            client.Send(data, data.Length, remoteEndPoint);
            //}
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
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
