using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace UTT;

public class Game
{
    // How long should we allow a new game to go without starting
    private static TimeSpan WaitingGameLimit = TimeSpan.FromMinutes(10);

    // How long should we allow a game to go without playing a move
    private static TimeSpan PlayingWithoutMoveLimit = TimeSpan.FromMinutes(5);

    // How long should game stick around after completing
    private static TimeSpan EndedGameLimit = TimeSpan.FromMinutes(5);

    private static ConcurrentDictionary<int, Game> _games = new ConcurrentDictionary<int, Game>();
    private static int _id;

    private DateTimeOffset _lastMovePlayedAt = DateTimeOffset.UtcNow;
    private DateTimeOffset _endedAt;

    public int Id { get; set; }
    public string Player1 { get; set; }
    public string Player2 { get; set; }
    public GameStatus Status { get; set; }
    public string Winner => Board.Winner == 0 ? null : GetPlayer(Board.Winner);
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

        if (board.IsFull)
        {
            // This board is done
            return false;
        }

        nextMove.CellValue = GetPlayerValue(player);
        string playerTurn = null;

        switch (nextMove.CellValue)
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

        // Record the last time played
        _lastMovePlayedAt = DateTimeOffset.UtcNow;

        board.Play(innerRowIndex, innerColIndex, nextMove.CellValue);

        if (board.IsFull)
        {
            // Remove a board if we can no longer play here
            Board.RemoveBoard();
        }

        var nextBoard = Board.Boards[innerRowIndex][innerColIndex];
        if (nextBoard.IsFull || nextBoard.Winner != 0)
        {
            NextBoardPosition = new BoardPosition(-1, -1);
        }
        else
        {
            NextBoardPosition = new BoardPosition(innerRowIndex, innerColIndex);
        }

        Board.CheckWinner();

        if (Winner != null || Board.IsFull)
        {
            MarkCompleted();
        }

        nextMove.OuterRowIndex = outerRowIndex;
        nextMove.OuterColIndex = outerColIndex;
        nextMove.InnerRowIndex = innerRowIndex;
        nextMove.InnerColIndex = innerColIndex;
        nextMove.PlayerTurn = playerTurn;
        nextMove.NextBoardPosition = NextBoardPosition;

        nextMove.GameStatus = Status;
        nextMove.GameWinner = Winner;

        nextMove.GameBoardWinner = Board.Winner;
        nextMove.GameBoardIsFull = Board.IsFull;

        nextMove.BoardIsFull = board.IsFull;
        nextMove.BoardWinner = board.Winner;

        PlayerTurn = playerTurn;

        return true;
    }

    private void MarkCompleted()
    {
        Status = GameStatus.Completed;
        _endedAt = DateTimeOffset.UtcNow;
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

    public static void Initialize(IConfiguration configuration)
    {
        if (TryParseMinutes(configuration["UTT:WaitingGameLimit"], out var timeSpan))
        {
            WaitingGameLimit = timeSpan;
        }

        if (TryParseMinutes(configuration["UTT:PlayingWithoutMoveLimit"], out timeSpan))
        {
            PlayingWithoutMoveLimit = timeSpan;
        }

        if (TryParseMinutes(configuration["UTT:EndedGameLimit"], out timeSpan))
        {
            EndedGameLimit = timeSpan;
        }
    }

    private static bool TryParseMinutes(string value, out TimeSpan timeSpan)
    {
        timeSpan = TimeSpan.Zero;
        if (!string.IsNullOrEmpty(value) && int.TryParse(value, out var intVal))
        {
            timeSpan = TimeSpan.FromMinutes(intVal);
            return true;
        }
        return false;
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

    public static bool MarkExpiredGames()
    {
        var now = DateTimeOffset.UtcNow;
        var stateChanged = false;

        foreach (var game in _games)
        {
            var elapsed = now.Subtract(game.Value._lastMovePlayedAt);

            // PlayingWithoutMoveLimit without playing then mark the game as completed
            if (game.Value.Status == GameStatus.Playing && elapsed > PlayingWithoutMoveLimit)
            {
                game.Value.MarkCompleted();
                stateChanged = true;
            }

            // If the player has been waiting for more than WaitingGameLimit minutes for somebody to join the game
            // mark it as completed
            if (game.Value.Status == GameStatus.Waiting && elapsed > WaitingGameLimit)
            {
                game.Value.MarkCompleted();
                stateChanged = true;
            }
        }
        return stateChanged;
    }

    public static int RemoveCompletedGames()
    {
        var removed = 0;
        var now = DateTimeOffset.UtcNow;

        foreach (var game in _games)
        {
            // Remove games that ended for > 5 minutes
            if (game.Value.Status == GameStatus.Completed &&
                now.Subtract(game.Value._endedAt) > EndedGameLimit)
            {
                _games.TryRemove(game.Key, out _);
                removed++;
            }
        }

        return removed;
    }
}
