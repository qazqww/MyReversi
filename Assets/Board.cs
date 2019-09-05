using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    newClient client;

    // 0 : 빈칸, 1 : 빨강, 2 : 파랑
    int[,] boardInfo = new int[8, 8];
    SpriteRenderer[,] pieces = new SpriteRenderer[8, 8];

    List<int> changeList = new List<int>();

    GameObject slot;
    GameObject piece;
    GameObject temp;
    Sprite black;
    Sprite white;

    bool isChanged = false;

    string state = string.Empty;
    bool turn = true; // true : 검정 턴, false : 하양 턴
    public void SetTurn(bool turn)
    {
        this.turn = turn;
    }

    void Awake()
    {
        client = GetComponent<newClient>();

        slot = Resources.Load<GameObject>("slot");
        piece = Resources.Load<GameObject>("piece");
        temp = Resources.Load<GameObject>("gray");
        black = Resources.Load<Sprite>("black");
        white = Resources.Load<Sprite>("white");
        BoardSetting();
        temp = Instantiate(temp, new Vector3(-10, -10, 0), Quaternion.identity);
    }

    void Update()
    {
        // 현재 마우스 위치에 돌 미리 표시
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (pos.x < -4 || pos.x > 4 || pos.y < -3 || pos.y > 5) // 돌판범위 밖일 경우
        {
            temp.transform.position = new Vector3(-10, -10, 0);
        }
        else
        {
            pos.x = Mathf.Floor(pos.x) + 0.5f;
            pos.y = Mathf.Floor(pos.y) + 0.5f;
            pos.z = temp.transform.position.z;
            temp.transform.position = pos;

            if (Input.GetMouseButtonDown(0) && turn)
                SetPieceWithClick(1);
            else if (Input.GetMouseButtonDown(0) && !turn)
                SetPieceWithClick(2);
        }
    }

    private void OnGUI()
    {
        if (turn)
            state = "검은 돌의 턴입니다.";
        else
            state = "하얀 돌의 턴입니다.";

        GUI.TextArea(new Rect(400, 0, 200, 100), state);
    }

    // 돌판 초기 세팅
    void BoardSetting()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                GameObject obj = Instantiate(slot, new Vector3(i - 3.5f, j - 2.5f, 0), Quaternion.identity, transform);
                obj.GetComponent<SpriteRenderer>().sortingOrder = -1;
                obj.name = string.Format("{0},{1}", i, j);
            }
        }

        SetPiece(3, 4, 2);
        SetPiece(4, 3, 2);
        SetPiece(3, 3, 1);
        SetPiece(4, 4, 1);
    }

    // 돌을 놓는다
    public void SetPiece(int row, int col, int id)
    {
        Sprite sprite = black;
        if (id == 2) sprite = white;

        pieces[row, col] = Instantiate(
            piece, new Vector3(row - 3.5f, col - 2.5f, 0), Quaternion.identity, transform).GetComponent<SpriteRenderer>();
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

            if (boardInfo[r, c] == 0)
            {
                if (CheckToChange(r, c, id))
                {
                    client.SetPiece(r, c, id);
                    //SetPiece(r, c, id);
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
        if (nextPiece == 0)
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
                    //ChangePiece(changeList[i] / 10, changeList[i] % 10, id);
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
        bool changed = false;
        ChangePieces(row, col, -1, -1, id);
        ChangePieces(row, col, -1, 0, id);        
        ChangePieces(row, col, -1, 1, id);
        ChangePieces(row, col, 0, -1, id);
        ChangePieces(row, col, 0, 1, id);
        ChangePieces(row, col, 1, -1, id);
        ChangePieces(row, col, 1, 0, id);
        ChangePieces(row, col, 1, 1, id);

        changed = isChanged;
        isChanged = false;
        return changed;
    }
}
