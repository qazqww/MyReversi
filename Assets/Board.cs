using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    // 0 : 빈칸, 1 : 빨강, 2 : 파랑
    int[,] boardInfo = new int[8, 8];

    GameObject slot;
    GameObject piece;
    Sprite black;
    Sprite white;

    void Awake()
    {
        slot = Resources.Load<GameObject>("slot");
        piece = Resources.Load<GameObject>("piece");
        black = Resources.Load<Sprite>("black");
        white = Resources.Load<Sprite>("white");
        BoardSetting();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            if (hit.transform != null)
            {
                Debug.Log(hit.transform.name);
            }
        }
    }

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

        SetPiece(3, 4, 1);
        SetPiece(4, 3, 1);
        SetPiece(3, 3, 2);
        SetPiece(4, 4, 2);
    }

    void SetPiece(int row, int col, int id)
    {
        if (boardInfo[row, col] != 0)
            return;

        Sprite sprite = black;
        if (id == 2) sprite = white;

        SpriteRenderer renderer = Instantiate(            
            piece, new Vector3(row - 3.5f, col - 2.5f, 0), Quaternion.identity, transform).GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        boardInfo[row, col] = id;
    }
}
