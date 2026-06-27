using Console = TrueColorConsole.VTConsole;

namespace YouTubeCommentsToFile;

internal static partial class Program
{
    private static void WriteErrorLine(string line = null)
    {
        WriteLine(line, Color.Red);
    }

    private static void WriteWarningLine(string line = null)
    {
        WriteLine(line, Color.Yellow);
    }

    private static void Write(string line, Color color)
    {
        Console.Write(line, color);
    }

    private static void WriteLine(string line, Color color)
    {
        Console.WriteLine(line, color);
    }

    private static void PressAnyKey()
    {
        Console.WriteLine("Press any key to continue . . .");
        System.Console.ReadKey(true);
    }
}
