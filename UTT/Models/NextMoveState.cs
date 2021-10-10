namespace UTT;

public class NextMoveState
{
    // Next move
    public int CellValue { get; set; }
    public int OuterRowIndex { get; set; }
    public int OuterColIndex { get; set; }
    public int InnerRowIndex { get; set; }
    public int InnerColIndex { get; set; }


    // Game state
    public BoardPosition NextBoardPosition { get; set; }
    public string PlayerTurn { get; set; }
    public GameStatus GameStatus { get; set; }
    public string GameWinner { get; set; }

    // Game board state
    public int GameBoardWinner { get; set; }
    public bool GameBoardIsFull { get; set; }

    // Board state
    public bool BoardIsFull { get; set; }
    public int BoardWinner { get; set; }
}
