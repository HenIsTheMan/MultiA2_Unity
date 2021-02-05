using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceive : MonoBehaviour
{

    // receiving Thread
    Thread receiveThread;

    // udpclient object
    UdpClient client;

    // public
    public string IP = "127.0.0.1";
    public int port; // define > init
    public Text text;

    // infos
    public string lastReceivedUDPPacket = "";
    public string allReceivedUDPPackets = ""; // clean up this from time to time!

    void OnDisable()
    {
        if (receiveThread != null)
            receiveThread.Abort();

        client.Close();
    }
    public void OnServerPressed()
    {
        init();
    }
 

    // OnGUI
    void OnGUI()
    {
        text.text = "# UDPReceive\n127.0.0.1 " + port + " #\n"
                    + "\nLast Packet: \n" + lastReceivedUDPPacket
                    + "\n\nAll Messages: \n" + allReceivedUDPPackets;
    }

    // init
    private void init()
    {
        print("UDPSend.init()");

        // define port
        port = 8051;

        // status
        print("Sending to 127.0.0.1 : " + port);
        print("Test-Sending to this Port: nc -u 127.0.0.1  " + port + "");


        receiveThread = new Thread(
            new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

    }

    // receive thread
    private void ReceiveData()
    {
        Debug.Log("New Client");
            client = new UdpClient(port);
        

        while (true)
        {

            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                int rcvport = ((IPEndPoint)client.Client.LocalEndPoint).Port;
                string rcvIp = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();

                Debug.Log(rcvIp + "    " + rcvport);

                string text = Encoding.UTF8.GetString(data);

              
                print(">> " + text);

                // latest UDPpacket
                lastReceivedUDPPacket = text;

                // ....
                allReceivedUDPPackets = allReceivedUDPPackets + text;

            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    // getLatestUDPPacket
    // cleans up the rest
    public string getLatestUDPPacket()
    {
        allReceivedUDPPackets = "";
        return lastReceivedUDPPacket;
    }
}
