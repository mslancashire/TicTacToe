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
        var game = Game.CreateGame(new GameConsoleIO(LogType.Basic));

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

    public Board MainBoard { get; private set; }

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

        MainBoard = new Board(Position.MainBoard, _gameIO);
        MainBoard.AllocateTiles(Boards);
    }

    public void ChangeTileOwner(Position position, PlayerType owner)
        => GetBoard(position).ChangeTileOwner(position, owner);

    public Board GetBoard(Position position)
        => Boards.First(b => b.Tiles.Any(t => t.Position == position));

    public Board GetBoard(TileCode tileCode)
        => Boards.First(b => b.Code == tileCode);

    public IEnumerable<Board> PlayableBoards()
        => Boards.Where(b => b.IsPlayable());

    public IEnumerable<BoardValue> Evaluate()
        => PlayableBoards().Select(b => b.Evaluate());

    public Position GetBestMove()
    {
        var currentTurn = CurrentTurn;

        if (currentTurn.Number == 1)
        {
            return new Position(0, 0); // TL
            //var rnd = new Random();
            //return currentTurn.ValidMoves.ElementAt(rnd.Next(currentTurn.ValidMoves.Count()));
        }

        var gameEvaluation = MainBoard.Evaluate();
        foreach (var boardState in gameEvaluation.Tiles)
        {
            _gameIO.LogInfo($"Board {boardState.TileCode} has a value to me of {boardState.MyValue} vs {boardState.OpponentValue}", LogType.Board);
        }

        var evaluatedBoards = Evaluate();
        foreach (var evaluatedBoard in evaluatedBoards)
        {
            _gameIO.LogInfo($"Board {evaluatedBoard.TileCode} has a value to me of {evaluatedBoard.MyValue} and to my opponent of {evaluatedBoard.OpponentValue}.", LogType.Board);
        }

        var moveEvaluator = new MoveEvaluator(evaluatedBoards);

        var opponentsSelectedBoard = GetBoard(currentTurn.OpponentsLastMove.GetTileCode());
        if (opponentsSelectedBoard.IsPlayable())
        {
            _gameIO.LogInfo($"Opponent selected a valid board of {opponentsSelectedBoard.Code}.", LogType.Selection);
        }
        else
        {
            _gameIO.LogInfo($"Opponent selected an invalid board of {opponentsSelectedBoard.Code}.", LogType.Selection);
        }

        var evaluatedMoves = opponentsSelectedBoard.IsPlayable() ?
            moveEvaluator.EvaluateFor(opponentsSelectedBoard) :
            moveEvaluator.EvaluateAll();

        foreach (var evaluatedMove in evaluatedMoves)
        {
            _gameIO.LogInfo($"Move made on board {evaluatedMove.MoveOn} for {evaluatedMove.MyMove.TileCode}, valued at {evaluatedMove.Delta} ({evaluatedMove.MyMove.MyValue} - {evaluatedMove.OpponentsMove.OpponentValue}).", LogType.Selection);
        }

        var selectedMove = evaluatedMoves
                .OrderByDescending(em => em.Delta)
                .FirstOrDefault();

        _gameIO.LogInfo($"Selected move on board {selectedMove.MoveOn} for {selectedMove.MyMove.TileCode}, valued at {selectedMove.Delta} ({selectedMove.MyMove.MyValue} - {selectedMove.OpponentsMove.OpponentValue})", LogType.Selection);

        if (selectedMove is null)
        {
            _gameIO.LogInfo($"No best tile found, use first valid.", LogType.Selection);
            return currentTurn.ValidMoves.First();
        }

        return selectedMove.MyMove.Tile.Position;
    }

    public void EndTurn()
    {
        var myMove = GetBestMove();
        _gameIO.MakeMove(myMove);
        AddMyMove(myMove);

        _gameIO.LogInfo($"My move => `{myMove.GetTileCode()}`, `{myMove}`", LogType.Selection);
        _gameIO.LogInfo($"End of turn {CurrentTurn.Number}.", LogType.Turn);
    }
}

public record Board : ITile
{
    private readonly IGameIO _gameIO;

    public Board(Position position, IGameIO gameIO)
    {
        _gameIO = gameIO;

        Code = position.GetTileCode();
        Position = position;
        WinConditions = [.. Settings.WinConditions.Select(w => w with { })];
    }

    public HashSet<WinCondition> WinConditions { get; private init; }

    public static Func<WinCondition, bool> PlayableCondition()
        => wc => wc.IsPlayable();

    public static Func<WinCondition, bool> WinnableCondition()
        => wc => wc.IsStillWinnable();

    public IEnumerable<WinCondition> PlayableWinConditions()
        => WinConditions.Where(PlayableCondition());

    public IEnumerable<WinCondition> WinnableConditions()
        => WinConditions.Where(WinnableCondition());

    public TileCode Code { get; }

    public Position Position { get; }

    public bool BeenWon()
        => WinConditions.Any(wc => wc.WonBy(PlayerType.Me) || wc.WonBy(PlayerType.Opponent));

    public bool IsFull()
        => Tiles.All(t => t.Owner != PlayerType.None);

    public bool IsPlayable()
        => IsFull() == false && BeenWon() == false;

    public bool IsStillWinnable()
        => IsFull() == false && BeenWon() == false && RemainingWinConditions() > 0;

    public bool IsStillWinnableBy(PlayerType player)
        => IsFull() == false && BeenWon() == false && RemainingWinConditionsFor(player) > 0;

    public int RemainingWinConditions()
        => WinConditions.Count(WinnableCondition());

    public int RemainingWinConditionsFor(PlayerType owner)
        => WinConditions.Count(wc => wc.IsStillWinnableBy(owner));

    public HashSet<ITile> Tiles { get; private set; }

    public void AllocateTiles(IEnumerable<ITile> tiles)
        => Tiles = [.. tiles];

    public void ChangeTileOwner(Position position, PlayerType owner)
        => ChangeTileOwner(position.GetTileCode(), owner);

    public void ChangeTileOwner(TileCode tileCode, PlayerType owner)
    {
        Tiles
            .Single(t => t.Code == tileCode)
            .ChangeOwner(owner);

        foreach (var winCondition in WinConditions)
        {
            winCondition.ChangeOwner(tileCode, owner);
        }
    }

    public PlayerType Owner
    {
        get
        {
            if (WinConditions.Any(wc => wc.WonBy(PlayerType.Me)))
            {
                return PlayerType.Me;
            }

            if (WinConditions.Any(wc => wc.WonBy(PlayerType.Opponent)))
            {
                return PlayerType.Opponent;
            }

            return PlayerType.None;
        }
    }

    public void ChangeOwner(PlayerType playerType)
        => throw new NotSupportedException("A boards ownership is controlled by its internal tiles.");

    public BoardValue Evaluate()
    {
        var evaluatedBoard = BoardValue.Empty(this);

        foreach (var tile in Tiles.Where(t => t.Owner == PlayerType.None))
        {
            var currentRatings = PlayableWinConditions()
                .Where(wc => wc.Conditions.Contains(tile.Code))
                .Select(wc => wc.Rating());

            var tileValue = TileValue.Empty(tile);
            tileValue.Append(currentRatings);

            evaluatedBoard.Append(tileValue);
        }

        return evaluatedBoard;
    }
}

public record Position(int Row, int Col)
{
    public static Position Invalid
        => new(-1, -1);

    public static Position MainBoard
        => new(50, 50);

    public bool IsValid()
        => this != Invalid;

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

public interface ITile
{
    TileCode Code { get; }

    PlayerType Owner { get; }

    Position Position { get; }

    void ChangeOwner(PlayerType owner);
}

public record Tile : ITile
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

    public PlayerValue Rating()
    {
        if (IsPlayable() == false)
        {
            return PlayerValue.NotPlayable();
        }

        if (IsStillWinnable() == false)
        {
            return PlayerValue.NotWinnableByEitherPlayer();
        }

        if (Free == 3)
        {
            return PlayerValue.NothingPlayed();
        }

        return PlayerValue.AssessValue(this);
    }

    public bool IsPlayable()
        => Free > 0;

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

    public bool IsStillWinnableBy(PlayerType owner) =>
        IsStillWinnable() &&
        CheckWinState(has => has >= 0, owner) &&
        CheckWinState(has => has == 0, owner.GetOtherPlayer());

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

    private bool CheckWinState(Func<int, bool> requirement, PlayerType owner)
    {
        return owner switch
        {
            PlayerType.Me => requirement(Mine),
            PlayerType.Opponent => requirement(Opponents),
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

public record PlayerValue(int MyValue, int OpponentsValue)
{
    public static PlayerValue Empty()
        => new(0, 0);

    public static PlayerValue NotPlayable()
        => new(0, 0);

    public static PlayerValue NotWinnableByEitherPlayer()
        => new(0, 0);

    public static PlayerValue NothingPlayed()
        => new(1, 1);

    public static PlayerValue AssessValue(WinCondition winCondition)
    {
        if (winCondition.IsStillWinnableBy(PlayerType.Me))
        {
            return new PlayerValue(100 * winCondition.Mine, 10 * winCondition.Mine);
        }

        if (winCondition.IsStillWinnableBy(PlayerType.Opponent))
        {
            return new PlayerValue(10 * winCondition.Opponents, 100 * winCondition.Opponents);
        }

        return NothingPlayed();
    }
};

public record TileValue(TileCode TileCode, ITile Tile)
{
    public static TileValue Empty(ITile tile)
        => new(tile.Code, tile);

    public int MyValue { get; private set; } = 0;

    public int OpponentValue { get; private set; } = 0;

    public void Append(IEnumerable<PlayerValue> winConditionValues)
    {
        MyValue += winConditionValues.Sum(wc => wc.MyValue);
        OpponentValue += winConditionValues.Sum(wc => wc.OpponentsValue);
    }
}

public record BoardValue(TileCode TileCode, Board Board)
{
    public static BoardValue Empty(Board board)
        => new(board.Code, board);

    private Dictionary<TileCode, TileValue> _tileValues = [];

    public int MyValue => _tileValues.Sum(kv => kv.Value.MyValue);

    public int OpponentValue => _tileValues.Sum(kv => kv.Value.OpponentValue);

    public IEnumerable<TileValue> Tiles
        => _tileValues.Values;

    public TileValue BestFor(PlayerType player)
    {
        return _tileValues
            .OrderByDescending(tv => player == PlayerType.Me ? tv.Value.MyValue : tv.Value.OpponentValue)
            .First().Value;
    }

    public void Append(TileValue tileValue)
    {
        if (_tileValues.ContainsKey(tileValue.TileCode) == false)
        {
            _tileValues.Add(tileValue.TileCode, tileValue);
        }
    }
}

public record MoveEvaluator
{
    private Dictionary<TileCode, BoardValue> _evaluatedBoards;

    public MoveEvaluator(IEnumerable<BoardValue> evaluatedBoards)
    {
        _evaluatedBoards = evaluatedBoards.ToDictionary(k => k.TileCode, v => v);
    }

    public IEnumerable<MoveValue> EvaluateAll()
    {
        var evaluatedMoves = new List<MoveValue>();

        foreach (var board in _evaluatedBoards.Values)
        {
            var myBestMove = board.BestFor(PlayerType.Me);
            var oppenentsSelectedBoard = SelectBoardFor(myBestMove.TileCode, PlayerType.Opponent);
            var oppenentsBestNextMove = oppenentsSelectedBoard.BestFor(PlayerType.Opponent);

            evaluatedMoves.Add(new MoveValue(board.TileCode, myBestMove, oppenentsBestNextMove));
        }

        return evaluatedMoves;
    }

    public IEnumerable<MoveValue> EvaluateFor(Board board)
    {
        var evaluatedMoves = new List<MoveValue>();
        var evaludatedBoard = SelectBoardFor(board.Code, PlayerType.Me);

        foreach (var tile in evaludatedBoard.Tiles)
        {
            var oppenentsSelectedBoard = SelectBoardFor(tile.TileCode, PlayerType.Opponent);
            var oppenentsBestNextMove = oppenentsSelectedBoard.BestFor(PlayerType.Opponent);

            evaluatedMoves.Add(new MoveValue(board.Code, tile, oppenentsBestNextMove));
        }

        return evaluatedMoves;
    }

    private BoardValue SelectBoardFor(TileCode tileCode, PlayerType player)
    {
        if (_evaluatedBoards.TryGetValue(tileCode, out var selectedBoard))
        {
            return selectedBoard;
        }

        return _evaluatedBoards
            .OrderByDescending(eb => eb.Value.OpponentValue)
            .First()
            .Value;
    }
}

public record MoveValue(TileCode MoveOn, TileValue MyMove, TileValue OpponentsMove)
{
    public int Delta => MyMove.MyValue - OpponentsMove.OpponentValue;
}

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
        if (tilePosition == Position.Invalid)
        {
            return TileCode.None;
        }

        if (tilePosition == Position.MainBoard)
        {
            return TileCode.None;
        }

        var tileCode = "";

        tileCode += TilePositionCodes.First(p => p.Sets.Contains(tilePosition.Row)).RowCode;
        tileCode += TilePositionCodes.First(p => p.Sets.Contains(tilePosition.Col)).ColCode;

        return Enum.Parse<TileCode>(tileCode);
    }

    public static PlayerType GetOtherPlayer(this PlayerType playerType) => playerType switch
    {
        PlayerType.Me => PlayerType.Opponent,
        PlayerType.Opponent => PlayerType.Me,
        _ => PlayerType.None,
    };
}

public class Settings
{
    public static readonly List<WinCondition> WinConditions =
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

    public void MakeMove(Position move)
    {
        IssueInstruction($"{move.Row} {move.Col}");
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
    void MakeMove(Position move);

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
    Selection = 16,
    RuleEvaluation = 32,
    Basic = Setup | Board | Turn | Selection,
    All = Setup | Board | Turn | ValidMoves | Selection | RuleEvaluation,
}