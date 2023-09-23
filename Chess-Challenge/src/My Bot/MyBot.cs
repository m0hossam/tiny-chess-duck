using System;
using ChessChallenge.API;

// I think hash collisions are causing blunders ??!

public class MyBot : IChessBot
{
    public struct Transposition
    {
        public ulong zKey = 0;
        public Move move = Move.NullMove;
        public int eval = 0;
        public sbyte depth = -128;
        public sbyte flag = INVALID;

        public Transposition()
        {
        }
    }
    public const ulong TPT_MASK = 0x7FFFFF;
    public const sbyte INVALID = 0, LOWER = 1, UPPER = 2, EXACT = 3;
    public const int INFINITY = 999999999;
    public Move rootBestMove;
    public Transposition[] tpt = new Transposition[TPT_MASK + 1];
    public Board theBoard;
    public int[] pieceWeight = { 100, 320, 330, 500, 900, 20000 }; // P, N, B, R, Q, K
    public int[] pst = new int[384];
    public ulong[] packedPST =
    {
        9259542123273814144,
        12876550765177647794,
        9982954933006404234,
        9621248571257554309,
        9259542209508704384,
        9618411723462966149,
        9622655752112605829,
        9259542123273814144,
        5645370307605846094,
        6371608862222478424,
        7097825361929076834,
        7099238255929820514,
        7097830881046265954,
        7099232736812631394,
        6371608883781200984,
        5645370307605846094,
        7815564454513768044,
        8538966182894534774,
        8538971723570446454,
        8540379098454001014,
        8538977221128913014,
        8541791970896022134,
        8540373557778089334,
        7815564454513768044,
        9259542123273814144,
        9622655881464941189,
        8899254153084174459,
        8899254153084174459,
        8899254153084174459,
        8899254153084174459,
        8899254153084174459,
        9259542144832536704,
        7815564476072490604,
        8538966182894534774,
        8538971702011723894,
        8899259672201363579,
        9259547642391003259,
        8540379076895277174,
        8538971680452673654,
        7815564476072490604,
        7086511107012581474,
        7086511107012581474,
        7086511107012581474,
        7086511107012581474,
        7809912835393348204,
        8533314606891560054,
        10706323503566591124,
        10709149248450633364
    };

    public MyBot()
    {
        // Unpack PST
        int offset = 128;
        for (int i = 0; i < 8 * 6; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                ulong mask = (ulong)0b11111111 << (8 * (7 - (j % 8)));
                ulong val = (ulong)(packedPST[i] & mask) >> (8 * (7 - (j % 8)));
                int trueVal = (int)val - offset;
                pst[j + i * 8] = trueVal;

            }
        }
    }

    public int MoveScore(Move move)
    {
        int score = 0;

        if (move.IsCapture)
            return 10 * pieceWeight[(int)move.CapturePieceType - 1] - pieceWeight[(int)move.MovePieceType - 1];

        return score;
    }

    public int Evaluate()
    {
        int materialScore = 0;
        int activityScore = 0;
        PieceList[] pieceLists = theBoard.GetAllPieceLists();
        for (int i = 0; i < 12; i++)
        {
            materialScore += pieceLists[i].Count * pieceWeight[i % 6] * (pieceLists[i].IsWhitePieceList ? 1 : -1);
            for (int j = 0; j < pieceLists[i].Count; j++)
            {
                int file = pieceLists[i].GetPiece(j).Square.File; //column
                int rank = pieceLists[i].GetPiece(j).Square.Rank; //row
                int index = pieceLists[i].IsWhitePieceList ? file + (8 * (7 - rank)) : file + (8 * rank);
                activityScore += pst[index + 64 * (i % 6)] * (pieceLists[i].IsWhitePieceList ? 1 : -1);
            }
        }

        int eval = (materialScore + activityScore) * (theBoard.IsWhiteToMove ? 1 : -1);
        return eval;
    }

    public int Search(int alpha, int beta, int depth, bool root) // Fail-Soft Alpha Beta + Quiescent Search
    {
        if (theBoard.IsDraw())
            return 0;
        if (theBoard.IsInCheckmate())
            return -INFINITY; // should this be alpha? does it make a difference?

        ref Transposition tp = ref tpt[theBoard.ZobristKey & TPT_MASK];

        if (!root && tp.zKey == theBoard.ZobristKey && tp.depth >= depth)
        {
            if (tp.flag == EXACT) 
                return tp.eval;
            if (tp.flag == LOWER && tp.eval >= beta) 
                return tp.eval;
            if (tp.flag == UPPER && tp.eval <= alpha) 
                return tp.eval;
        }

        bool qsearch = depth <= 0;
        if (qsearch)
        {
            int standPat = Evaluate();
            if (standPat >= beta)
                return standPat;
            if (alpha < standPat)
                alpha = standPat;
            if (depth < -5) // cutoff after 5 consecutive captures to reduce time
                return alpha;
        }

        int startingAlpha = alpha;

        Move[] moves = theBoard.GetLegalMoves(qsearch);
        if (moves.Length == 0)
            return alpha;

        int bestScore = alpha;
        Move bestMove = moves[0];
        if (root)
            rootBestMove = bestMove;

        int[] moveScores = new int[moves.Length];
        for (int i = 0; i < moves.Length; i++)
            moveScores[i] = moves[i] == tp.move ? INFINITY : MoveScore(moves[i]);

        for (int i = 0; i < moves.Length; i++)
        {
            for (int j = i + 1; j < moves.Length; j++) // selection sort
            {
                if (moveScores[j] > moveScores[i])
                {
                    (moveScores[i], moveScores[j]) = (moveScores[j], moveScores[i]);
                    (moves[i], moves[j]) = (moves[j], moves[i]);
                }
            }

            theBoard.MakeMove(moves[i]);
            int score = -Search(-beta, -alpha, depth - 1, false);
            theBoard.UndoMove(moves[i]);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = moves[i];
                if (root)
                    rootBestMove = bestMove;
            }

            if (score >= beta)
                break; 
            if (score > alpha)
                alpha = score;
        }

        if (!qsearch)
        {
            tp.eval = bestScore;
            tp.zKey = theBoard.ZobristKey;
            tp.move = bestMove;
            tp.depth = (sbyte)depth;
            if (bestScore < startingAlpha)
                tp.flag = UPPER;
            else if (bestScore >= beta)
                tp.flag = LOWER;
            else 
                tp.flag = EXACT;
        }

        return bestScore;
    }

    public Move Think(Board board, Timer timer)
    {
        theBoard = board;

        int maxDepth = 4;
        int bestScore = Search(-INFINITY, INFINITY, maxDepth, true);

        //Console.WriteLine($"{rootBestMove.StartSquare.Name}{rootBestMove.TargetSquare.Name} | {bestScore}");

        return rootBestMove == Move.NullMove ? theBoard.GetLegalMoves()[0] : rootBestMove;
    }
}