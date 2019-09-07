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
    int uniqueID = -1;
    public int UniqueID
    {
        get { return uniqueID; }
    }

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
                string command = Encoding.UTF8.GetString(buffer);
                string[] str = command.Split('/');
                for (int i = 0; i < str.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(str[i]))
                        return;

                    string[] strs = str[i].Split(',');
                    int protocolVal = 0;
                    int.TryParse(strs[0], out protocolVal);
                    chat += str[i] + "\n";

                    switch (protocolVal)
                    {
                        case 1000: // 돌 놓기
                            {
                                int r, c, id;
                                int.TryParse(strs[1], out r);
                                int.TryParse(strs[2], out c);
                                int.TryParse(strs[3], out id);
                                board.SetPiece(r, c, id);
                                bool turn = (id == 1) ? false : true;
                                board.Turn = turn;
                                break;
                            }
                        case 1001: // 돌 바꾸기
                            {
                                int r, c, id;
                                int.TryParse(strs[1], out r);
                                int.TryParse(strs[2], out c);
                                int.TryParse(strs[3], out id);
                                board.ChangePiece(r, c, id);
                                break;
                            }
                        case 1002: // 실시간 점수 계산
                            {
                                int b, w;
                                int.TryParse(strs[1], out b);
                                int.TryParse(strs[2], out w);
                                board.SetScore(b, w);
                                board.CanSetPiece();
                                break;
                            }
                        case 1005: // 채팅
                            chat += strs[1] + "\n";
                            break;
                        case 1010: // 연결 번호 부여
                            int uniq;
                            int.TryParse(strs[1], out uniq);
                            uniqueID = uniq;
                            break;
                        case 1011: // 게임 시작
                            board.SetReady(true);
                            board.CanSetPiece();
                            break;
                        default:
                            board.CheckScore();
                            break;
                    }
                }
            }
        }
    }

    // protocol : 1000
    public void SetPiece(int r, int c, int id)
    {
        byte[] buffer = new byte[1024];
        string str = string.Format("1000,{0},{1},{2}/", r, c, id);
        buffer = Encoding.UTF8.GetBytes(str);
        client.Send(buffer);
    }

    // protocol : 1001
    public void ChangePiece(int r, int c, int id)
    {
        byte[] buffer = new byte[1024];
        string str = string.Format("1001,{0},{1},{2}/", r, c, id);
        buffer = Encoding.UTF8.GetBytes(str);
        client.Send(buffer);
    }

    // protocol : 1002
    public void CheckScore(int b, int w)
    {
        byte[] buffer = new byte[1024];
        string str = string.Format("1002,{0},{1}", b, w);
        buffer = Encoding.UTF8.GetBytes(str);
        client.Send(buffer);
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 100), "Connect"))
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
