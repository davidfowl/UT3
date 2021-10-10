namespace UTT;

public struct BoardPosition
{
    public int Column { get; }
    public int Row { get; }

    public bool IsValid => Column != -1 && Row != -1;

    public BoardPosition(int row, int col)
    {
        Row = row;
        Column = col;
    }
}