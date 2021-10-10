using System.Diagnostics;

namespace UTT;

public class OuterBoard
{
    private int _remaining = 9;

    // 3x3 inner board
    public InnerBoard[][] Boards { get; set; }

    public int Winner { get; set; }

    public bool IsFull => _remaining == 0;

    public OuterBoard()
    {
        Boards = new InnerBoard[3][];
        for (var i = 0; i < 3; ++i)
        {
            Boards[i] = new InnerBoard[3];
            for (var j = 0; j < 3; ++j)
            {
                Boards[i][j] = new InnerBoard();
            }
        }
    }

    public void RemoveBoard()
    {
        Debug.Assert(_remaining > 0);

        _remaining--;
    }

    public void CheckWinner()
    {
        if (Winner != 0)
        {
            return;
        }

        for (var i = 0; i < 3; ++i)
        {
            var h1 = Boards[i][0];
            var h2 = Boards[i][1];
            var h3 = Boards[i][2];

            // Horizontal
            if (h1.Winner != 0 && h1.Winner == h2.Winner && h2.Winner == h3.Winner)
            {
                Winner = h1.Winner;
                return;
            }

            var v1 = Boards[0][i];
            var v2 = Boards[1][i];
            var v3 = Boards[2][i];

            // Vertical
            if (v1.Winner != 0 && v1.Winner == v2.Winner && v2.Winner == v3.Winner)
            {
                Winner = v1.Winner;
                return;
            }
        }

        // 0,0    0,2
        //    1,1
        // 2,0    2,2

        var d1 = Boards[0][0];
        var d2 = Boards[1][1];
        var d3 = Boards[2][2];

        // Diagnoal
        if (d1.Winner != 0 && d1.Winner == d2.Winner && d2.Winner == d3.Winner)
        {
            Winner = d1.Winner;
            return;
        }

        var od1 = Boards[0][2];
        var od2 = Boards[1][1];
        var od3 = Boards[2][0];

        if (od1.Winner != 0 && od1.Winner == od2.Winner && od2.Winner == od3.Winner)
        {
            Winner = od1.Winner;
        }
    }
}
