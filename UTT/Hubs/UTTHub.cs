using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace UTT
{
    public class UTTHub : Hub
    {
        public string UserName => Context.User.Identity.Name;

        public override async Task OnConnectedAsync()
        {
            User.AddUser(UserName);

            await Clients.All.SendAsync("GameUpdated", Game.GetGames());
            await Clients.All.SendAsync("UsersChanged", User.GetUsers());
        }

        public Task SendToLobby(string message)
        {
            return Clients.All.SendAsync("LobbyMessage", UserName, message);
        }

        public Task CreateGame(string name)
        {
            Game.CreateGame(UserName, name);
            return Clients.All.SendAsync("GameUpdated", Game.GetGames());
        }

        public Task JoinGame(int id)
        {
            Game.JoinGame(id, UserName);
            return Clients.All.SendAsync("GameUpdated", Game.GetGames());
        }

        public Task PlayTurn(int id, int outerRowIndex, int outerColIndex, int innerRowIndex, int innerColIndex)
        {
            if (Game.PlayTurn(UserName,
                              id,
                              outerRowIndex,
                              outerColIndex,
                              innerRowIndex,
                              innerColIndex,
                              out var nextMove))
            {
                return Clients.All.SendAsync("PlayMove", id, nextMove);
            }
            return Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            // TODO: Handle stopping games that include this user

            User.Remove(UserName);

            return Clients.All.SendAsync("UsersChanged", User.GetUsers());
        }
    }

    public class User
    {
        public string Name { get; set; }

        private static ConcurrentDictionary<string, User> _users = new ConcurrentDictionary<string, User>();

        public static void AddUser(string user)
        {
            _users.TryAdd(user, new User
            {
                Name = user
            });
        }

        public static void Remove(string user)
        {
            _users.TryRemove(user, out _);
        }

        public static IEnumerable<User> GetUsers() => _users.Values;
    }

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

            // Get the next board to check it for validity
            var nextBoard = Board.Boards[innerRowIndex][innerColIndex];

            if (nextBoard.Winner != null)
            {
                // The next board is full so the board is open again
                nextMove.NextBoardPosition = new BoardPosition(-1, -1);
            }
            else
            {
                nextMove.NextBoardPosition = new BoardPosition(innerRowIndex, innerColIndex);
            }

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

    public class NextMoveState
    {
        public BoardPosition NextBoardPosition { get; set; }
        public int CellValue { get; set; }
        public string BoardWinner { get; set; }
        public string PlayerTurn { get; set; }
        public string Winner { get; set; }
        public int OuterRowIndex { get; set; }
        public int OuterColIndex { get; set; }
        public int InnerRowIndex { get; set; }
        public int InnerColIndex { get; set; }
    }

    public enum GameStatus
    {
        Waiting,
        Playing,
        Completed
    }
}