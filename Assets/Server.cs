using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server : MonoBehaviour
{
    Socket server;
    //Socket client; // 클라이언트 소켓 복사본
    List<Socket> clientList = new List<Socket>();

    //int uniqueCount = 1;

    void Start()
    {
        CreateServer();
    }

    void Update()
    {
        // 소켓의 상태 확인
        // 대기 시간, 통신모드 (SelectRead: 읽어들일 데이터가 있는지)
        if (server.Poll(0, SelectMode.SelectRead))
        {
            Socket client = server.Accept();
            //string sendStr = string.Format("{0},{1}", 1000, uniqueCount); // ex) sendStr = "1000(protocol),1"
            //byte[] sendData = System.Text.Encoding.UTF8.GetBytes(sendStr);
            //client.Send(sendData);
            clientList.Add(client);
        }

        for (int i = 0; i < clientList.Count; i++)
        {
            if (clientList[i].Poll(0, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[1024];

                try
                {
                    int recvLength = clientList[i].Receive(buffer); // 사이즈 값을 int 타입으로 리턴
                    if (recvLength == 0) // 클라이언트가 종료된 경우
                    {
                        clientList[i] = null;
                        clientList.Remove(clientList[i]);
                        continue;
                    }
                    else
                    {
                        for (int j = 0; j < clientList.Count; j++)
                            clientList[j].Send(buffer); // 받은 buffer를 전체 client에 보내겠다
                    }
                }
                catch (Exception ex)
                {
                    clientList[i] = null;
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
            server.Bind(new IPEndPoint(IPAddress.Any, 80));
            server.Listen(1); // 접속 가능 대수를 int 타입으로 받음
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    private void OnApplicationQuit()
    {
        for (int i = 0; i < clientList.Count; i++)
        {
            if (clientList[i] != null)
            {
                clientList[i].Shutdown(SocketShutdown.Both);
                clientList[i].Close();
                clientList.Remove(clientList[i]);
                clientList[i] = null;
            }
        }
        if (server != null)
        {
            server.Shutdown(SocketShutdown.Both);
            server.Close();
            server = null;
        }
    }
}
