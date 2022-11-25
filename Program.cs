namespace GitUpdateAll;

public class Program
{
    private Program() { }
    public static readonly Program Instance = new();
    public static int Main(string[] args) => Instance.InstanceMain(args);
    private int InstanceMain(string[] args)
    {
        var task = InstanceAsyncMain(args);
        task.Wait();
        return task.Result;
    }
    private async Task<int> InstanceAsyncMain(string[] args)
    {
        var root = args.ElementAtOrDefault(0);
        if (string.IsNullOrWhiteSpace(root))
#if DEBUG
            root = @"D:\repos\";
#else
            root = Environment.CurrentDirectory;
#endif
        var hasErrors = false;
        var git = Cli.Wrap("git").WithValidation(CommandResultValidation.None);
        var directories = Directory.GetDirectories(root, "", SearchOption.TopDirectoryOnly);
        foreach (var directory in directories)
        {
            var localGit = git.WithWorkingDirectory(directory);
            if ((await localGit.ExecuteAsync("status")).ExitCode != 0)
                continue;
            Log(Path.GetFileName(directory) + Environment.NewLine); // current directory
            localGit = localGit.WithStandardErrorPipe(LogErrorPipe);
            if (await TryFetchAndPull(localGit))
                continue;
            else Log();

            hasErrors = true;
        }
        Log("Completed.", ConsoleColor.Cyan);
        Console.ReadKey();
        return hasErrors ? -1 : 0;
    }

    private static async Task<bool> TryFetchAndPull(Command git)
    {
        return (await git.ExecuteAsync("fetch")).IsOk() &&
                (await git.ExecuteAsync("pull")).IsOk();
    }
}
