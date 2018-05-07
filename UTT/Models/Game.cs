using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace UTT
{
    public class Game
    {
        private static ConcurrentDictionary<int, Game> _games = new ConcurrentDictionary<int, Game>();
        private static int _id;

        public int Id { get; set; }
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public GameStatus Status { get; set; }
        public string Name { get; set; }
        public string PlayerTurn { get; set; }
        public OuterBoard Board { get; set; }
        public BoardPosition NextBoardPosition { get; set; } = new BoardPosition(-1, -1);

        public bool Play(
            string player,
            int outerRowIndex,
            int outerColIndex,
            int innerRowIndex,
            int innerColIndex,
            out NextMoveState nextMove)
        {
            var board = Board.Boards[outerRowIndex][outerColIndex];
            var expectedBoard = NextBoardPosition.IsValid ? Board.Boards[NextBoardPosition.Row][NextBoardPosition.Column] : null;
            var cell = board.Cells[innerRowIndex][innerColIndex];

            nextMove = new NextMoveState();
            nextMove.OuterRowIndex = outerRowIndex;
            nextMove.OuterColIndex = outerColIndex;
            nextMove.InnerRowIndex = innerRowIndex;
            nextMove.InnerColIndex = innerColIndex;
            nextMove.NextBoardPosition = default(BoardPosition);
            nextMove.PlayerTurn = null;
            nextMove.CellValue = 0;
            nextMove.Winner = null;
            nextMove.BoardWinner = null;

            if (cell != 0)
            {
                // Can't play over an existing cell
                return false;
            }

            if (expectedBoard != null && board != expectedBoard)
            {
                // Invalid move, the next player has to play in the specific board
                return false;
            }

            if (board.Winner != null)
            {
                // This board is done
                return false;
            }

            nextMove.CellValue = GetPlayerValue(player);

            switch (nextMove.CellValue)
            {
                case 0:
                    return false;
                case 1:
                    nextMove.PlayerTurn = Player2;
                    break;
                case 2:
                    nextMove.PlayerTurn = Player1;
                    break;
            }

            board.Play(innerRowIndex, innerColIndex, nextMove.CellValue, GetPlayer);

            nextMove.NextBoardPosition = new BoardPosition(innerRowIndex, innerColIndex);

            Board.CheckWinner();

            if (Board.Winner != null)
            {
                Status = GameStatus.Completed;
            }

            nextMove.Winner = Board.Winner;
            nextMove.BoardWinner = board.Winner;
            PlayerTurn = nextMove.PlayerTurn;
            NextBoardPosition = nextMove.NextBoardPosition;

            return true;
        }

        private string GetPlayer(int value)
        {
            return value == 1 ? Player1 : Player2;
        }

        private int GetPlayerValue(string player)
        {
            if (player != Player1 && player != Player2)
            {
                // Spectators can't play
                return 0;
            }

            if (player != PlayerTurn)
            {
                // Not the correct player's turn
                return 0;
            }

            return player == Player1 ? 1 : 2;
        }

        public static bool PlayTurn(string player,
                                    int id, int outerRowIndex,
                                    int outerColIndex,
                                    int innerRowIndex,
                                    int innerColIndex,
                                    out NextMoveState nextMove)
        {
            nextMove = null;

            if (!_games.TryGetValue(id, out var game))
            {
                return false;
            }

            if (game.Status == GameStatus.Completed)
            {
                return false;
            }

            return game.Play(player, outerRowIndex, outerColIndex, innerRowIndex, innerColIndex, out nextMove);
        }

        public static void CreateGame(string player, string name)
        {
            var id = Interlocked.Increment(ref _id);
            var game = new Game
            {
                Id = id,
                Player1 = player,
                Name = name,
                Status = GameStatus.Waiting,
                PlayerTurn = player,
                Board = new OuterBoard()
            };

            _games.TryAdd(id, game);
        }

        public static void JoinGame(int id, string userName)
        {
            if (_games.TryGetValue(id, out var game) && game.Player2 == null)
            {
                game.Player2 = userName;
                game.Status = GameStatus.Playing;
            }
        }

        public static IEnumerable<Game> GetGames() => _games.Values;
    }
}