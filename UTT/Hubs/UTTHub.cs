using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

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
            if (Game.PlayTurn(UserName, id, outerRowIndex, outerColIndex, innerRowIndex, innerColIndex, out int value, out var playerTurn))
            {
                return Clients.All.SendAsync("PlayMove", id, outerRowIndex, outerColIndex, innerRowIndex, innerColIndex, value, playerTurn);
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
        private static ConcurrentDictionary<string, string> _users = new ConcurrentDictionary<string, string>();

        public static void AddUser(string user)
        {
            _users.TryAdd(user, user);
        }

        public static void Remove(string user)
        {
            _users.TryRemove(user, out _);
        }

        public static IEnumerable<string> GetUsers() => _users.Values;
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

        public bool Play(string player, int outerRowIndex, int outerColIndex, int innerRowIndex, int innerColIndex, out int value, out string playerTurn)
        {
            value = GetPlayerValue(player);

            ref var cell = ref Board.Boards[outerRowIndex][outerColIndex].Cells[innerRowIndex][innerColIndex];
            playerTurn = null;

            switch (value)
            {
                case 0:
                    return false;
                case 1:
                    playerTurn = Player2;
                    break;
                case 2:
                    playerTurn = Player1;
                    break;
            }

            PlayerTurn = playerTurn;
            cell = value;
            return true;
        }

        private int GetPlayerValue(string player)
        {
            if (player != Player1 && player != Player2)
            {
                return 0;
            }

            if (player != PlayerTurn)
            {
                return 0;
            }

            return player == Player1 ? 1 : 2;
        }

        public static bool PlayTurn(string player, int id, int outerRowIndex, int outerColIndex, int innerRowIndex, int innerColIndex, out int value, out string playerTurn)
        {
            value = -1;
            playerTurn = null;
            if (!_games.TryGetValue(id, out var game))
            {
                return false;
            }

            return game.Play(player, outerRowIndex, outerColIndex, innerRowIndex, innerColIndex, out value, out playerTurn);
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

    public class OuterBoard
    {
        // 3x3 inner board
        public InnerBoard[][] Boards { get; set; }

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
    }

    public class InnerBoard
    {
        // 3x3 tic tac toe board
        public int[][] Cells { get; set; }
        public InnerBoard()
        {
            Cells = new int[3][];
            for (var i = 0; i < 3; ++i)
            {
                Cells[i] = new int[3];
            }
        }
    }

    public enum GameStatus
    {
        Waiting,
        Playing,
        Completed
    }
}