using System.Text;

namespace TicTacToe.Tests.Helpers;

internal static class IssuedInstructionsAssertions
{
    internal static StringReader FirstIs(this StringBuilder instructions, string instruction)
    {
        var reader = new StringReader(instructions.ToString());
        reader.ReadLine().Should().Be(instruction);

        return reader;
    }

    internal static StringReader NextIs(this StringReader reader, string instruction)
    {
        reader.ReadLine().Should().Be(instruction);

        return reader;
    }

    internal static void NoMore(this StringReader reader)
    {
        reader.ReadLine().Should().BeNull();
    }
}
