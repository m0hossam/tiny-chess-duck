using ChessChallenge.API;
using System;

namespace ChessChallenge.Example
{
    //TinyChessDuck 
    public class EvilBot : IChessBot
    {
        public const int infinity = 999999999;
        public Board theBoard;
        public Timer theTimer; //might be needed in the future (REMOVE IF UNNECESSARY)
        public bool compressed = true;
        public int[] pieceWeight = { 100, 320, 330, 500, 900, 20000 }; // P, N, B, R, Q, K
        public int[] PST = new int[384];
        public ulong[] compressedPST =
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

        public int Evaluate()
        {
            if (theBoard.IsDraw())
                return 0;
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
                    activityScore += PST[index + 64 * (i % 6)] * (pieceLists[i].IsWhitePieceList ? 1 : -1);
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
            return (pieceWeight[(int)moveB.CapturePieceType - 1] - pieceWeight[(int)moveB.MovePieceType - 1])
                - (pieceWeight[(int)moveA.CapturePieceType - 1] - pieceWeight[(int)moveA.MovePieceType - 1]);
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
            if (theBoard.IsDraw())
                return 0;
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

            // Unpack PST
            if (compressed)
            {
                compressed = false;
                int offset = 128;
                for (int i = 0; i < 8 * 6; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        ulong mask = (ulong)0b11111111 << (8 * (7 - (j % 8)));
                        ulong val = (ulong)(compressedPST[i] & mask) >> (8 * (7 - (j % 8)));
                        int trueVal = (int)val - offset;
                        PST[j + i * 8] = trueVal;

                    }
                }
            }

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
                //Console.WriteLine($"Move: {move.StartSquare.Name}{move.TargetSquare.Name} | Score: {score}");
            }
            //Console.WriteLine($"Best Move: {bestMove.StartSquare.Name}{bestMove.TargetSquare.Name} | Best Score: {bestScore}\n");
            return bestMove;
        }
    }
}

