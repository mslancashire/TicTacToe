using System.Text;

namespace TicTacToe.Tests.Helpers;

internal class GameTestIO : IGameIO
{
    private StringReader? _reader;
    private readonly StringBuilder _writer;
    private readonly ITestOutputHelper _logger;

    internal GameTestIO(ITestOutputHelper logger)
    {
        _writer = new StringBuilder();
        _logger = logger;
    }

    public void MakeMove(Position position)
    {
        IssueInstruction($"{position.Row} {position.Col}");
    }

    public void SetInput(string input)
        => _reader = new StringReader(input);

    public void IssueInstruction(string instruction)
    {
        _writer.AppendLine(instruction);
    }

    public string ReadInput()
    {
        if (_reader is null)
        {
            return string.Empty;
        }

        return _reader.ReadLine() ?? string.Empty;
    }

    public StringBuilder GetAllIssuedInstructions()
        => _writer;

    public void LogInfo(string message, LogType logType)
        => _logger.WriteLine($"{logType} => {message}.");
}
