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
    Board board;

    Socket client;

    int portNum = 80;
    string chat = string.Empty;
    string sendMsg = string.Empty;    

    void Start()
    {
        board = GetComponent<Board>();
    }

    void Update()
    {
        if(client != null && client.Poll(0, SelectMode.SelectRead))
        {
            byte[] buffer = new byte[1024];
            if (client.Receive(buffer) > 0)
            {
                string str = Encoding.UTF8.GetString(buffer);
                string[] strs = str.Split(',');
                int protocolVal = 0;
                int.TryParse(strs[0], out protocolVal);
                chat += str + "\n";

                switch (protocolVal)
                {
                    case 1000:
                        {
                            int r, c, id;                            
                            int.TryParse(strs[1], out r);
                            int.TryParse(strs[2], out c);
                            int.TryParse(strs[3], out id);
                            board.SetPiece(r, c, id);
                            bool turn = (id==1) ? false : true;
                            board.SetTurn(turn);
                            break;
                        }
                    case 1001:
                        {
                            int r, c, id;
                            int.TryParse(strs[1], out r);
                            int.TryParse(strs[2], out c);
                            int.TryParse(strs[3], out id);
                            board.ChangePiece(r, c, id);
                            break;
                        }
                    case 1005:
                        chat += strs[1] + "\n";
                        break;
                    default:
                        break;
                }
            }
        }
    }

    // protocol : 1000
    public void SetPiece(int r, int c, int id)
    {
        byte[] buffer = new byte[1024];
        string str = string.Format("1000,{0},{1},{2}", r, c, id);
        buffer = Encoding.UTF8.GetBytes(str);
        client.Send(buffer);
    }

    public void ChangePiece(int r, int c, int id)
    {
        byte[] buffer = new byte[1024];
        string str = string.Format("1001,{0},{1},{2}", r, c, id);
        buffer = Encoding.UTF8.GetBytes(str);
        client.Send(buffer);
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
            sendMsg = "1005," + sendMsg;
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
