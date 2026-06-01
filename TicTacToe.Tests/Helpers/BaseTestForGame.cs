using System.Reflection;
using System.Text;

namespace TicTacToe.Tests.Helpers;

public class BaseTestForGame
{
    protected readonly ITestOutputHelper Logger;
    private readonly GameTestIO GameTestIO;
    protected readonly Game Game;
    private readonly string _pathForTests;

    internal BaseTestForGame(ITestOutputHelper logger)
    {
        Logger = logger;
        GameTestIO = new GameTestIO(logger);
        Game = Game.CreateGame(GameTestIO);
        Game.IsDebug = true;

        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        _pathForTests = Path.Combine(path, "IntergrationTests");
    }

    protected void SetInput(string input)
        => GameTestIO.SetInput(input);

    protected void SetInputFromFile(string fileName)
    {
        var lines = File.ReadAllLines(Path.Combine(_pathForTests, fileName));
        GameTestIO.SetInput(string.Join("\r\n", lines));
    }

    protected StringBuilder GetIssuedInstructions()
        => GameTestIO.GetAllIssuedInstructions();
}