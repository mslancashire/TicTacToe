using TicTacToe.Tests.Helpers;

namespace TicTacToe.Tests;

public class GameIntegrationTests : BaseTestForGame
{
    public GameIntegrationTests(ITestOutputHelper logger) : base(logger)
    {
    }

    [Fact]
    public void GameSetup_should_be_correct_based_on_1_turn_of_input()
    {
        // arrange
        SetInputFromFile("Board_1_Turn.txt");

        // assert - check initial board setup
        Game.Boards.Should().HaveCount(9);
        Game.Boards.Should().AllSatisfy(b => b.Tiles.Should().HaveCount(9));
        Game.Boards.Should().AllSatisfy(b => b.Code.Should().NotBe(TileCode.None));

        Game.Tiles.Should().HaveCount(81);
        Game.Tiles.Should().AllSatisfy(t => t.Code.Should().NotBe(TileCode.None));
        Game.Tiles.Should().AllSatisfy(t => t.Owner.Should().Be(PlayerType.None));

        Game.Boards
            .FirstIs(b => b.Code.Should().Be(TileCode.TL))
            .NextIs(b => b.Code.Should().Be(TileCode.TM))
            .NextIs(b => b.Code.Should().Be(TileCode.TR))
            .NextIs(b => b.Code.Should().Be(TileCode.ML))
            .NextIs(b => b.Code.Should().Be(TileCode.MM))
            .NextIs(b => b.Code.Should().Be(TileCode.MR))
            .NextIs(b => b.Code.Should().Be(TileCode.BL))
            .NextIs(b => b.Code.Should().Be(TileCode.BM))
            .NextIs(b => b.Code.Should().Be(TileCode.BR))
            .NoMore();

        Game.Boards.Should().AllSatisfy(b => b.Tiles
            .FirstIs(t => t.Code.Should().Be(TileCode.TL))
            .NextIs(t => t.Code.Should().Be(TileCode.TM))
            .NextIs(t => t.Code.Should().Be(TileCode.TR))
            .NextIs(t => t.Code.Should().Be(TileCode.ML))
            .NextIs(t => t.Code.Should().Be(TileCode.MM))
            .NextIs(t => t.Code.Should().Be(TileCode.MR))
            .NextIs(t => t.Code.Should().Be(TileCode.BL))
            .NextIs(t => t.Code.Should().Be(TileCode.BM))
            .NextIs(t => t.Code.Should().Be(TileCode.BR))
            .NoMore());

        Game.Boards[0].Tiles.ElementAt(0).Should().BeSameAs(Game.Tiles.ElementAt(0));

        // act - begin turn
        Game.BeginTurn();

        // assert - check being turn state
        Game.Turns.Should().HaveCount(1);
        Game.CurrentTurn.Should().NotBeNull();
        Game.Turns[0].Should().Be(Game.CurrentTurn);
        Game.CurrentTurn.Number.Should().Be(1);
        Game.CurrentTurn.OpponentsLastMove.Should().BeNull();
        Game.CurrentTurn.MyMove.Should().BeNull();
        Game.CurrentTurn.ValidMoves.Should().BeNull();

        // act - opponents move
        Game.AddOpponentsMove();

        // assert - check opponents move
        Game.Turns.Should().HaveCount(1);
        Game.CurrentTurn.OpponentsLastMove.Should().NotBeNull();
        Game.CurrentTurn.OpponentsLastMove.ShouldBeInvalid();

        // act - valid moves
        Game.ReadValidMoves();

        // assert - check valid moves
        Game.Turns.Should().HaveCount(1);
        Game.CurrentTurn.ValidMoves.Should().NotBeNull();
        Game.CurrentTurn.ValidMoves.Should().HaveCount(11);
        var validMoves = Game.CurrentTurn.ValidMoves.ToList();
        validMoves
            .FirstIs(5, 6)
            .NextIs(0, 5)
            .NextIs(4, 1)
            .NextIs(5, 3)
            .NextIs(4, 4)
            .NextIs(0, 0)
            .NextIs(2, 0)
            .NextIs(2, 3)
            .NextIs(8, 1)
            .NextIs(5, 2)
            .NextIs(2, 5)
            .NoMore();

        // act - make move
        Game.EndTurn();

        // assert - check end turn state
        Game.Turns.Should().HaveCount(1);
        Game.CurrentTurn.MyMove.Should().NotBeNull();
        Game.CurrentTurn.MyMove.ShouldBe(0, 0);
        GetIssuedInstructions()
            .FirstIs("0 0")
            .NoMore();
    }

    [Fact]
    public void Changing_the_owner_of_a_tile_should_update_the_tile_and_win_conditions()    
    {
        // arrange
        SetInputFromFile("Board_1_Turn.txt");

        var move = new Position(4, 4);

        // act
        Game.ChangeTileOwner(move, PlayerType.Me);

        // assert
        var tile = Game.Tiles.First(t => t.Position == move);
        tile.Owner.Should().Be(PlayerType.Me);

        var board = Game.GetBoard(move);
        board.Tiles.Any(t => t.Code == tile.Code).Should().BeTrue();
        board.BeenWon().Should().BeFalse();
        board.IsPlayable().Should().BeTrue();
        board.IsFull().Should().BeFalse();

        var relevantWCs = board.WinConditions.Where(wc => wc.Conditions.Contains(move.GetTileCode()));
        relevantWCs.Should().NotBeEmpty();
        relevantWCs.Should().HaveCount(4);
        relevantWCs.Should().AllSatisfy(wc => wc.Conditions.Should().HaveCount(3));
        relevantWCs.Should().AllSatisfy(wc => wc.Free.Should().Be(2));
        relevantWCs.Should().AllSatisfy(wc => wc.Mine.Should().Be(1));
        relevantWCs.Should().AllSatisfy(wc => wc.Opponents.Should().Be(0));
        relevantWCs.Should().AllSatisfy(wc => wc.IsStillWinnable().Should().BeTrue());
        relevantWCs.Should().AllSatisfy(wc => wc.IsStillWinnableBy(PlayerType.Me).Should().BeTrue());
        relevantWCs.Should().AllSatisfy(wc => wc.IsStillWinnableBy(PlayerType.Opponent).Should().BeFalse());

        board.RemainingWinConditions().Should().Be(8);
        board.RemainingWinConditionsFor(PlayerType.Me).Should().Be(8);
        board.RemainingWinConditionsFor(PlayerType.Opponent).Should().Be(4);

        var evaluatedBoard = board.Evaluate();
        evaluatedBoard.MyValue.Should().Be(812);
        var bestTile = evaluatedBoard.BestFor(PlayerType.Me);

        var validCodes = new[] { TileCode.TL, TileCode.TR, TileCode.BL, TileCode.BR };
        validCodes.Should().Contain(bestTile.TileCode);
    }
}
