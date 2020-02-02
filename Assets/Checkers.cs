using System.Collections.Generic;
using UnityEngine;

public class Checkers : MonoBehaviour
{
    public Pices[,] pices = new Pices[8, 8];
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;
    private Vector3 offset = new Vector3(-4f, 0, -4f);
    private Vector3 offset2 = new Vector3(0.5f, 0, 0.5f);

    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;
    private Pices selectedPiece;
    private List<Pices> forcedPieces;

    public bool isWhiteTurn;
    public bool isWhite;
    private bool hasKilled;

    private void Update()
    {
        updateMouseOver();
        //Debug.Log(mouseOver);
        //if its my turn
        if((isWhite)?isWhiteTurn:!isWhiteTurn)
        {
            int x = (int)mouseOver.x;
            int y = (int)mouseOver.y;

            if (selectedPiece != null)
                UpdatePieceDrag(selectedPiece);

            if (Input.GetMouseButtonDown(0))
                SelectPiece(x, y);

            if (Input.GetMouseButtonUp(0))
                Trymove((int)startDrag.x, (int)startDrag.y, x, y);
        }
    }

    private void UpdatePieceDrag(Pices p)
    {
        if (!Camera.main)
        {
            Debug.Log("NO CAMERA");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }

    }
    private void updateMouseOver()
    {
        if(!Camera.main)
        {
            Debug.Log("NO CAMERA");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            mouseOver.x = (int) (hit.point.x - offset.x);
            mouseOver.y = (int) (hit.point.z - offset.z);
        }
        else
        {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }
    }
    private void SelectPiece(int x,int y)
    {
        //out of bounds 
        if (x < 0 || x >= 8 || y < 0 || y >= 8 )
            return;

        Pices p = pices[x, y];
        if(p != null && p.isWhite == isWhite )
        {
            if (forcedPieces.Count == 0)
            {
                selectedPiece = p;
                startDrag = mouseOver;
            }
            else
            {
                //look for the pice under our forced pices list 
                if(forcedPieces.Find(fp => fp == p)== null)
                {
                    return;
                }
                selectedPiece = p;
                startDrag = mouseOver; 
            }
           // Debug.Log(selectedPiece.name);
        }


    }
    private void Trymove(int x1,int y1,int x2,int y2)
    {

        forcedPieces = ScanforPossibleMove();
        //multiplayer support 
        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = pices[x1, y1];

        //out of bounds 
        if(x2 < 0|| x2 >= 8  || y2 < 0 || y2 >= 8)
        {
            if (selectedPiece != null)
                MovePiece(selectedPiece, x1,y1);

             
            selectedPiece = null;
            startDrag = Vector2.zero;
            return;
        }

        if(selectedPiece!=null)
        {
            if(endDrag == startDrag)
            {
                MovePiece(selectedPiece, x1, y1);
                selectedPiece = null;
                startDrag = Vector2.zero;
                return;
            }
        }
        //check if its vaild move 
        if(selectedPiece.ValidMove(pices,x1,y1,x2,y2))
        {
            if(Mathf.Abs(x2 - x1) == 2 )
            {
                Pices p = pices[(x1 + x2) / 2, (y1 + y2) / 2];
                if(p!=null)
                {
                    pices[(x1 + x2) / 2, (y1 + y2) / 2] = null;
                    Destroy(p.gameObject);
                    hasKilled = true;
                }
            }

            if(forcedPieces.Count != 0 && !hasKilled)
            {
                MovePiece(selectedPiece, x1, y1);
                selectedPiece = null;
                startDrag = Vector2.zero;
                return;

            }

            pices[x2, y2] = selectedPiece;
            pices[x1, y1] = null;
            MovePiece(selectedPiece, x2, y2);

            EndTurn();
        }
        else
        {
            MovePiece(selectedPiece, x1, y1);
            selectedPiece = null;
            startDrag = Vector2.zero;
            return;
        }

      //  MovePiece(selectedPiece, x2, y2);
    }   

    private void EndTurn()
    {
        int x = (int)endDrag.x;
        int y = (int)endDrag.y;
        //promotion
        if(selectedPiece !=null)
        {
               if(selectedPiece.isWhite && !selectedPiece.isKing && y==7)
            {
               selectedPiece.isKing = true;
               selectedPiece.transform.Rotate(Vector3.right * 180);
            }
            else if (!selectedPiece.isWhite && !selectedPiece.isKing && y == 0)
            {
                selectedPiece.isKing = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);
            }

        }
        selectedPiece = null;
        startDrag = Vector2.zero;

        if (ScanforPossibleMove(selectedPiece, x, y).Count != 0 && hasKilled)
            return;

        isWhiteTurn = !isWhiteTurn;
        isWhite = !isWhite;
        if (!isWhiteTurn)
            aiMove();
        hasKilled = false;
        CheckVictory();




    }
    private void aiMove()
    {
        Pices[,] p = pices.Clone() as Pices[,];
        AI a = new AI();
       Moves m = a.aiMove(p, isWhiteTurn);
        //  Pices p = pices[m.x, m.y];
      //  Debug.Log(m.pice + "G" + m.x + " " + m.y);

       // selectedPiece = m.pice;
        Trymove(m.x,m.y,m.nextX,m.nextY);
        isWhiteTurn = true;
        isWhite = true;

    }
    private void CheckVictory()
    {
        var ps = FindObjectsOfType<Pices>();
        bool hasWhite = false, hasBlack = false;
        for(int i=0; i< ps.Length; i++)
        {
            if (ps[i].isWhite)
                hasWhite = true;
            else
                hasBlack = true;
        }
        if (!hasWhite)
            Victory(false);
        if (!hasBlack)
            Victory(true);

    }
    private void Victory(bool isWhite)
    {
        if (isWhite)
            Debug.Log("white team has won");
        else
            Debug.Log("Black team has won");
    }

    private List<Pices> ScanforPossibleMove(Pices p,int x,int y)
    {
        forcedPieces = new List<Pices>();

        if (pices[x, y].IsForceToMove(pices, x, y))
            forcedPieces.Add(pices[x, y]);


        return forcedPieces;

    }
    private List<Pices> ScanforPossibleMove()
    {
        forcedPieces = new List<Pices>();
        for(int i=0; i<8 ;i++)
        {
            for(int j=0;j <8;j++)
            {
                if(pices[i,j] !=null && pices[i,j].isWhite == isWhiteTurn)
                {
                    if(pices[i,j].IsForceToMove(pices,i,j))
                    {
                        forcedPieces.Add(pices[i, j]);
                    }
                }
            }
        }
        return forcedPieces;
    }

    private void Start()
    {
        isWhiteTurn = true;
        isWhite = true;
        forcedPieces = new List<Pices>();
        GenerateBord();

    }
    private void GenerateBord()
    {
        for( int y = 0;y < 3; y++)
        {
            bool oddrow = (y % 2 == 0) ;
            for(int x = 0; x < 8; x += 2)
            {
                GeneratePiece((oddrow) ? x : x+1, y);
            }
        }

        for (int y = 7; y > 4; y--)
        {
            bool oddrow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                GeneratePiece((oddrow) ? x : x + 1, y);
            }
        }
    }
    private void GeneratePiece(int x,int y)
    {
        bool isWhitePicese = (y < 3) ? true:false;
        GameObject go = Instantiate((isWhitePicese) ? whitePiecePrefab:blackPiecePrefab) as GameObject;
        go.transform.SetParent(transform);
        Pices p = go.GetComponent<Pices>();
        pices[x, y] = p;
        MovePiece(p, x, y);
    }
    public void MovePiece(Pices p,int x,int y)
    {
       p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + offset + offset2;

    }

}
