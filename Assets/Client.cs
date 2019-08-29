using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

// 프로토콜 : 규칙, 규약
// 정해진 규칙에 맞도록 제공하는 클래스 : Socket
// tcp소켓에서 중요한 함수 : Bind, Listen, Poll

/* TCP 클라이언트 동작 순서
 * 1. 소켓 생성
 * 2. 서버 연결
 */

public class Client : MonoBehaviour
{
    Socket client;
    string sendMsg = string.Empty;
    string recvStr = string.Empty;

    List<string> chatStr = new List<string>();
    Vector2 scrollPos = Vector2.zero;

    private void Start()
    {
        chatStr.Add("안녕");
        chatStr.Add("안녕하");
        chatStr.Add("안녕하세");
        chatStr.Add("안녕하세요");
        chatStr.Add("안녕");
        chatStr.Add("안녕하");
        chatStr.Add("안녕하세");
        chatStr.Add("안녕하세요");
        chatStr.Add("안녕");
        chatStr.Add("안녕하");
        chatStr.Add("안녕하세");
        chatStr.Add("안녕하세요");
        chatStr.Add("안녕");
        chatStr.Add("안녕하");
        chatStr.Add("안녕하세");
        chatStr.Add("안녕하세요");
    }

    private void Update()
    {
        if (client != null && client.Poll(0, SelectMode.SelectRead))
        {
            byte[] buffer = new byte[1024];
            int recvLen = client.Receive(buffer);
            if (recvLen > 0)
            {
                string receiveData = System.Text.Encoding.UTF8.GetString(buffer);
                string[] column = receiveData.Split(',');
                int protocolVal;
                int.TryParse(column[0], out protocolVal);

                switch (protocolVal)
                {
                    case 1000:
                        chatStr.Add("got 1000");
                        break;
                    case 1001:
                        chatStr.Add("got 1001");
                        break;
                }
                chatStr.Add(System.Text.Encoding.UTF8.GetString(buffer));
            }
        }
    }

    private void OnGUI()
    {
        #region 채팅 시스템
        Vector2 stringSize = GUI.skin.textArea.CalcSize(new GUIContent("안녕"));

        int height = 250;
        int totalHeight = (int)stringSize.y * chatStr.Count;
        int area = 0;
        if (totalHeight >= height)
        {
            area = totalHeight - height;
        }

        scrollPos = GUI.BeginScrollView(new Rect(50, 600, 400, height), scrollPos, new Rect(50, 500, 380, height + area));

        float contentsHeight = 0;
        for (int i = 0; i < chatStr.Count; i++)
        {
            stringSize = GUI.skin.textArea.CalcSize(new GUIContent(chatStr[i]));

            GUI.TextArea(new Rect(50, contentsHeight + 500, stringSize.x, stringSize.y), chatStr[i]);
            contentsHeight += stringSize.y;
        }

        GUI.EndScrollView();
        #endregion

        if (GUI.Button(new Rect(350, 870, 100, 30), "Connect"))
        {
            Connect("127.0.0.1", 80);
        }

        sendMsg = GUI.TextField(new Rect(50, 870, 300, 100), sendMsg);

        if (GUI.Button(new Rect(350, 900, 100, 70), "Send"))
        {
            if (!sendMsg.Equals(string.Empty))
            {
                byte[] msg = System.Text.Encoding.UTF8.GetBytes(sendMsg); // Unicode -> UTF8 -> Byte
                client.Send(msg);
                sendMsg = string.Empty;
            }
        }
    }

    bool Connect(string ipaddress, int port)
    {
        try
        {
            // 소켓 생성
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // 서버 연결
            // ※ EndPoint 클래스는 ip address와 port를 관리하는 클래스
            client.Connect(ipaddress, port);

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
            client = null;
        }
        return false;
    }

    private void OnApplicationQuit()
    {
        if (client != null)
        {
            client.Shutdown(SocketShutdown.Both); // 소켓을 종료. 양쪽 다
            client.Close(); // 연결을 닫음
            client = null;
        }
    }
}
