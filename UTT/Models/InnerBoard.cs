using System;
using System.Diagnostics;

namespace UTT;

public class InnerBoard
{
    private int _remaining = 9;

    // 3x3 tic tac toe board
    public int[][] Cells { get; set; }

    public int Winner { get; set; }

    public bool IsFull => _remaining == 0;

    public InnerBoard()
    {
        Cells = new int[3][];
        for (var i = 0; i < 3; ++i)
        {
            Cells[i] = new int[3];
        }
    }

    public void Play(int row, int column, int value)
    {
        Cells[row][column] = value;

        CheckWinner();

        // We should never be able to get here if the slot isn't empty
        Debug.Assert(_remaining > 0);

        _remaining--;
    }

    private void CheckWinner()
    {
        if (Winner != 0)
        {
            return;
        }

        for (var i = 0; i < 3; ++i)
        {
            var h1 = Cells[i][0];
            var h2 = Cells[i][1];
            var h3 = Cells[i][2];

            // Horizontal
            if (h1 != 0 && h1 == h2 && h2 == h3)
            {
                Winner = h1;
                return;
            }

            var v1 = Cells[0][i];
            var v2 = Cells[1][i];
            var v3 = Cells[2][i];

            // Vertical
            if (v1 != 0 && v1 == v2 && v2 == v3)
            {
                Winner = v1;
                return;
            }
        }

        // 0,0    0,2
        //    1,1
        // 2,0    2,2

        var d1 = Cells[0][0];
        var d2 = Cells[1][1];
        var d3 = Cells[2][2];

        // Diagnoal
        if (d1 != 0 && d1 == d2 && d2 == d3)
        {
            Winner = d1;
            return;
        }

        var od1 = Cells[0][2];
        var od2 = Cells[1][1];
        var od3 = Cells[2][0];

        if (od1 != 0 && od1 == od2 && od2 == od3)
        {
            Winner = od1;
        }
    }
}
