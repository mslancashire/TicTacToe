namespace TicTacToe.Tests;

public class BoardTests
{
    private readonly Mock<IGameIO> _gameIOMock;
    private readonly Game _game;

    public BoardTests()
    {
        _gameIOMock = new Mock<IGameIO>();
        _game = Game.CreateGame(_gameIOMock.Object);
    }

    private Board CreateSUT()
    {
        return _game.Boards.First();
    }

    [Fact]
    public void Board_win_conditions_should_be_correct_on_setup()
    {
        // arrange
        var sut = CreateSUT();

        // act

        // assert
        sut.BeenWon().Should().BeFalse();
        sut.IsFull().Should().BeFalse();
        sut.IsPlayable().Should().BeTrue();

        sut.IsStillWinnable().Should().BeTrue();
        sut.IsStillWinnableBy(PlayerType.Me).Should().BeTrue();
        sut.IsStillWinnableBy(PlayerType.Opponent).Should().BeTrue();

        sut.WinConditions.Should().HaveCount(8);
        sut.RemainingWinConditions().Should().Be(8);
        sut.RemainingWinConditionsFor(PlayerType.Me).Should().Be(8);
        sut.RemainingWinConditionsFor(PlayerType.Opponent).Should().Be(8);

        var evaluatedBoard = sut.Evaluate();
        evaluatedBoard.Should().NotBeNull();
        evaluatedBoard.Tiles.Should().HaveCount(9);
    }

    [Fact]
    public void Board_should_be_full_when_all_tiles_have_been_played_in_stale_mate()
    {
        // arrange
        var sut = CreateSUT();

        // act
        sut.ChangeTileOwner(TileCode.TL, PlayerType.Me);
        sut.ChangeTileOwner(TileCode.TM, PlayerType.Opponent);
        sut.ChangeTileOwner(TileCode.TR, PlayerType.Me);

        sut.ChangeTileOwner(TileCode.ML, PlayerType.Me);
        sut.ChangeTileOwner(TileCode.MM, PlayerType.Opponent);
        sut.ChangeTileOwner(TileCode.MR, PlayerType.Opponent);

        sut.ChangeTileOwner(TileCode.BL, PlayerType.Opponent);
        sut.ChangeTileOwner(TileCode.BM, PlayerType.Me);
        sut.ChangeTileOwner(TileCode.BR, PlayerType.Opponent);

        // assert
        sut.BeenWon().Should().BeFalse();
        sut.IsFull().Should().BeTrue();
        sut.IsPlayable().Should().BeFalse();

        sut.IsStillWinnable().Should().BeFalse();
        sut.IsStillWinnableBy(PlayerType.Me).Should().BeFalse();
        sut.IsStillWinnableBy(PlayerType.Opponent).Should().BeFalse();

        sut.RemainingWinConditions().Should().Be(0);
        sut.RemainingWinConditionsFor(PlayerType.Me).Should().Be(0);
        sut.RemainingWinConditionsFor(PlayerType.Opponent).Should().Be(0);

        sut.WinConditions.Should().HaveCount(8);
        sut.WinConditions.Should().AllSatisfy(wc => wc.Free.Should().Be(0));
        sut.WinConditions.Should().AllSatisfy(wc => wc.IsStillWinnable().Should().BeFalse());

        var evaluatedBoard = sut.Evaluate();
        evaluatedBoard.Should().NotBeNull();
        evaluatedBoard.Tiles.Should().BeEmpty();
    }

    [Fact]
    public void Board_should_be_still_be_playable_when_all_but_one_tile_has_been_played_but_no_longer_winnable()
    {
        // arrange
        var sut = CreateSUT();

        // act
        sut.ChangeTileOwner(TileCode.TL, PlayerType.Me);
        sut.ChangeTileOwner(TileCode.TM, PlayerType.Opponent);
        sut.ChangeTileOwner(TileCode.TR, PlayerType.Me);

        sut.ChangeTileOwner(TileCode.ML, PlayerType.Me);
        sut.ChangeTileOwner(TileCode.MM, PlayerType.Opponent);
        sut.ChangeTileOwner(TileCode.MR, PlayerType.Opponent);

        sut.ChangeTileOwner(TileCode.BL, PlayerType.Opponent);
        sut.ChangeTileOwner(TileCode.BM, PlayerType.Me);

        // assert
        sut.BeenWon().Should().BeFalse();
        sut.IsFull().Should().BeFalse();
        sut.IsPlayable().Should().BeTrue();

        sut.IsStillWinnable().Should().BeFalse();
        sut.IsStillWinnableBy(PlayerType.Me).Should().BeFalse();
        sut.IsStillWinnableBy(PlayerType.Opponent).Should().BeFalse();

        sut.RemainingWinConditions().Should().Be(0);
        sut.RemainingWinConditionsFor(PlayerType.Me).Should().Be(0);
        sut.RemainingWinConditionsFor(PlayerType.Opponent).Should().Be(0);

        sut.WinConditions.Should().HaveCount(8);
        sut.PlayableWinConditions().Should().HaveCount(3);
        sut.PlayableWinConditions().Should().AllSatisfy(wc => wc.IsStillWinnable().Should().BeFalse());

        var evaluatedBoard = sut.Evaluate();
        evaluatedBoard.Should().NotBeNull();
        evaluatedBoard.Tiles.Should().HaveCount(1);
    }

    [Fact]
    public void Board_should_be_still_be_playable_when_all_but_two_tiles_have_been_played_and_is_still_winnable()
    {
        // arrange
        var sut = CreateSUT();

        // act
        sut.ChangeTileOwner(TileCode.TL, PlayerType.Me);
        sut.ChangeTileOwner(TileCode.TM, PlayerType.Opponent);
        sut.ChangeTileOwner(TileCode.TR, PlayerType.Me);

        sut.ChangeTileOwner(TileCode.ML, PlayerType.Me);
        sut.ChangeTileOwner(TileCode.MM, PlayerType.Opponent);
        sut.ChangeTileOwner(TileCode.MR, PlayerType.Opponent);

        sut.ChangeTileOwner(TileCode.BL, PlayerType.Opponent);

        // assert
        sut.BeenWon().Should().BeFalse();
        sut.IsFull().Should().BeFalse();
        sut.IsPlayable().Should().BeTrue();

        sut.IsStillWinnable().Should().BeTrue();
        sut.IsStillWinnableBy(PlayerType.Me).Should().BeFalse();
        sut.IsStillWinnableBy(PlayerType.Opponent).Should().BeTrue();

        sut.RemainingWinConditions().Should().Be(2);
        sut.RemainingWinConditionsFor(PlayerType.Me).Should().Be(0);
        sut.RemainingWinConditionsFor(PlayerType.Opponent).Should().Be(2);

        sut.WinConditions.Should().HaveCount(8);
        sut.PlayableWinConditions().Should().HaveCount(4);

        var evaluatedBoard = sut.Evaluate();
        evaluatedBoard.Should().NotBeNull();
        evaluatedBoard.Tiles.Should().HaveCount(2);
    }

    [Fact]
    public void Board_should_be_still_be_playable_and_winnable_by_both_players()
    {
        // arrange
        var sut = CreateSUT();

        // act
        sut.ChangeTileOwner(TileCode.TL, PlayerType.Me);
        sut.ChangeTileOwner(TileCode.TM, PlayerType.Opponent);
        sut.ChangeTileOwner(TileCode.TR, PlayerType.Me);

        sut.ChangeTileOwner(TileCode.ML, PlayerType.Me);
        sut.ChangeTileOwner(TileCode.MM, PlayerType.Opponent);
        sut.ChangeTileOwner(TileCode.MR, PlayerType.Me);

        sut.ChangeTileOwner(TileCode.BL, PlayerType.Opponent);

        // assert
        sut.BeenWon().Should().BeFalse();
        sut.IsFull().Should().BeFalse();
        sut.IsPlayable().Should().BeTrue();

        sut.IsStillWinnable().Should().BeTrue();
        sut.IsStillWinnableBy(PlayerType.Me).Should().BeTrue();
        sut.IsStillWinnableBy(PlayerType.Opponent).Should().BeTrue();

        sut.RemainingWinConditions().Should().Be(3);
        sut.RemainingWinConditionsFor(PlayerType.Me).Should().Be(1);
        sut.RemainingWinConditionsFor(PlayerType.Opponent).Should().Be(2);

        sut.WinConditions.Should().HaveCount(8);
        sut.PlayableWinConditions().Should().HaveCount(4);

        var evaluatedBoard = sut.Evaluate();
        evaluatedBoard.Should().NotBeNull();
        evaluatedBoard.Tiles.Should().HaveCount(2);
    }
}
