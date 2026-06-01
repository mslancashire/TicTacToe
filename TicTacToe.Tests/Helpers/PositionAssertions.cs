namespace TicTacToe.Tests.Helpers;

internal static class PositionAssertions
{
    internal static void ShouldBeInvalid(this Position position)
    {
        position.ShouldBe(-1, -1);
        position.IsValid().Should().BeFalse();
    }

    internal static IEnumerator<TType> FirstIs<TType>(this IEnumerable<TType> items, Action<TType> check)
    {
        var enumerator = items.GetEnumerator();
        enumerator.MoveNext().Should().BeTrue();

        enumerator.Current.Should().Satisfy(check);
        
        return enumerator;
    }

    internal static IEnumerator<TType> NextIs<TType>(this IEnumerator<TType> enumerator, Action<TType> check)
    {
        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Should().Satisfy(check);

        return enumerator;
    }

    internal static IEnumerator<Position> FirstIs(this IEnumerable<Position> positions, int row, int col)
    {   
        var enumerator = positions.GetEnumerator();
        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.ShouldBe(row, col);
        return enumerator;
    }

    internal static IEnumerator<Position> NextIs(this IEnumerator<Position> positions, int row, int col)
    {
        positions.MoveNext().Should().BeTrue();
        positions.Current.ShouldBe(row, col);

        return positions;
    }

    internal static void NoMore<TType>(this IEnumerator<TType> enumerator)
    {
        enumerator.MoveNext().Should().BeFalse();
    }

    internal static void ShouldBe(this Position position, int row, int col)
    {
        position.Row.Should().Be(row);
        position.Col.Should().Be(col);
    }
}
