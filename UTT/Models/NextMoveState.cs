namespace UTT
{
    public class NextMoveState
    {
        public BoardPosition NextBoardPosition { get; set; }
        public bool IsFull { get; set; }
        public int CellValue { get; set; }
        public string BoardWinner { get; set; }
        public string PlayerTurn { get; set; }
        public string Winner { get; set; }
        public int OuterRowIndex { get; set; }
        public int OuterColIndex { get; set; }
        public int InnerRowIndex { get; set; }
        public int InnerColIndex { get; set; }
    }
}