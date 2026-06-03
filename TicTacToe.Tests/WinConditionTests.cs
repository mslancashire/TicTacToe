namespace TicTacToe.Tests;

public class WinConditionTests
{
    [Fact]
    public void WinCondition_should_have_correct_state_given_basic_setup()
    {
        // arrange

        // act
        var sut = new WinCondition([TileCode.ML, TileCode.MM, TileCode.MR]);

        // assert
        sut.Free.Should().Be(3);
        sut.Mine.Should().Be(0);
        sut.Opponents.Should().Be(0);

        sut.IsStillWinnable().Should().BeTrue();

        sut.IsStillWinnableBy(PlayerType.Me).Should().BeTrue();
        sut.IsStillWinnableBy(PlayerType.Opponent).Should().BeTrue();

        sut.WinnableInOneMoveBy(PlayerType.Me).Should().BeFalse();
        sut.WinnableInOneMoveBy(PlayerType.Opponent).Should().BeFalse();

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
        var sut = new WinCondition([TileCode.ML, TileCode.MM, TileCode.MR]);

        // act
        sut.ChangeOwner(TileCode.ML, PlayerType.Me);
        sut.ChangeOwner(TileCode.MM, PlayerType.Me);
        sut.ChangeOwner(TileCode.MR, PlayerType.Me);

        // assert
        sut.Free.Should().Be(0);
        sut.Mine.Should().Be(3);
        sut.Opponents.Should().Be(0);

        sut.IsStillWinnable().Should().BeFalse();

        sut.IsStillWinnableBy(PlayerType.Me).Should().BeFalse();
        sut.IsStillWinnableBy(PlayerType.Opponent).Should().BeFalse();

        sut.WinnableInOneMoveBy(PlayerType.Me).Should().BeFalse();
        sut.WinnableInOneMoveBy(PlayerType.Opponent).Should().BeFalse();

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
        var sut = new WinCondition([TileCode.ML, TileCode.MM, TileCode.MR]);

        // act
        sut.ChangeOwner(TileCode.ML, PlayerType.Opponent);
        sut.ChangeOwner(TileCode.MM, PlayerType.Opponent);
        sut.ChangeOwner(TileCode.MR, PlayerType.Opponent);

        // assert
        sut.Free.Should().Be(0);
        sut.Mine.Should().Be(0);
        sut.Opponents.Should().Be(3);

        sut.IsStillWinnable().Should().BeFalse();

        sut.IsStillWinnableBy(PlayerType.Me).Should().BeFalse();
        sut.IsStillWinnableBy(PlayerType.Opponent).Should().BeFalse();

        sut.WinnableInOneMoveBy(PlayerType.Me).Should().BeFalse();
        sut.WinnableInOneMoveBy(PlayerType.Opponent).Should().BeFalse();

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
        var sut = new WinCondition([TileCode.ML, TileCode.MM, TileCode.MR]);

        // act
        sut.ChangeOwner(TileCode.ML, PlayerType.Me);
        sut.ChangeOwner(TileCode.MM, PlayerType.Opponent);

        // assert
        sut.Free.Should().Be(1);
        sut.Mine.Should().Be(1);
        sut.Opponents.Should().Be(1);

        sut.IsStillWinnable().Should().BeFalse();

        sut.IsStillWinnableBy(PlayerType.Me).Should().BeFalse();
        sut.IsStillWinnableBy(PlayerType.Opponent).Should().BeFalse();

        sut.WinnableInOneMoveBy(PlayerType.Me).Should().BeFalse();
        sut.WinnableInOneMoveBy(PlayerType.Opponent).Should().BeFalse();

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
        var sut = new WinCondition([TileCode.ML, TileCode.MM, TileCode.MR]);

        // act
        sut.ChangeOwner(TileCode.ML, PlayerType.Me);
        sut.ChangeOwner(TileCode.MM, PlayerType.Me);

        // assert
        sut.Free.Should().Be(1);
        sut.Mine.Should().Be(2);
        sut.Opponents.Should().Be(0);
        sut.IsStillWinnable().Should().BeTrue();
        sut.IsStillWinnableBy(PlayerType.Me).Should().BeTrue();
        sut.IsStillWinnableBy(PlayerType.Opponent).Should().BeFalse();

        sut.WinnableInOneMoveBy(PlayerType.Me).Should().BeTrue();
        sut.WinnableInOneMoveBy(PlayerType.Opponent).Should().BeFalse();

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
        var sut = new WinCondition([TileCode.ML, TileCode.MM, TileCode.MR]);

        // act
        sut.ChangeOwner(TileCode.ML, PlayerType.Opponent);
        sut.ChangeOwner(TileCode.MM, PlayerType.Opponent);

        // assert
        sut.Free.Should().Be(1);
        sut.Mine.Should().Be(0);
        sut.Opponents.Should().Be(2);
        sut.IsStillWinnable().Should().BeTrue();
        sut.IsStillWinnableBy(PlayerType.Me).Should().BeFalse();
        sut.IsStillWinnableBy(PlayerType.Opponent).Should().BeTrue();

        sut.WinnableInOneMoveBy(PlayerType.Me).Should().BeFalse();
        sut.WinnableInOneMoveBy(PlayerType.Opponent).Should().BeTrue();

        sut.WonBy(PlayerType.Me).Should().BeFalse();
        sut.WonBy(PlayerType.Opponent).Should().BeFalse();
        var rating = sut.Rating();
        rating.MyValue.Should().Be(20);
        rating.OpponentsValue.Should().Be(200);
    }
}
