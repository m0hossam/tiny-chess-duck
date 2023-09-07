using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    public Board theBoard;
    public int infinity = 999999999;

    public int Evaluate()
    {
        PieceList[] pieceLists = theBoard.GetAllPieceLists();
        int wP = pieceLists[0].Count,
            wN = pieceLists[1].Count,
            wB = pieceLists[2].Count,
            wR = pieceLists[3].Count,
            wQ = pieceLists[4].Count,
            wK = pieceLists[5].Count,
            bP = pieceLists[6].Count,
            bN = pieceLists[7].Count,
            bB = pieceLists[8].Count,
            bR = pieceLists[9].Count,
            bQ = pieceLists[10].Count,
            bK = pieceLists[11].Count,
            pawnWt = 100,
            knightWt = 320,
            bishopWt = 330,
            rookWt = 500,
            queenWt = 900,
            kingWt = 20000;
        int materialScore = kingWt * (wK - bK)
            + queenWt * (wQ - bQ)
            + rookWt * (wR - bR)
            + knightWt * (wN - bN)
            + bishopWt * (wB - bB)
            + pawnWt * (wP - bP);
        int eval = (materialScore) * (theBoard.IsWhiteToMove ? 1 : -1);
        return eval;
    }

    int Quiesce(int alpha, int beta) //takes too much time
    {
        int standPat = Evaluate();
        if (standPat >= beta)
            return beta;
        if (alpha < standPat)
            alpha = standPat;
        Move[] moves = theBoard.GetLegalMoves(true);
        foreach (Move move in moves)
        {
            theBoard.MakeMove(move);
            int score = -Quiesce(-beta, -alpha);
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
        if (depth == 0) return Quiesce(alpha, beta);
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

        int depth = 1;
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