using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class newServer : MonoBehaviour
{
    Socket server;
    List<Socket> clients = new List<Socket>();

    int portNum = 80;

    void Start()
    {
        CreateServer();
    }

    void Update()
    {
        if(server.Poll(0, SelectMode.SelectRead))
        {
            Socket client = server.Accept();
            clients.Add(client);
            Debug.Log("A Client is connected.");
        }

        for(int i=0; i<clients.Count; i++)
        {
            if (clients[i].Poll(0, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[1024];
                try
                {
                    int recvLen = clients[i].Receive(buffer);
                    if (recvLen > 0)
                    {
                        for (int j = 0; j < clients.Count; j++)
                            clients[j].Send(buffer);
                    }
                    else
                    {
                        clients[i] = null;
                        clients.Remove(clients[i]);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    clients[i] = null;
                    clients.Remove(clients[i]);
                    Debug.Log(ex);
                }
            }
        }
    }

    void CreateServer()
    {
        try
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Any, portNum));
            server.Listen(1);
        }
        catch(Exception ex)
        {
            Debug.Log(ex);
        }
    }

    private void OnApplicationQuit()
    {
        for(int i=0; i<clients.Count; i++)
        {
            clients[i].Shutdown(SocketShutdown.Both);
            clients[i].Close();
            clients[i] = null;
            clients.Remove(clients[i]);
        }
        if (server != null)
        {
            server.Shutdown(SocketShutdown.Both);
            server.Close();
            server = null;
        }
    }
}
