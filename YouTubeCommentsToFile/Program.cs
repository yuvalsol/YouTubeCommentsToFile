namespace YouTubeCommentsToFile;

internal static partial class Program
{
    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Clear();

#if RELEASE
        return Run(args);
#elif DEBUG
        return Debug();
#endif
    }
}
