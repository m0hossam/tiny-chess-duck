using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Challenge.src.My_Bot
{
    // To use, copy and paste the pieceSquareTable and the Algorithm to MyBot.Think()
    internal class CompressingPST
    {
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

        public void Algorithm()
        {
            // Compression
            bool ok = true;
            ulong[] compressed = new ulong[8 * 6];

            int offset = 128; //8bit offset to use unsigned
            ulong res = 0;
            for (int i = 0; i < 64 * 6; i++)
            {
                ulong val = (ulong)(pieceSquareTable[i] + offset) << (8 * (7 - (i % 8)));
                ulong mask = (ulong)0b11111111 << (8 * (7 - (i % 8)));
                res &= ~mask; //clear 8bit place to be set
                res |= val; //set 8bit place to val

                //Console.WriteLine(val);

                if ((i + 1) % 8 == 0) //output 64bit number and reset
                {
                    //Console.WriteLine($"\n{res}\n\n");
                    compressed[i / 8] = res;
                    res = 0;
                }
            }

            // Decompression
            for (int i = 0; i < 8 * 6; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    ulong mask = (ulong)0b11111111 << (8 * (7 - (j % 8)));
                    ulong val = (ulong)(compressed[i] & mask) >> (8 * (7 - (j % 8)));
                    int trueVal = (int)val - offset;
                    //Console.Write($"{trueVal}, ");
                    if (trueVal != pieceSquareTable[j + i * 8])
                        ok = false;
                }
                //Console.WriteLine();
            }

            Console.WriteLine(ok ? "PASSED" : "FAILED");
            // End
        }
    }
}
