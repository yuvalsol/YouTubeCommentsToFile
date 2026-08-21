namespace YouTubeCommentsToFile;

internal partial class Program
{
    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

#if RELEASE
        return new Program(args).Run();
#elif DEBUG
        Console.Clear();
        return Debug();
#endif
    }
}
