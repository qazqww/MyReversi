using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class newClient : MonoBehaviour
{
    Socket client;

    int portNum = 80;
    string chat = string.Empty;
    string sendMsg = string.Empty;    

    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            try
            {
                byte[] buffer = new byte[1024];
                buffer = Encoding.UTF8.GetBytes("abc");
                client.Send(buffer);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        if(client != null && client.Poll(0, SelectMode.SelectRead))
        {
            byte[] buffer = new byte[1024];
            if (client.Receive(buffer) > 0)
            {
                chat += Encoding.UTF8.GetString(buffer) + "\n";
            }
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0,0,100,100), "Connect"))
        {
            Connect("127.0.0.1", portNum);
        }

        GUI.TextArea(new Rect(0, 200, 300, 300), chat);
        sendMsg = GUI.TextArea(new Rect(0, 520, 190, 100), sendMsg);

        if(GUI.Button(new Rect(210, 520, 90, 100), "Send"))
        {
            byte[] buffer = new byte[1024];
            buffer = Encoding.UTF8.GetBytes(sendMsg);
            client.Send(buffer);
            sendMsg = string.Empty;
        }
    }

    void Connect(string ipaddress, int port)
    {
        try
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(ipaddress, port);
        }
        catch(Exception ex)
        {
            Debug.Log(ex);
            client = null;
        }
    }

    private void OnApplicationQuit()
    {
        if(client != null)
        {
            client.Shutdown(SocketShutdown.Both);
            client.Close();
            client = null;
        }
    }
}
