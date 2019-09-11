using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    newClient client;

    // 0 : 빈칸, 1 : 검정, 2 : 하양, 3 : 놓을 수 있는 곳
    int[,] boardInfo = new int[8, 8];
    SpriteRenderer[,] pieces = new SpriteRenderer[8, 8];
    GameObject[,] tempPlaces = new GameObject[8, 8];

    List<int> changeList = new List<int>();

    GameObject slot;
    GameObject piece;
    GameObject tempPos;
    GameObject tempPlace;
    Sprite black;
    Sprite white;

    bool ready = false;
    public void SetReady(bool ready)
    {
        this.ready = ready;
    }
    bool isChanged = false;
    bool canChange = false;
    bool justCheck = false;
    bool gameSet = false;

    int blackScore = 2;    
    int whiteScore = 2;
    public void SetScore(int black, int white)
    {
        blackScore = black;
        whiteScore = white;
    }

    string state = string.Empty;
    bool turn = true; // true : 검정 턴, false : 하양 턴
    public bool Turn
    {
        get { return turn; }
        set { turn = value; }
    }

    void Awake()
    {
        client = GetComponent<newClient>();

        slot = Resources.Load<GameObject>("slot");
        piece = Resources.Load<GameObject>("piece");
        tempPos = Resources.Load<GameObject>("gray");
        tempPlace = Resources.Load<GameObject>("temp");
        black = Resources.Load<Sprite>("black");
        white = Resources.Load<Sprite>("white");
        BoardSetting();
        tempPos = Instantiate(tempPos, new Vector3(-10, -10, 0), Quaternion.identity);
    }

    void Update()
    {
        if (!ready || gameSet)
            return;

        // 현재 마우스 위치에 돌 미리 표시
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (pos.x < -6 || pos.x > 2 || pos.y < -3 || pos.y > 5) // 돌판범위 밖일 경우
        {
            tempPos.transform.position = new Vector3(-10, -10, 0);
        }
        else
        {
            pos.x = Mathf.Floor(pos.x) + 0.5f;
            pos.y = Mathf.Floor(pos.y) + 0.5f;
            pos.z = tempPos.transform.position.z;
            tempPos.transform.position = pos;

            if (Input.GetMouseButtonDown(0))
            {
                if (client.UniqueID == 0 && turn)
                    SetPieceWithClick(1);
                else if (client.UniqueID == 1 && !turn)
                    SetPieceWithClick(2);
            }
        }
    }

    private void OnGUI()
    {
        if (client.UniqueID == -1)
            state = "Unconnected.";
        else if (!ready)
            state = "상대를 기다리는 중...";
        else if (!gameSet)
        {
            if (turn)
                state = "검은 돌의 턴입니다.\n";
            else
                state = "하얀 돌의 턴입니다.\n";

            state += string.Format("검은 돌 개수: {0}\n" + "하얀 돌 개수: {1}\n", blackScore, whiteScore);
        }

        GUI.TextArea(new Rect(560, 0, 200, 100), state);
    }

    // 돌판 초기 세팅
    void BoardSetting()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                GameObject obj = Instantiate(slot, new Vector3(i - 5.5f, j - 2.5f, 0), Quaternion.identity, transform);
                obj.GetComponent<SpriteRenderer>().sortingOrder = -1;
                obj.name = string.Format("{0},{1}", i, j);

                tempPlace = Instantiate(tempPlace, new Vector3(i - 5.5f, j - 2.5f, 0), Quaternion.identity, transform);
                tempPlace.GetComponent<SpriteRenderer>().sortingOrder = 1;
                tempPlace.name = string.Format("temp:{0},{1}", i, j);
                tempPlace.SetActive(false);
                tempPlaces[i, j] = tempPlace;
            }
        }

        SetPiece(3, 4, 2);
        SetPiece(4, 3, 2);
        SetPiece(3, 3, 1);
        SetPiece(4, 4, 1);

        //SetPiece(3, 4, 1);
        //SetPiece(3, 5, 2);
        //SetPiece(3, 6, 1);
        //SetPiece(5, 4, 1);
        //SetPiece(5, 5, 2);
        //SetPiece(5, 6, 1);
        //SetPiece(7, 7, 2);
    }

    // 돌을 놓는다
    public void SetPiece(int row, int col, int id)
    {
        Sprite sprite = black;
        if (id == 2) sprite = white;

        pieces[row, col] = Instantiate(
            piece, new Vector3(row - 5.5f, col - 2.5f, 0), Quaternion.identity, transform).GetComponent<SpriteRenderer>();
        pieces[row, col].sprite = sprite;
        boardInfo[row, col] = id;
    }

    // 마우스 클릭으로 새 돌을 놓는다
    public void SetPieceWithClick(int id)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.transform != null)
        {
            string[] str = hit.transform.name.Split(',');
            int r, c;
            int.TryParse(str[0], out r);
            int.TryParse(str[1], out c);

            if (boardInfo[r, c] == 3)
            {
                if (CheckToChange(r, c, id))
                {
                    client.SetPiece(r, c, id);
                    boardInfo[r, c] = id;
                }
            }
            else // 이미 돌이 놓여진 위치일 경우
            {
                return;
            }
        }
    }

    // 해당 위치의 돌 색을 변경
    public void ChangePiece(int row, int col, int id)
    {
        Sprite sprite = black;
        if (id == 2) sprite = white;
        
        boardInfo[row, col] = id;
        pieces[row, col].sprite = sprite;
    }

    void ChangePieces(int row, int col, int toR, int toC, int id)
    {
        if (row + toR > 7 || row + toR < 0 || col + toC > 7 || col + toC < 0)
        {
            changeList.Clear();
            return;
        }

        // row + toR, col + toC의 돌을 체크
        int nextPiece = boardInfo[row + toR, col + toC];

        // 돌이 없을 경우
        if (nextPiece == 0 || nextPiece == 3)
        {
            changeList.Clear();
            return;
        }
        // 내 돌일 경우
        else if (nextPiece == id)
        {
            if (changeList.Count > 0)
            {
                for (int i = 0; i < changeList.Count; i++)
                {
                    if (!justCheck)
                        client.ChangePiece(changeList[i] / 10, changeList[i] % 10, id);
                }

                isChanged = true;
            }
            return;
        }
        // 상대방 돌일 경우
        else
        {
            changeList.Add((row+toR)*10 + col+toC);
        }

        ChangePieces(row+toR, col+toC, toR, toC, id);
        changeList.Clear();
    }

    // ChangePieces를 전 방향으로 호출
    public bool CheckToChange(int row, int col, int id)
    {
        ChangePieces(row, col, -1, -1, id);
        ChangePieces(row, col, -1, 0, id);        
        ChangePieces(row, col, -1, 1, id);
        ChangePieces(row, col, 0, -1, id);
        ChangePieces(row, col, 0, 1, id);
        ChangePieces(row, col, 1, -1, id);
        ChangePieces(row, col, 1, 0, id);
        ChangePieces(row, col, 1, 1, id);

        bool changed = isChanged;
        isChanged = false;
        return changed;
    }

    public bool CheckToChangeLite(int row, int col, int id)
    {
        for (int i=-1; i<=1; i++)
        {
            for(int j=-1; j<=1; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                ChangePieces(row, col, i, j, id);
                if (isChanged)
                {
                    isChanged = false;
                    return true;
                }
            }
        }
        return false;
    }

    // 돌을 놓을 수 있는 위치인지 표시
    void CanSetPiece(int id)
    {
        canChange = false;

        // 자기 차례가 아닌 경우 판 정리
        if (id == -1)
        {
            for (int r = 0; r < 8; r++) {
                for (int c = 0; c < 8; c++) {
                    if (boardInfo[r, c] == 3)
                    {
                        tempPlaces[r, c].SetActive(false);
                        boardInfo[r, c] = 0;
                    }
                }
            }
            canChange = true;
            return;
        }

        justCheck = true;
        
        for (int r = 0; r < 8; r++) {
            for (int c = 0; c < 8; c++) {
                if (boardInfo[r, c] == 3)
                {
                    tempPlaces[r,c].SetActive(false);
                    boardInfo[r, c] = 0;
                }
                else if (boardInfo[r, c] != 0)
                    continue;
              
                if (CheckToChangeLite(r, c, id))
                {
                    if(!canChange)
                        canChange = true;
                    tempPlaces[r, c].SetActive(true);
                    boardInfo[r, c] = 3;
                }
            }
        }
        justCheck = false;
    }

    public void CanSetPiece()
    {
        if (client.UniqueID == 0 && turn)
            CanSetPiece(1);
        else if (client.UniqueID == 1 && !turn)
            CanSetPiece(2);
        else
            CanSetPiece(-1);

        if (!canChange && !gameSet)
            client.ChangeTurn();
    }

    public void CheckScore()
    {
        if (client.UniqueID != 1)
            return;

        blackScore = 0;
        whiteScore = 0;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (boardInfo[i, j] == 1)
                    blackScore++;
                else if (boardInfo[i, j] == 2)
                    whiteScore++;
            }
        }
        client.CheckScore(blackScore, whiteScore);

        if (blackScore == 0 || whiteScore == 0 || blackScore + whiteScore == 64)
        {
            client.EndGame();
        }
    }

    public void EndGame()
    {
        gameSet = true;
        state = "게임이 종료되었습니다.\n";
        if (blackScore > whiteScore)
            state += "검은 돌 승리!";
        else if (whiteScore > blackScore)
            state += "하얀 돌 승리!";
        else
            state += "동점 무승부!";
    }
}
