using System;
using System.Collections.Immutable;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    public Board theBoard;
    public Timer theTimer; //might be needed in the future
    public const int infinity = 999999999;
    public int[] pieceWeights = { 100, 320, 330, 500, 900, 20000 }; // P, N, B, R, Q, K
    public int[] pieceSquareTable =
    {
        //Pawns
        0,  0,  0,  0,  0,  0,  0,  0,
        50, 50, 50, 50, 50, 50, 50, 50,
        10, 10, 20, 30, 30, 20, 10, 10,
        5,  5, 10, 25, 25, 10,  5,  5,
        0,  0,  0, 20, 20,  0,  0,  0,
        5, -5,-10,  0,  0,-10, -5,  5,
        5, 10, 10,-20,-20, 10, 10,  5,
        0,  0,  0,  0,  0,  0,  0,  0,
        //Knights
        -50,-40,-30,-30,-30,-30,-40,-50,
        -40,-20,  0,  0,  0,  0,-20,-40,
        -30,  0, 10, 15, 15, 10,  0,-30,
        -30,  5, 15, 20, 20, 15,  5,-30,
        -30,  0, 15, 20, 20, 15,  0,-30,
        -30,  5, 10, 15, 15, 10,  5,-30,
        -40,-20,  0,  5,  5,  0,-20,-40,
        -50,-40,-30,-30,-30,-30,-40,-50,
        //Bishops
        -20,-10,-10,-10,-10,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5, 10, 10,  5,  0,-10,
        -10,  5,  5, 10, 10,  5,  5,-10,
        -10,  0, 10, 10, 10, 10,  0,-10,
        -10, 10, 10, 10, 10, 10, 10,-10,
        -10,  5,  0,  0,  0,  0,  5,-10,
        -20,-10,-10,-10,-10,-10,-10,-20,
        //Rooks
        0,  0,  0,  0,  0,  0,  0,  0,
        5, 10, 10, 10, 10, 10, 10,  5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        0,  0,  0,  5,  5,  0,  0,  0,
        //Queens
        -20,-10,-10, -5, -5,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5,  5,  5,  5,  0,-10,
        -5,  0,  5,  5,  5,  5,  0, -5,
        0,  0,  5,  5,  5,  5,  0, -5,
        -10,  5,  5,  5,  5,  5,  0,-10,
        -10,  0,  5,  0,  0,  0,  0,-10,
        -20,-10,-10, -5, -5,-10,-10,-20,
        //Kings in opening
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -20,-30,-30,-40,-40,-30,-30,-20,
        -10,-20,-20,-20,-20,-20,-20,-10,
        20, 20,  0,  0,  0,  0, 20, 20,
        20, 30, 10,  0,  0, 10, 30, 20
    };

    public int Evaluate()
    {
        int materialScore = 0;
        int activityScore = 0;
        PieceList[] pieceLists = theBoard.GetAllPieceLists();
        for (int i = 0; i < 12; i++)
        {
            materialScore += pieceLists[i].Count * pieceWeights[i % 6] * (pieceLists[i].IsWhitePieceList ? 1 : -1);
            for (int j = 0; j < pieceLists[i].Count; j++)
            {
                int file = pieceLists[i].GetPiece(j).Square.File; //column
                int rank = pieceLists[i].GetPiece(j).Square.Rank; //row
                int index = pieceLists[i].IsWhitePieceList ? file + (8 * (7 - rank)) : file + (8 * rank);
                activityScore += pieceSquareTable[index + 64 * (i % 6)] * (pieceLists[i].IsWhitePieceList ? 1 : -1);
            }
        }

        int eval = (materialScore + activityScore) * (theBoard.IsWhiteToMove ? 1 : -1);
        return eval;
    }

    public Move[] OrderMoves(Move[] moves)
    {
        Array.Sort(moves, CaptureMoveComparer);
        return moves;
    }

    public int CaptureMoveComparer(Move moveA, Move moveB) // MVV - LVA: Most Valuable Victim - Least Valuable Aggressor
    {
        return (pieceWeights[(int)moveB.CapturePieceType - 1] - pieceWeights[(int)moveB.MovePieceType - 1])
            - (pieceWeights[(int)moveA.CapturePieceType - 1] - pieceWeights[(int)moveA.MovePieceType - 1]);
    }

    public int Quiesce(int alpha, int beta, int calls) //needs more time optimization
    {
        int standPat = Evaluate();
        if (standPat >= beta)
            return beta;
        if (alpha < standPat)
            alpha = standPat;
        if (calls > 5) //cutoff after 5 consecutive captures to reduce time
            return alpha;
        Move[] moves = theBoard.GetLegalMoves(true); //captures only
        moves = OrderMoves(moves);
        foreach (Move move in moves)
        {
            theBoard.MakeMove(move);
            int score = -Quiesce(-beta, -alpha, calls + 1);
            theBoard.UndoMove(move);

            if (score >= beta)
                return beta;
            if (score > alpha)
                alpha = score;
        }
        return alpha;
    }

    public int AlphaBeta(int alpha, int beta, int depth)
    {
        if (depth == 0)
            return Quiesce(alpha, beta, 0);
        Move[] moves = theBoard.GetLegalMoves();
        foreach (Move move in moves)
        {
            theBoard.MakeMove(move);
            int score = -AlphaBeta(-beta, -alpha, depth - 1);
            theBoard.UndoMove(move);
            if (score >= beta)
                return beta;   //  fail hard beta-cutoff
            if (score > alpha)
                alpha = score; // alpha acts like max in MiniMax
        }
        return alpha;
    }

    public Move Think(Board board, Timer timer)
    {
        theBoard = board;
        theTimer = timer;

        int depth = 3;
        Move[] moves = theBoard.GetLegalMoves();
        Move bestMove = moves[0];
        if (depth == 0) return bestMove;
        int bestScore = -infinity;
        int alpha = -infinity;
        int beta = infinity;

        foreach (Move move in moves)
        {
            theBoard.MakeMove(move);
            int score = -AlphaBeta(-beta, -alpha, depth - 1);
            theBoard.UndoMove(move);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }
        return bestMove;
    }
}