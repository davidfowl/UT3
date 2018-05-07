namespace UTT
{
    public class OuterBoard
    {
        // 3x3 inner board
        public InnerBoard[][] Boards { get; set; }

        public string Winner { get; set; }

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

        public void CheckWinner()
        {
            for (var i = 0; i < 3; ++i)
            {
                // Horizontal
                if (Boards[i][0].Winner != null && Boards[i][0].Winner == Boards[i][1].Winner && Boards[i][1].Winner == Boards[i][2].Winner)
                {
                    Winner = Boards[i][0].Winner;
                    return;
                }

                // Vertical
                if (Boards[0][i].Winner != null && Boards[0][i].Winner == Boards[1][i].Winner && Boards[1][i].Winner == Boards[2][i].Winner)
                {
                    Winner = Boards[0][i].Winner;
                    return;
                }
            }

            // 0,0    0,2
            //    1,1
            // 2,0    2,2

            // Diagnoal
            if (Boards[0][0].Winner != null && Boards[0][0].Winner == Boards[1][1].Winner && Boards[1][1].Winner == Boards[2][2].Winner)
            {
                Winner = Boards[0][0].Winner;
                return;
            }

            if (Boards[0][2].Winner != null && Boards[0][2].Winner == Boards[1][1].Winner && Boards[1][1].Winner == Boards[2][0].Winner)
            {
                Winner = Boards[0][2].Winner;
            }
        }
    }
}