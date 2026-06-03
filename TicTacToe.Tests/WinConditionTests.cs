namespace TicTacToe.Tests;

public class WinConditionTests
{
    private readonly ITile _tileML = Tile.CreateFrom(TileCode.ML);
    private readonly ITile _tileMM = Tile.CreateFrom(TileCode.MM);
    private readonly ITile _tileMR = Tile.CreateFrom(TileCode.MR);

    private WinCondition CreateSUT()
        => new([_tileML, _tileMM, _tileMR]);

    [Fact]
    public void WinCondition_should_have_correct_state_given_basic_setup()
    {
        // arrange

        // act
        var sut = CreateSUT();

        // assert
        sut.Free.Should().Be(3);
        sut.Mine.Should().Be(0);
        sut.Opponents.Should().Be(0);

        sut.IsStillWinnable().Should().BeTrue();

        sut.IsStillWinnableBy(PlayerType.Me).Should().BeTrue();
        sut.IsStillWinnableBy(PlayerType.Opponent).Should().BeTrue();

        sut.WonBy(PlayerType.Me).Should().BeFalse();
        sut.WonBy(PlayerType.Opponent).Should().BeFalse();
        
        var rating = sut.Rating();
        rating.MyValue.Should().Be(1);
        rating.OpponentsValue.Should().Be(1);
    }

    [Fact]
    public void WinCondition_should_have_correct_state_given_i_own_all_tiles()
    {
        // arrange
        var sut = CreateSUT();

        // act
        _tileML.ChangeOwner(PlayerType.Me);
        _tileMM.ChangeOwner(PlayerType.Me);
        _tileMR.ChangeOwner(PlayerType.Me);

        // assert
        sut.Free.Should().Be(0);
        sut.Mine.Should().Be(3);
        sut.Opponents.Should().Be(0);

        sut.IsStillWinnable().Should().BeFalse();

        sut.IsStillWinnableBy(PlayerType.Me).Should().BeFalse();
        sut.IsStillWinnableBy(PlayerType.Opponent).Should().BeFalse();

        sut.WonBy(PlayerType.Me).Should().BeTrue();
        sut.WonBy(PlayerType.Opponent).Should().BeFalse();
        
        var rating = sut.Rating();
        rating.MyValue.Should().Be(0);
        rating.OpponentsValue.Should().Be(0);
    }

    [Fact]
    public void WinCondition_should_have_correct_state_given_opponent_owns_all_tiles()
    {
        // arrange
        var sut = CreateSUT();

        // act
        _tileML.ChangeOwner(PlayerType.Opponent);
        _tileMM.ChangeOwner(PlayerType.Opponent);
        _tileMR.ChangeOwner(PlayerType.Opponent);

        // assert
        sut.Free.Should().Be(0);
        sut.Mine.Should().Be(0);
        sut.Opponents.Should().Be(3);

        sut.IsStillWinnable().Should().BeFalse();

        sut.IsStillWinnableBy(PlayerType.Me).Should().BeFalse();
        sut.IsStillWinnableBy(PlayerType.Opponent).Should().BeFalse();

        sut.WonBy(PlayerType.Me).Should().BeFalse();
        sut.WonBy(PlayerType.Opponent).Should().BeTrue();
        var rating = sut.Rating();
        rating.MyValue.Should().Be(0);
        rating.OpponentsValue.Should().Be(0);
    }

    [Fact]
    public void WinCondition_should_have_correct_state_given_mixed_ownership()
    {
        // arrange
        var sut = CreateSUT();

        // act
        _tileML.ChangeOwner(PlayerType.Me);
        _tileMM.ChangeOwner(PlayerType.Opponent);

        // assert
        sut.Free.Should().Be(1);
        sut.Mine.Should().Be(1);
        sut.Opponents.Should().Be(1);

        sut.IsStillWinnable().Should().BeFalse();

        sut.IsStillWinnableBy(PlayerType.Me).Should().BeFalse();
        sut.IsStillWinnableBy(PlayerType.Opponent).Should().BeFalse();

        sut.WonBy(PlayerType.Me).Should().BeFalse();
        sut.WonBy(PlayerType.Opponent).Should().BeFalse();
        
        var rating = sut.Rating();
        rating.MyValue.Should().Be(0);
        rating.OpponentsValue.Should().Be(0);
    }

    [Fact]
    public void WinCondition_should_have_correct_state_given_i_own_2_tiles()
    {
        // arrange
        var sut = CreateSUT();

        // act
        _tileML.ChangeOwner(PlayerType.Me);
        _tileMM.ChangeOwner(PlayerType.Me);

        // assert
        sut.Free.Should().Be(1);
        sut.Mine.Should().Be(2);
        sut.Opponents.Should().Be(0);
        sut.IsStillWinnable().Should().BeTrue();
        sut.IsStillWinnableBy(PlayerType.Me).Should().BeTrue();
        sut.IsStillWinnableBy(PlayerType.Opponent).Should().BeFalse();

        sut.WonBy(PlayerType.Me).Should().BeFalse();
        sut.WonBy(PlayerType.Opponent).Should().BeFalse();
        
        var rating = sut.Rating();
        rating.MyValue.Should().Be(200);
        rating.OpponentsValue.Should().Be(20);
    }

    [Fact]
    public void WinCondition_should_have_correct_state_given_opponent_owns_2_tiles()
    {
        // arrange
        var sut = CreateSUT();

        // act
        _tileML.ChangeOwner(PlayerType.Opponent);
        _tileMM.ChangeOwner(PlayerType.Opponent);

        // assert
        sut.Free.Should().Be(1);
        sut.Mine.Should().Be(0);
        sut.Opponents.Should().Be(2);
        sut.IsStillWinnable().Should().BeTrue();
        sut.IsStillWinnableBy(PlayerType.Me).Should().BeFalse();
        sut.IsStillWinnableBy(PlayerType.Opponent).Should().BeTrue();

        sut.WonBy(PlayerType.Me).Should().BeFalse();
        sut.WonBy(PlayerType.Opponent).Should().BeFalse();
        
        var rating = sut.Rating();
        rating.MyValue.Should().Be(20);
        rating.OpponentsValue.Should().Be(200);
    }
}
