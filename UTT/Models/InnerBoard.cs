using System;

namespace UTT
{
    public class InnerBoard
    {
        // 3x3 tic tac toe board
        public int[][] Cells { get; set; }

        public string Winner { get; set; }

        public InnerBoard()
        {
            Cells = new int[3][];
            for (var i = 0; i < 3; ++i)
            {
                Cells[i] = new int[3];
            }
        }

        public void Play(int row, int column, int value, Func<int, string> getPlayer)
        {
            Cells[row][column] = value;
            
            CheckWinner(getPlayer);
        }

        private void CheckWinner(Func<int, string> getPlayer)
        {
            for (var i = 0; i < 3; ++i)
            {
                // Horizontal
                if (Cells[i][0] != 0 && Cells[i][0] == Cells[i][1] && Cells[i][1] == Cells[i][2])
                {
                    Winner = getPlayer(Cells[i][0]);
                    return;
                }

                // Vertical
                if (Cells[0][i] != 0 && Cells[0][i] == Cells[1][i] && Cells[1][i] == Cells[2][i])
                {
                    Winner = getPlayer(Cells[0][i]);
                    return;
                }
            }

            // 0,0    0,2
            //    1,1
            // 2,0    2,2

            // Diagnoal
            if (Cells[0][0] != 0 && Cells[0][0] == Cells[1][1] && Cells[1][1] == Cells[2][2])
            {
                Winner = getPlayer(Cells[0][0]);
                return;
            }

            if (Cells[0][2] != 0 && Cells[0][2] == Cells[1][1] && Cells[1][1] == Cells[2][0])
            {
                Winner = getPlayer(Cells[0][2]);
            }
        }
    }
}