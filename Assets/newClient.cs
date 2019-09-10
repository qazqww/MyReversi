using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

enum ProtocolValue
{
    SetPiece = 1000,
    ChangePiece,
    CheckScore,
    ChangeTurn,
    GameSet,
    Chat,
    SetUniqueID = 1010,
    StartGame
}

public class newClient : MonoBehaviour
{
    Socket client;

    Board board;
    Vector2 scrollPos = Vector2.zero;

    int portNum = 80;    
    int connectTry = 1;
    float elapsedTime = 0;
    int uniqueID = -1;
    public int UniqueID
    {
        get { return uniqueID; }
    }

    List<string> chatStr = new List<string>();
    int chatCount = 0;
    string chat = string.Empty;
    string chatMsg = string.Empty;    

    void Start()
    {
        board = GetComponent<Board>();
        Connect("127.0.0.1", portNum);
    }

    void Update()
    {
        if (client == null)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > 3f)
            {
                connectTry++;
                Debug.Log(connectTry);
                elapsedTime = 0;
                Connect("127.0.0.1", portNum);
            }
        }

        else if (client.Poll(0, SelectMode.SelectRead))
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
                    int.TryParse(strs[0], out int protocolVal);

                    switch (protocolVal)
                    {
                        case (int)ProtocolValue.SetPiece:
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
                        case (int)ProtocolValue.ChangePiece:
                            {
                                int r, c, id;
                                int.TryParse(strs[1], out r);
                                int.TryParse(strs[2], out c);
                                int.TryParse(strs[3], out id);
                                board.ChangePiece(r, c, id);
                                break;
                            }
                        case (int)ProtocolValue.CheckScore:
                            {
                                int b, w;
                                int.TryParse(strs[1], out b);
                                int.TryParse(strs[2], out w);
                                board.SetScore(b, w);
                                board.CanSetPiece();
                                break;
                            }
                        case (int)ProtocolValue.ChangeTurn:
                            board.Turn = !board.Turn;
                            board.CanSetPiece();
                            break;
                        case (int)ProtocolValue.GameSet:
                            board.EndGame();
                            break;
                        case (int)ProtocolValue.Chat:
                            {
                                string msg = strs[1] + "\n";
                                chatStr.Add(msg);
                                break;
                            }
                        case (int)ProtocolValue.SetUniqueID:
                            int uniq;
                            int.TryParse(strs[1], out uniq);
                            uniqueID = uniq;
                            break;
                        case (int)ProtocolValue.StartGame:
                            board.SetReady(true);
                            board.CheckScore();
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

    void SendMsg(string str)
    {
        byte[] buffer = new byte[str.Length];
        buffer = Encoding.UTF8.GetBytes(str);
        client.Send(buffer);
    }

    public void SetPiece(int r, int c, int id)
    {
        string str = string.Format("1000,{0},{1},{2}/", r, c, id);
        SendMsg(str);
    }

    public void ChangePiece(int r, int c, int id)
    {
        string str = string.Format("1001,{0},{1},{2}/", r, c, id);
        SendMsg(str);
    }

    public void CheckScore(int b, int w)
    {
        string str = string.Format("1002,{0},{1}", b, w);
        SendMsg(str);
    }

    public void ChangeTurn()
    {
        string str = "1003";
        SendMsg(str);
    }

    public void EndGame()
    {
        string str = "/1004";
        SendMsg(str);
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 100), "Connect"))
        {
            Connect("127.0.0.1", portNum);
        }

        Vector2 stringSize = GUI.skin.textArea.CalcSize(new GUIContent("안녕"));
        int height = 300;
        int contentHeight = (int)stringSize.y * chatCount;
        int addedArea = (contentHeight >= height) ? contentHeight - height : 0;

        scrollPos = GUI.BeginScrollView(new Rect(0, 200, 200, height), scrollPos, new Rect(0, 200, 180, height + addedArea));

        float contentsHeight = 0;
        for (int i = 0; i < chatStr.Count; i++)
        {
            stringSize = GUI.skin.textArea.CalcSize(new GUIContent(chatStr[i]));

            GUI.TextArea(new Rect(0, contentsHeight, stringSize.x, stringSize.y), chatStr[i]);
            contentsHeight += stringSize.y;
        }
        GUI.EndScrollView();

        chatMsg = GUI.TextField(new Rect(0, 520, 190, 100), chatMsg);


        if (GUI.Button(new Rect(210, 520, 90, 100), "Send"))
        {
            if (chatMsg != string.Empty)
            {
                //chatMsg = "1005," + chatMsg;
                //SendMsg(chatMsg);
                chat += chatMsg + "\n";
                chat += chatMsg + "\n";
                chat += chatMsg + "\n";
                chat += chatMsg + "\n";
                chat += chatMsg + "\n";
                chat += chatMsg + "\n";
                chatMsg = string.Empty;
            }
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

        if (!client.Connected)
            client = null;
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
