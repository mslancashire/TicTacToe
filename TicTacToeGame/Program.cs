using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Collections.Frozen;
using System.Buffers;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        var game = Game.CreateGame(new GameConsoleIO(LogType.All));

        // game loop
        while (true)
        {
            game.BeginTurn();
            game.AddOpponentsMove();
            game.ReadValidMoves();
            game.EndTurn();
        }
    }
}

public class Game
{
    public bool IsDebug = true;
    public LogType EnabledLogs = LogType.All;

    private IGameIO _gameIO;

    public static Game CreateGame(IGameIO gameIO)
        => new(gameIO);

    private Game(IGameIO gameIO)
    {
        _gameIO = gameIO;
        BuildBoards();
    }

    public void SetGameIO(IGameIO gameIO)
        => _gameIO = gameIO;

    public List<Turn> Turns { get; private set; } = [];

    /// <summary>
    /// Starts a new turn by adding a new Turn record to the Turns list with the turn number and empty moves. The opponent's move and my move will be added to this record as the turn progresses.
    /// </summary>
    public void BeginTurn()
    {
        var turn = new Turn(Turns.Count + 1, null, null, null);
        Turns.Add(turn);
    }

    /// <summary>
    /// Gets the current turn in the sequence of turns.
    /// </summary>
    public Turn CurrentTurn => Turns.LastOrDefault();

    /// <summary>
    /// Processes and records the opponent's most recent move.
    /// </summary>
    public void AddOpponentsMove()
    {
        var opponentsMove = Position.ParseNextMove(_gameIO);

        _gameIO.LogInfo($"My opponents move => `{opponentsMove.GetTileCode()}`, `{opponentsMove}`", LogType.Turn);

        if (opponentsMove.IsValid())
        {
            ChangeTileOwner(opponentsMove, PlayerType.Opponent);
        }

        Turns[^1] = CurrentTurn with { OpponentsLastMove = opponentsMove };
    }

    /// <summary>
    /// Reads the valid moves for the current turn from the game input.
    /// </summary>
    public void ReadValidMoves()
    {
        var validMoves = new List<Position>();
        int validActionCount = int.Parse(_gameIO.ReadInput());

        _gameIO.LogInfo($"Valid moves => {validActionCount}.", LogType.Turn);

        for (int i = 0; i < validActionCount; i++)
        {
            var validMove = Position.ParseNextMove(_gameIO);
            validMoves.Add(validMove);

            _gameIO.LogInfo($"Valid move => `{validMove.GetTileCode()}`, `{validMove}`.", LogType.ValidMoves);
        }

        Turns[^1] = CurrentTurn with { ValidMoves = validMoves };
    }

    /// <summary>
    /// Records the player's move and updates the game state accordingly.
    /// </summary>
    /// <param name="myMove"></param>
    public void AddMyMove(Position myMove)
    {
        ChangeTileOwner(myMove, PlayerType.Me);

        Turns[^1] = CurrentTurn with { MyMove = myMove };
    }

    public HashSet<Tile> Tiles { get; private set; }

    public List<Board> Boards { get; private set; }

    private void BuildBoards()
    {
        var tileSize = 9;
        var tileSet = Enumerable.Range(0, tileSize);
        Tiles = new(tileSet
            .SelectMany(r => tileSet
                .Select(c => new Tile(new Position(r, c)))));

        var boardSize = 3;
        var boardSet = Enumerable.Range(0, boardSize);
        Boards = new(boardSet
            .SelectMany(r => boardSet
                .Select(c => new Board(new Position(r, c), _gameIO))));

        var boardIndex = 0;
        var rowMin = 0;
        var colMin = 0;
        for (var rowMax = 2; rowMax <= 8; rowMax += boardSize)
        {
            for (var colMax = 2; colMax <= 8; colMax += boardSize)
            {
                var tiles = Tiles.Where(t => t.Position.For(rowMin, rowMax, colMin, colMax));
                Boards[boardIndex].AllocateTiles(tiles);

                colMin = colMax + 1;
                boardIndex++;
            }

            rowMin = rowMax + 1;
            colMin = 0;
        }
    }

    public void ChangeTileOwner(Position position, PlayerType owner)
        => GetBoard(position).ChangeTileOwner(position, owner);

    public Board GetBoard(Position position)
        => Boards.First(b => b.Tiles.Any(t => t.Position == position));

    public IEnumerable<(Board BestBoard, (int Rating, Tile BestTile) RatedTile)> RatedBoards()
        => Boards.Select(b => (b, b.GetBestTile()));

    private static readonly List<WinCondition> _winConditions =
    [
        new([TileCode.TL, TileCode.TM, TileCode.TR]),
        new([TileCode.ML, TileCode.MM, TileCode.MR]),
        new([TileCode.BL, TileCode.BM, TileCode.BR]),
        new([TileCode.TL, TileCode.ML, TileCode.BL]),
        new([TileCode.TM, TileCode.MM, TileCode.BM]),
        new([TileCode.TR, TileCode.MR, TileCode.BR]),
        new([TileCode.TL, TileCode.MM, TileCode.BR]),
        new([TileCode.TR, TileCode.MM, TileCode.BL]),
    ];

    public static List<WinCondition> Winners = _winConditions;

    public Position GetBestMove()
    {
        var currentTurn = CurrentTurn;

        if (currentTurn.Number == 1)
        {
            return new Position(4, 4);
        }

        var ratedBoards = RatedBoards().ToList();
        foreach (var ratedBoard in ratedBoards)
        {
            _gameIO.LogInfo($"Board {ratedBoard.BestBoard.Code} has best tile of {ratedBoard.RatedTile.BestTile?.Code} rated at {ratedBoard.RatedTile.Rating}.", LogType.Board);
        }

        var board = Boards.First(b => b.Code == currentTurn.OpponentsLastMove.GetTileCode());
        var validBoard = board.Tiles.Any(t => currentTurn.ValidMoves.Contains(t.Position));

        if (validBoard == false)
        {
            var bestRatedBoard = ratedBoards
                .OrderByDescending(rb => rb.RatedTile.Rating)
                .First();

            _gameIO.LogInfo($"Opponent selected an invalid board of {board.Code}.", LogType.Selection);

            return bestRatedBoard.RatedTile.BestTile.Position;
        }

        _gameIO.LogInfo($"Opponent selected a valid board of {board.Code}.", LogType.Selection);
        var (rating, bestTile) = board.GetBestTile();
        if (bestTile is null)
        {
            _gameIO.LogInfo($"No best tile found for board {board.Code}, use first valid.", LogType.Selection);
            return currentTurn.ValidMoves.First();
        }

        // if we have a winning move or a blocking move, use it even if the opponent selected the board
        if (rating > 50)
        {
            _gameIO.LogInfo($"Using winning/blocking move of {bestTile.Code} for rated at {rating}.", LogType.Selection);
            return bestTile.Position;
        }

        // avoid using a move that puts the opponent in a win / blocking position
        foreach (var ratedBoard in ratedBoards.OrderBy(rb => rb.RatedTile.Rating))
        {
            if (currentTurn.ValidMoves.Contains(ratedBoard.BestBoard.Position))
            {
                _gameIO.LogInfo($"Using best board selection of {ratedBoard.BestBoard.Code}, rated at {ratedBoard.RatedTile.Rating} for {ratedBoard.RatedTile.BestTile.Code} to avoid opponent win/block.", LogType.Selection);
                return ratedBoard.RatedTile.BestTile.Position;
            }
        }

        _gameIO.LogInfo($"No good move found, using first valid.", LogType.Selection);
        return currentTurn.ValidMoves.First();
    }

    public void EndTurn()
    {
        var myMove = GetBestMove();
        myMove.MakeMove(_gameIO);
        AddMyMove(myMove);

        _gameIO.LogInfo($"My move => `{myMove.GetTileCode()}`, `{myMove}`", LogType.Selection);
        _gameIO.LogInfo($"End of turn {CurrentTurn.Number}.", LogType.Turn);
    }
}

public record Board
{
    private readonly IGameIO _gameIO;

    public Board(Position position, IGameIO gameIO)
    {
        _gameIO = gameIO;

        Code = position.GetTileCode();
        Position = position;
        _winConditions = [.. Game.Winners.Select(w => w with { })];
    }

    private readonly HashSet<WinCondition> _winConditions;

    public TileCode Code { get; }

    public Position Position { get; }

    public HashSet<Tile> Tiles { get; private set; }

    public void AllocateTiles(IEnumerable<Tile> tiles)
        => Tiles = [.. tiles];

    public void ChangeTileOwner(Position position, PlayerType owner)
    {
        Tiles
            .First(t => t.Position == position)
            .ChangeOwner(owner);

        foreach (var winCondition in _winConditions)
        {
            winCondition.ChangeOwner(position.GetTileCode(), owner);
        }
    }

    public Tile GetWinningTile(PlayerType owner)
    {
        var winCondition = _winConditions
            .FirstOrDefault(wc => wc.IsStillWinnableBy(owner));

        if (winCondition == null)
        {
            return null;
        }

        _gameIO.LogInfo($"Found a win condition for {owner}, {string.Join(',', winCondition.Conditions)}, {winCondition}.", LogType.Selection);

        var emptyTiles = Tiles.Where(t => t.Owner == PlayerType.None);
        _gameIO.LogInfo($"Selecting from {string.Join(',', emptyTiles.Select(t => t.Code))} empty tiles.", LogType.Selection);

        var winningTile = emptyTiles.FirstOrDefault(t => winCondition.Conditions.Contains(t.Code));
        _gameIO.LogInfo($"Selected winning tile of `{winningTile.Code}`.", LogType.Selection);

        return winningTile;
    }

    internal static Tile GetBestOfTheRest(Board board)
    {
        var possibleWinConditions = board._winConditions
            .Where(wc => wc.Opponents == 0);

        var bestRatedTiles = board.Tiles
            .Where(t => t.Owner == PlayerType.None)
            .Select(t => new { Count = possibleWinConditions.Count(wt => wt.Conditions.Contains(t.Code)), Tile = t })
            .GroupBy(g => g.Count, v => v, (g, v) => new { Count = g, Tiles = v.ToList() });

        var bestTiles = bestRatedTiles.OrderByDescending(bt => bt.Count).First();

        var rnd = new Random();
        return bestTiles.Tiles[rnd.Next(bestTiles.Tiles.Count)].Tile;
    }

    internal static Tile GetRandom(Board board)
    {
        var rnd = new Random();

        var emptyTiles = board.Tiles
            .Where(t => t.Owner == PlayerType.None)
            .ToList();
        var pick = rnd.Next(emptyTiles.Count);

        return emptyTiles[pick];
    }

    private static IEnumerable<(int Rating, Func<Board, Tile> Rule)> _ratedRules =
    [
        (200, b => b.GetWinningTile(PlayerType.Me)), // win
        (100, b => b.GetWinningTile(PlayerType.Opponent)), // block
        (10, GetBestOfTheRest),
        (1, GetRandom),
    ];

    public (int Rating, Tile BestTile) GetBestTile()
    {
        foreach (var (rating, rule) in _ratedRules)
        {
            var bestTile = rule(this);
            if (bestTile is not null)
            {
                return (rating, bestTile);
            }
        }

        return (0, null);
    }
}


public record Position(int Row, int Col)
{
    public void MakeMove(IGameIO gameIO)
    {
        gameIO.IssueInstruction($"{Row} {Col}");
    }

    public bool IsValid()
     => Row > -1 && Col > -1;

    public bool For(int rowMin, int rowMax, int colMin, int colMax)
    {
        if (Row < rowMin)
            return false;

        if (Row > rowMax)
            return false;

        if (Col < colMin)
            return false;

        if (Col > colMax)
            return false;

        return true;
    }

    public static Position ParseNextMove(IGameIO gameIO)
    {
        var inputs = gameIO.ReadInput().Split(' ');
        int row = int.Parse(inputs[0]);
        int col = int.Parse(inputs[1]);

        return new Position(row, col);
    }
};

public record Tile
{
    public Tile(Position position)
    {
        Position = position;
        Owner = PlayerType.None;
        Code = position.GetTileCode();
    }

    public TileCode Code { get; }

    public Position Position { get; }

    public void ChangeOwner(PlayerType owner)
    {
        if (Owner != PlayerType.None)
        {
            throw new Exception($"Tile at {Code} already has an owner of {Owner}, cannot change to {owner}.");
        }

        Owner = owner;
    }

    public PlayerType Owner { get; private set; }
};

public record WinCondition(TileCode[] Conditions)
{
    public int Free { get; private set; } = 3;

    public int Mine { get; private set; } = 0;

    public int Opponents { get; private set; } = 0;

    public bool IsStillWinnable()
    {
        if (Free == 0)
        {
            return false;
        }

        if (Mine > 0 && Opponents > 0)
        {
            return false;
        }

        return true;
    }

    public bool IsStillWinnableBy(PlayerType owner)
        => IsStillWinnable() && CheckWinState(has => has > 1, owner);

    public bool WinnableInOneMoveBy(PlayerType owner)
    {
        if (IsStillWinnable() == false)
        {
            return false;
        }

        return CheckWinState(has => has == 2, owner);
    }

    public bool WonBy(PlayerType owner)
    {
        if (Free > 0)
        {
            return false;
        }

        return CheckWinState(has => has == 3, owner);
    }

    private bool CheckWinState(Func<int, bool> requirment, PlayerType owner)
    {
        return owner switch
        {
            PlayerType.Me => requirment(Mine),
            PlayerType.Opponent => requirment(Opponents),
            _ => throw new Exception($"Owner of {owner} does not have a win condition."),
        };
    }

    public void ChangeOwner(TileCode tileCode, PlayerType owner)
    {
        if (Conditions.Contains(tileCode) == false)
        {
            return;
        }

        Free--;

        switch (owner)
        {
            case PlayerType.Me:
                Mine++;
                break;
            case PlayerType.Opponent:
                Opponents++;
                break;
            default:
                throw new Exception($"Owner of {owner} is not a win condition.");
        }
    }
}

public record Turn(int Number, Position MyMove, Position OpponentsLastMove, IEnumerable<Position> ValidMoves)
{ }

public static class Helper
{
    public static readonly (int[] Sets, char RowCode, char ColCode)[] TilePositionCodes =
    [
        ([0, 3, 6], 'T', 'L'),
        ([1, 4, 7], 'M', 'M'),
        ([2, 5, 8], 'B', 'R'),
    ];

    public static TileCode GetTileCode(this Position tilePosition)
    {
        if (tilePosition.Row == -1 || tilePosition.Col == -1)
        {
            return TileCode.None;
        }

        var tileCode = "";

        tileCode += TilePositionCodes.First(p => p.Sets.Contains(tilePosition.Row)).RowCode;
        tileCode += TilePositionCodes.First(p => p.Sets.Contains(tilePosition.Col)).ColCode;

        return Enum.Parse<TileCode>(tileCode);
    }
}

public enum PlayerType
{
    None,
    Me,
    Opponent,
}

public enum TileCode : int
{
    None = 0,
    TL = 1,
    TM = 2,
    TR = 3,
    ML = 4,
    MM = 5,
    MR = 6,
    BL = 7,
    BM = 8,
    BR = 9
}

public class GameConsoleIO : IGameIO
{
    private readonly LogType _enabledLogs;

    public GameConsoleIO(LogType enabledLogs)
    {
        _enabledLogs = enabledLogs;
    }

    public void IssueInstruction(string instruction)
        => Console.WriteLine(instruction);

    public void LogInfo(string message, LogType logType)
    {
        if ((_enabledLogs & logType) == 0)
        {
            return;
        }

        Console.Error.WriteLine(message);
    }

    public string ReadInput()
        => Console.ReadLine()!;
}

public interface IGameIO
{
    string ReadInput();

    void IssueInstruction(string instruction);

    void LogInfo(string message, LogType logType);
}

[Flags]
public enum LogType : int
{
    None = 0,
    Setup = 1,
    Board = 2,
    Turn = 4,
    ValidMoves = 8,
    Selection,
    Basic = Setup | Board | Turn | Selection,
    All = Setup | Board | Turn | ValidMoves | Selection,
}