namespace TicTacToe.Tests;

public class PositionTests
{
    [Fact]
    public void Position_should_have_correct_state_given_row_and_col()
    {
        // arrange
        var row = 1;
        var col = 1;

        // act
        var sut = new Position(row, col);

        // assert
        sut.Row.Should().Be(row);
        sut.Col.Should().Be(col);
        sut.IsValid().Should().BeTrue();
    }

    [Theory]
    [InlineData("1 1", 1, 1, true)]
    [InlineData("0 0", 0, 0, true)]
    [InlineData("-1 -1", -1, -1, false)]
    [InlineData("5 1", 5, 1, true)]
    public void Position_should_have_correct_state_from_an_game_io_input(string input, int expectedRow, int expectedCol, bool isValid)
    {
        // arrange
        var mockIO = new Mock<IGameIO>();
        mockIO.Setup(io => io.ReadInput()).Returns(input);

        // act
        var sut = Position.ParseNextMove(mockIO.Object);

        // assert
        sut.Row.Should().Be(expectedRow);
        sut.Col.Should().Be(expectedCol);
        sut.IsValid().Should().Be(isValid);
    }

    [Fact]
    public void Position_should_be_invalid_when_row_and_col_are_minus_1()
    {
        // arrange
        var sut = new Position(-1, -1);

        // act
        var isValid = sut.IsValid();

        // assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Position_should_be_valid_when_row_and_col_are_valid()
    {
        // arrange
        var sut = new Position(0, 1);

        // act
        var isValid = sut.IsValid();

        // assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Position_should_be_valid_when_position_is_for_main_board()
    {
        // arrange
        var sut = Position.MainBoard;

        // act
        var isValid = sut.IsValid();

        // assert
        isValid.Should().BeTrue();
    }
}
