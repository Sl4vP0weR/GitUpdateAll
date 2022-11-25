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
        bool.TryParse(args.ElementAtOrDefault(1), out var quiet);
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
            if (!(await IsRepository(localGit)))
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

    public static async Task<bool> IsRepository(Command git) => (await git.ExecuteAsync("status")).IsOk();
    public static async Task<bool> TryFetchAndPull(Command git)
    {
        return (await git.ExecuteAsync("fetch")).IsOk() &&
                (await git.ExecuteAsync("pull")).IsOk();
    }
}
