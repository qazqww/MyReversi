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

enum MessageType
{
    system,
    player
}

public class newClient : MonoBehaviour
{
    Socket client;

    Board board;
    Vector2 scrollPos = Vector2.zero;

    int portNum = 80;
    bool connected = false;
    int connectTrycount = 1;
    float elapsedTime = 0;
    string id = string.Empty;
    int uniqueID = -1;
    public int UniqueID
    {
        get { return uniqueID; }
    }

    public GameObject chatPanel, textObj;
    public InputField chatBox;

    void Start()
    {
        board = GetComponent<Board>();
        Connect("127.0.0.1", portNum);
    }

    void Update()
    {
        if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
            chatBox.ActivateInputField();

        if (client == null)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > 6f)
            {
                elapsedTime = 0;
                SendChatMessage(string.Format("Connecting... (Try: {0})", connectTrycount), MessageType.system);
                connectTrycount++;
                Connect("127.0.0.1", portNum);
            }
        }

        if (!connected && client.Connected)
        {
            connected = true;
            SendChatMessage("Connected!", MessageType.system);
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
                                SendChatMessage(strs[1], MessageType.player);
                                break;
                            }
                        case (int)ProtocolValue.SetUniqueID:
                            int uniq;
                            int.TryParse(strs[1], out uniq);
                            uniqueID = uniq;

                            if (uniqueID % 2 == 0)
                                id = "Black" + uniqueID;
                            else
                                id = "White" + uniqueID;
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

    private void OnGUI()
    {
        if (GUI.Button(new Rect(725, 430, 50, 60), "Send"))
        {
            if(chatBox.text != string.Empty)
            {
                //SendChatMessage(chatBox.text, MessageType.player);
                Chat(chatBox.text);
                chatBox.text = string.Empty;
            }
        }
    }

    void SendChatMessage(string text, MessageType msgType)
    {
        //string tempStr = text;
        GameObject newText = Instantiate(textObj, chatPanel.transform);
        Text textInfo = newText.GetComponent<Text>();
        textInfo.text = text;

        if (msgType == MessageType.player)
            textInfo.color = Color.black;
        else if (msgType == MessageType.system) {
            textInfo.color = Color.green;
            textInfo.fontStyle = FontStyle.Italic;
        }

        //messageList.Add(tempStr);
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

    void Chat(string chatMsg)
    {
        string str = string.Format("1005,{0}: {1}", id, chatMsg);
        SendMsg(str);
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
            SendChatMessage("Connect Failed.", MessageType.system);
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
