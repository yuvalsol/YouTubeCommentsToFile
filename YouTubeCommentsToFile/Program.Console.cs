namespace YouTubeCommentsToFile;

internal partial class Program
{
    private static void WriteErrorLine(string line = null)
    {
        ConsoleColor foregroundColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(line);
        Console.ForegroundColor = foregroundColor;
    }

    private static void WriteErrorLine(StringBuilder sb)
    {
        ConsoleColor foregroundColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(sb);
        Console.ForegroundColor = foregroundColor;
    }

    private static void WriteWarningLine(string line = null)
    {
        ConsoleColor foregroundColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(line);
        Console.ForegroundColor = foregroundColor;
    }

    private static void WriteWarningLine(StringBuilder sb)
    {
        ConsoleColor foregroundColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Out.WriteLine(sb);
        Console.ForegroundColor = foregroundColor;
    }

    private static void PressAnyKey()
    {
        Console.WriteLine("Press any key to continue . . .");
        Console.ReadKey(true);
    }
}
