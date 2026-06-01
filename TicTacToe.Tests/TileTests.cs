namespace TicTacToe.Tests;

public class TileTests
{
    [Fact]
    public void Tile_should_have_correct_state_given_setup_from_a_position()
    {
        // arrange
        var position = new Position(4, 4);
        
        // act
        var sut = new Tile(position);

        // assert
        sut.Code.Should().Be(TileCode.MM);
        sut.Owner.Should().Be(PlayerType.None);
        sut.Position.Should().Be(position);
    }

    [Fact]
    public void Tile_ownership_should_change_correctly()
    {
        // arrange
        var sut = new Tile(new Position(4, 4));
        
        // act
        sut.ChangeOwner(PlayerType.Me);
        
        // assert
        sut.Owner.Should().Be(PlayerType.Me);
        
        // act
        var exectpion = Assert.Throws<Exception>(() => sut.ChangeOwner(PlayerType.Opponent));
        
        // assert
        exectpion.Message.Should().Be("Tile at MM already has an owner of Me, cannot change to Opponent.");
    }
}
