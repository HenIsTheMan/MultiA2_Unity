using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using System.Threading;

public class TCPSend : MonoBehaviour
{
    private static int localPort;

    // prefs
    public string IP = "127.0.0.1";  // define in init
    public int port;  // define in init
    public Text textInput;
    public Text textOutput;

    // "connection" things
    TcpClient client;
    // Start is called before the first frame update

    void OnDisable()
    {
        client.GetStream().Close();
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

        // define
        //port = 7890;

        client = new TcpClient(IP, port);
        sendString("Hello World!");     
    }

    public void Clicked()
    {
        sendString(textInput.text);
    }


    private void sendString(string message)
    {
        NetworkStream stream = client.GetStream();

        if (stream.CanWrite)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);

            stream.Write(data, 0, data.Length);
        }
      
    }

    void Update()
    {
        if(client.GetStream().DataAvailable)
        {
            byte[] bytes = new byte[client.ReceiveBufferSize];

            // Read can return anything from 0 to numBytesToRead.
            // This method blocks until at least one byte is read.
            client.GetStream().Read(bytes, 0, (int)client.ReceiveBufferSize);

            string returndata = Encoding.UTF8.GetString(bytes);
            textOutput.text = returndata;
        }
    }

}
