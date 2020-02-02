using System.Collections.Generic;
using UnityEngine;

public class AI
{
    // private Checkers checkObj = new Checkers();
    // private List<Pices> forcedPieces;
    private List<Moves> move;
    private static Moves best;
    //private Moves bestMove;
    //  private List<List<Moves>> moves;
    private List<Forced> forcedPieces;
    //private static int maxDepth = 4;
    private void getMove(Pices[,] p1, Pices p, int x, int y)
    {
        Moves m = new Moves(p, x, y);
        move = new List<Moves>();
        move = m.getMoves(p1);


    }

    private List<List<Moves>> getMoves(Pices[,] p, bool isWhiteTurn)
    {
        List<List<Moves>> moves = new List<List<Moves>>();
        forcedPieces = ScanForcedPossibleMove(p, isWhiteTurn);

        if (forcedPieces.Count != 0)
        {
            for (int i = 0; i < forcedPieces.Count; i++)
            {
                if (forcedPieces[i].forcedPice != null && forcedPieces[i].forcedPice.isWhite == isWhiteTurn)
                {
                    getMove(p, forcedPieces[i].forcedPice, forcedPieces[i].x, forcedPieces[i].y);
                    moves.Add(move);
                }
            }

        }
        else
        {

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (p[i, j] != null && p[i, j].isWhite == isWhiteTurn)
                    {
                        getMove(p, p[i, j], i, j);
                        moves.Add(move);
                    }
                }
            }
        }
        return moves;

    }
    private int evalFunction(Pices[,] pices, bool isWhiteTurn)
    {
        bool white;
        //if white is true we are calcluting for white or else we are calculating for black
        if (isWhiteTurn == true)
        {
            white = true;

        }
        else
        {
            white = false;
        }
        int ourColour = 0;
        int enemyColour = 0;
        Pices p;

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                p = pices[i, j];
                if (p != null)
                {
                    if (p.isWhite == white)
                    {
                        ourColour += calVal(p, i, j);
                    }
                    else
                    {
                        enemyColour += calVal(p, i, j);
                    }
                }

            }
        }
        //int t = Mathf.Abs(ourColour - enemyColour);
        return ourColour - enemyColour;
    }
    private int calVal(Pices p, int i, int j)
    {
        int value;
        {


            int[,] tableWeight = { { 4, 0, 4, 0, 4, 0, 4, 0
                },
                                  { 0, 4, 0, 3, 0, 3, 0, 3
                },
                                   { 3, 0, 2, 0, 2, 0, 4, 0
                },
                                    { 0, 4, 0, 2, 0, 1, 0, 3
                },
                                    { 3, 0, 2, 0, 1, 0, 4, 0
                },
                                      { 0, 4, 0, 2, 0, 2, 0, 3
                },
                                        { 3, 0, 3, 0, 3, 0, 4, 0
                },
                                        { 0, 4, 0, 4, 0, 4, 0, 4
                }
            };


            if (p.isWhite == true)
            {
                if (i >= 0 && j <= 2)
                {
                    value = 7;
                }
                else
                {
                    value = 5;
                }
            }
            else
            {
                if (i >= 0 && j <= 5)
                {
                    value = 7;
                }
                else
                {
                    value = 5;
                }
            }
            return value * tableWeight[i, j];
        }
    }


    private int MinMax2(Pices[,] pices, int alpha, int beta, int depth, bool maxMaxingPlayer)
    {

        //  Debug.Log("in the function ");
        if (cutOff(pices) || depth == 0)
        {
            //  Debug.Log("here :"+depth);
            return evalFunction(pices, maxMaxingPlayer);
        }
        int maxeval, mineval;
        int value;
        List<List<Moves>> moves = getMoves(pices, maxMaxingPlayer);
        Pices[,] p;


        if (!maxMaxingPlayer)
        {
            maxeval = -9999990;
            foreach (var sublist in moves)
                foreach (var mo in sublist)
                {
                    Board b = new Board(mo, pices);
                    p = b.nextBoard();
                    value = MinMax2(p, alpha, beta, depth - 1, false);
                    //maxeval = Mathf.Max(maxeval, value);
                    if (maxeval < value)
                    {
                         best = mo;
                        maxeval = value;
                        Debug.Log(maxeval);
                    }

                    alpha = Mathf.Max(alpha, maxeval);
                    if (beta <= alpha)
                    {

                        break;

                    }


                }

            return maxeval;


        }
        else
        {
            mineval = 99999;
            foreach (var sublist in moves)
                foreach (var mo in sublist)
                {
                    Board b = new Board(mo, pices);
                    p = b.nextBoard();
                    value = MinMax2(p, alpha, beta, depth - 1, true);
                    if (value < mineval)
                    {
                        mineval = value;

                    }
                    // mineval = Mathf.Min(value, mineval);
                    beta = Mathf.Min(beta, value);
                    if (beta <= alpha)
                    {
                        break;

                    }



                }
            return mineval;


        }
    }

    /*private Moves MiniMax(Pices[,] pices, bool isWhiteTurn)
     {
         Pices[,] p;
         Moves bestMove = null;
         int value, maxValue = -999999999;
         List<List<Moves>> moves = getMoves(pices, isWhiteTurn);
         foreach (var sublist in moves)
             foreach (var mo in sublist)
             {
                 Board b = new Board(mo, pices);
                 p = b.nextBoard();
                 value = minMove(p, 1, maxValue, 2147483647, isWhiteTurn);

                 if (value > maxValue)
                 {
                     maxValue = value;
                     bestMove = mo;

                 }

             }
         return bestMove;

     }
     private int minMove(Pices[,] pices, int depth, int alpha, int beta, bool isWhiteTurn)
     {
         if (cutOff(pices, depth))
             return evalFunction(pices, isWhiteTurn);
         Pices[,] p;
         int value = 0;
         List<List<Moves>> moves = getMoves(pices, isWhiteTurn);
         foreach (var sublist in moves)
             foreach (var mo in sublist)
             {
                 Board b = new Board(mo, pices);
                 p = b.nextBoard();
                 value = maxMove(p, depth + 1, alpha, beta, isWhiteTurn);

             }
         if (value < beta)
         {
             beta = value;
         }
         if (beta < alpha)
         {
             return alpha;
         }
         return beta;
     }
     private int maxMove(Pices[,] pices, int depth, int alpha, int beta, bool isWhiteTurn)
     {
         if (cutOff(pices, depth))
             return evalFunction(pices, isWhiteTurn);
         Pices[,] p;
         int value = 0;
         List<List<Moves>> moves = getMoves(pices, isWhiteTurn);
         foreach (var sublist in moves)
             foreach (var mo in sublist)
             {
                 Board b = new Board(mo, pices);
                 p = b.nextBoard();
                 value = minMove(p, depth + 1, alpha, beta, isWhiteTurn);

             }
         if (value > alpha)
         {
             alpha = value;
         }
         if (alpha < beta)
         {
             return beta;
         }
         return alpha;
     } */
    private bool cutOff(Pices[,] p)
    {
        int blackCount = 0, whiteCount = 0;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (p[i, j] != null)
                {
                    if (p[i, j].isWhite)
                    {
                        whiteCount++;
                    }
                    else
                    {
                        blackCount++;
                    }
                }
            }
        }

        if (whiteCount == 0 || blackCount == 0)
            return true;
        else
            return false;
    }
    public Moves aiMove(Pices[,] pices, bool isWhiteTurn)
    {
        Moves bestMove = null;
      //  Pices[,] p;
        //int value;
        int bestValue = MinMax2(pices, -99999999, 999999999, 4, isWhiteTurn);
        Debug.Log(bestValue);
        /* List<List<Moves>> moves = getMoves(pices, isWhiteTurn);

         foreach (var sublist in moves)
             foreach (var mo in sublist)
             {
                 Board b = new Board(mo, pices);
                 p = b.nextBoard();
                 value = evalFunction(p, isWhiteTurn);
                 if (value == bestValue)
                 {
                     bestMove = mo;
                     break;
                 }
             }*/
        bestMove = best;
        Debug.Log(bestMove);

        return bestMove;


    }

    // gets ALL the forced Pices on the board
    private List<Forced> ScanForcedPossibleMove(Pices[,] pices, bool isWhiteTurn)
    {
        List<Forced> forceds = new List<Forced>();
        //   forcedPieces = new List<Pices>();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (pices[i, j] != null && pices[i, j].isWhite == isWhiteTurn)
                {
                    if (pices[i, j].IsForceToMove(pices, i, j))
                    {
                        // forcedPieces.Add(pices[i, j]);
                        Forced f = new Forced(pices[i, j], i, j);
                        forceds.Add(f);
                    }
                }
            }
        }
        return forceds;
    }


}

public class Forced : Pices
{
    public Pices forcedPice;
    public int x;
    public int y;
    public Forced(Pices p, int i, int j)
    {
        forcedPice = p;
        x = i;
        y = j;
    }
}

public class Moves
{
    public int x, y, nextX, nextY;
    public Pices pice;
    private List<Moves> move;
    public Moves(Pices p, int i, int j)
    {
        pice = p;
        x = i;
        y = j;
        nextX = 0;
        nextY = 0;
    }
    public Moves()
    {
        pice = null;
        x = 0;
        y = 0;
        nextX = 0;
        nextY = 0;
    }
    public Moves(Pices p, int i, int j, int ni, int nj)
    {
        pice = p;
        x = i;
        y = j;
        nextX = ni;
        nextY = nj;
    }


    public List<Moves> getMoves(Pices[,] pices)
    {
        bool isForced = pice.IsForceToMove(pices, x, y);
        move = new List<Moves>();
        if (pice != null)
            if (isForced)
            {

                if (Trymove2(x, y, x + 2, y + 2, pice, pices))
                {
                    nextX = x + 2;
                    nextY = y + 2;
                    Moves m = new Moves(pice, x, y, nextX, nextY);
                    move.Add(m);

                }
                if (Trymove2(x, y, x - 2, y - 2, pice, pices))
                {
                    nextX = x - 2;
                    nextY = y - 2;
                    Moves m = new Moves(pice, x, y, nextX, nextY);
                    move.Add(m);
                }
                if (Trymove2(x, y, x + 2, y - 2, pice, pices))
                {
                    nextX = x + 2;
                    nextY = y - 2;
                    Moves m = new Moves(pice, x, y, nextX, nextY);
                    move.Add(m);
                }
                if (Trymove2(x, y, x - 2, y + 2, pice, pices))
                {
                    nextX = x - 2;
                    nextY = y + 2;
                    Moves m = new Moves(pice, x, y, nextX, nextY);
                    move.Add(m);
                }
            }
            else
            {
                if (Trymove2(x, y, x + 1, y + 1, pice, pices))
                {
                    nextX = x + 1;
                    nextY = y + 1;
                    Moves m = new Moves(pice, x, y, nextX, nextY);
                    move.Add(m);

                }
                if (Trymove2(x, y, x - 1, y - 1, pice, pices))
                {
                    nextX = x - 1;
                    nextY = y - 1;
                    Moves m = new Moves(pice, x, y, nextX, nextY);
                    move.Add(m);
                }
                if (Trymove2(x, y, x + 1, y - 1, pice, pices))
                {
                    nextX = x + 1;
                    nextY = y - 1;
                    Moves m = new Moves(pice, x, y, nextX, nextY);
                    move.Add(m);
                }
                if (Trymove2(x, y, x - 1, y + 1, pice, pices))
                {
                    nextX = x - 1;
                    nextY = y + 1;
                    Moves m = new Moves(pice, x, y, nextX, nextY);
                    move.Add(m);
                }

            }
        return move;


    }

    private bool Trymove2(int x1, int y1, int x2, int y2, Pices select, Pices[,] pices)
    {
        bool rightmove = false, Killed = false;
        select = pices[x1, y1];
        List<Pices> forcedPieces = ScanforPossibleMove(x1, y1, pices);
        //multiplayer support 
        Vector2 startDrag = new Vector2(x1, y1);
        Vector2 endDrag = new Vector2(x2, y2);


        //out of bounds 
        if (x2 < 0 || x2 >= 8 || y2 < 0 || y2 >= 8)
        {
            return false;
            // if (select != null)
            //    rightmove = false;

            //let the piece be at the same position
        }
        //if the start drag and the stop drag is same
        if (select != null)
        {
            if (endDrag == startDrag)
            {
                rightmove = false;
            }
        }
        //check if its vaild move 
        if (select.ValidMove(pices, x1, y1, x2, y2))
        {
            if (Mathf.Abs(x2 - x1) == 2)
            {
                Pices p = pices[(x1 + x2) / 2, (y1 + y2) / 2];
                if (p != null)
                {
                    Killed = true;
                }
            }

            if (forcedPieces.Count != 0 && !Killed)
            {
                rightmove = false;

            }

            rightmove = true;


        }
        else
        {
            rightmove = false;

        }
        return rightmove;

    }
    private List<Pices> ScanforPossibleMove(int x, int y, Pices[,] pices)
    {
        List<Pices> forcedPieces = new List<Pices>();

        if (pices[x, y].IsForceToMove(pices, x, y))
            forcedPieces.Add(pices[x, y]);


        return forcedPieces;

    }

}

class Board
{
    private Moves m;
    private Pices[,] pices;

    public Board(Moves m, Pices[,] p)
    {
        this.m = m;
        this.pices = p;
    }
    public Pices[,] nextBoard()
    {
        //Debug.Log(m.pice + " ," + " ," + m.x + "," + m.y + "," + m.nextX + "," + m.nextY);
        pices[m.nextX, m.nextY] = m.pice;
        pices[m.x, m.y] = null;
        return pices;


    }
}
