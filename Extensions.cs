global using static GitUpdateAll.Extensions;

namespace GitUpdateAll;

public static class Extensions
{
    public static void LogError(string line = null) => Log(line, ConsoleColor.Red);
    public static void Log(string line = null)
    {
        Console.WriteLine(line);
    }
    public static void Log(string line, ConsoleColor color)
    {
        var foregroundColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(line);
        Console.ForegroundColor = foregroundColor;
    }
    public static PipeTarget 
        LogPipe = PipeTarget.ToDelegate(Log),
        LogErrorPipe = PipeTarget.ToDelegate(LogError);
    public static CommandTask<CommandResult> ExecuteAsync(this Command @this, params string[] args) => @this.WithArguments(args).ExecuteAsync();
    public static bool IsOk(this CommandResult @this) => @this.ExitCode == 0;
}