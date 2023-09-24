using System;
using ChessChallenge.API;

// I think hash collisions are causing blunders ??!
// Packed PSTs are too large, need to use compressed 

public class MyBot : IChessBot
{
    struct Transposition
    {
        public ulong zKey = 0;
        public Move move = Move.NullMove;
        public int eval = 0;
        public sbyte 
            depth = -128,
            flag = 0; // INVALID = 0, LOWER = 1, UPPER = 2, EXACT = 3;

        public Transposition() { }
    }
    const ulong TPT_MASK = 0x7FFFFF;
    const int INFINITY = 999999999;
    Move rootBestMove;
    Transposition[] tpt = new Transposition[TPT_MASK + 1];
    Board theBoard;
    int[] 
        gamePhaseInc = { 0, 1, 1, 2, 4, 0 }, 
        middlegamePieceValue = { 82, 337, 365, 477, 1025, 0 },
        endgamePieceValue = { 94, 281, 297, 512, 936, 0 },
        middlegamePST = new int[384], 
        endgamePST = new int[384];

    ulong[] 
        packedMiddlegamePST =
        {
            9259542123273814144,
            16357001140413309557,
            8829195605423724908,
            8254401669090808169,
            7313418688563415655,
            7384914337435197812,
            6737222767702746730,
            9259542123273814144,
            11081220660097301,
            3987876604305901423,
            5889764664614570412,
            8615829970516021910,
            8323936949179225464,
            7599697423020169584,
            7154940515368202861,
            1687519861085072745,
            7170907476791297912,
            7390528431378371153,
            8117082644194436478,
            8972740228098131838,
            8830870139635010180,
            9263780805260055178,
            9552012216381055361,
            6880781611516057963,
            11577242485982338987,
            11214168400861567660,
            8904630919850999184,
            7527071450275215468,
            6658137190229968489,
            6009895851999132511,
            6084482357074295353,
            7886789834650901350,
            7241961428581395373,
            7519176848743963830,
            8318016057608023993,
            7306369598957387393,
            8603695488949846909,
            8251286653394652805,
            6735286635485691265,
            9182408126746812750,
            4582289962092626573,
            11348908854965197411,
            8617781306342151786,
            8028920214486217308,
            5728408685346316109,
            8246770769504399717,
            9333560970455320968,
            8188824274210166926
        },
        packedEndgamePST =
        {
            9259542123273814144,
            18374119105817541887,
            16061197207704294100,
            11572154847021273489,
            10198820791839654783,
            9549736231487373176,
            10198551484841362041,
            9259542123273814144,
            5069491205526864157,
            7455822975978661964,
            7524541400582942039,
            8035431733674084462,
            7960834280560624750,
            7601372000551791722,
            6227482657520642388,
            7155491318799420992,
            8244812703126220648,
            8681963116054150258,
            9401405507207856260,
            9045915848980202370,
            8828055359350013303,
            8394015409149540721,
            8245661556352184677,
            7599658875216755567,
            10199125451369908357,
            10055849173233928323,
            9765923324499426173,
            9548631222335405954,
            9477131093459761269,
            8971306245150767216,
            8825507717624329597,
            8611590021041063020,
            8617240532094257812,
            8040227885603724928,
            7820089199224394633,
            9481933937386764708,
            7970407823045732247,
            8099037312892111493,
            7667768075636792416,
            6873735846648900695,
            3917408671579997295,
            8399651535887701899,
            9984928491787627661,
            8689300325139585667,
            7961402723962161525,
            7889615596832196471,
            7310895313622170479,
            5430896352595961941
        };

    public MyBot()
    {
        // Unpack PST
        for (int i = 0; i < 48; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                int shift = (8 * (7 - (j % 8)));
                ulong 
                    mask = (ulong)0b11111111 << shift,
                    middlegameVal = (ulong)(packedMiddlegamePST[i] & mask) >> shift,
                    endgameVal = (ulong)(packedEndgamePST[i] & mask) >> shift;
                int
                    middlegameTrueVal = (int)middlegameVal - 128,
                    endgameTrueVal = (int)endgameVal - 128;
                middlegamePST[j + i * 8] = middlegameTrueVal;
                endgamePST[j + i * 8] = endgameTrueVal;
            }
        }
    }

    public int MoveScore(Move move)
    {
        int score = 0;

        if (move.IsCapture)
            score += 10 * middlegamePieceValue[(int)move.CapturePieceType - 1] - middlegamePieceValue[(int)move.MovePieceType - 1];
        if (move.IsPromotion)
            score += middlegamePieceValue[(int)move.PromotionPieceType - 1] - 100;

        return score;
    }

    public int Evaluate()
    {
        int gamePhase = 0;
        int middlegameScore = 0;
        int endgameScore = 0;

        PieceList[] pieceLists = theBoard.GetAllPieceLists();
        for (int i = 0; i < 12; i++)
        {
            for (int j = 0; j < pieceLists[i].Count; j++)
            {
                int file = pieceLists[i].GetPiece(j).Square.File, 
                    rank = pieceLists[i].GetPiece(j).Square.Rank,
                    index = pieceLists[i].IsWhitePieceList ? file + (8 * (7 - rank)) : file + (8 * rank);

                middlegameScore += (middlegamePieceValue[i % 6] + middlegamePST[index + 64 * (i % 6)]) * (pieceLists[i].IsWhitePieceList ? 1 : -1);
                endgameScore += (endgamePieceValue[i % 6] + endgamePST[index + 64 * (i % 6)]) * (pieceLists[i].IsWhitePieceList ? 1 : -1);
                gamePhase += gamePhaseInc[i % 6];
            }
        }

        // Tapered Eval
        int middlegamePhase = Math.Min(24, gamePhase); // in case of early promotion
        int endgamePhase = 24 - middlegamePhase;
        return ((middlegameScore * middlegamePhase + endgameScore * endgamePhase) / 24) * (theBoard.IsWhiteToMove ? 1 : -1);
    }

    /*
     * Fail-Soft Alpha Beta 
     * Quiescent Search 
     * Transposition Tables
     * Move Ordering
     */
    public int Search(int alpha, int beta, int depth, bool root)
    {
        if (theBoard.IsDraw())
            return 0;
        if (theBoard.IsInCheckmate())
            return -INFINITY; // should this be alpha? does it make a difference?

        ref Transposition tp = ref tpt[theBoard.ZobristKey & TPT_MASK];

        if (!root && tp.zKey == theBoard.ZobristKey && tp.depth >= depth)
            if (tp.flag == 3 || (tp.flag == 1 && tp.eval >= beta) || (tp.flag == 2 && tp.eval <= alpha)) 
                return tp.eval;

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
            tp.flag = (sbyte)(bestScore < startingAlpha ? 2 : bestScore >= beta ? 1 : 3);
        }

        return bestScore;
    }

    public Move Think(Board board, Timer timer)
    {
        theBoard = board;

        int maxDepth = BitboardHelper.GetNumberOfSetBits(theBoard.AllPiecesBitboard) < 7 ? 9 : 6;
        for (int i = 1; i < maxDepth; i++)
            Search(-INFINITY, INFINITY, i, true);

        return rootBestMove == Move.NullMove ? theBoard.GetLegalMoves()[0] : rootBestMove;
    }
}