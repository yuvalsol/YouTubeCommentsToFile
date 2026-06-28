using Console = TrueColorConsole.VTConsole;

namespace YouTubeCommentsToFile;

internal static partial class Program
{
    private static readonly Color errorColor = Color.FromArgb(231, 72, 86);
    private static readonly Color warningColor = Color.FromArgb(248, 241, 174);

    private static void WriteErrorLine(string line = null)
    {
        WriteLine(line, errorColor);
    }

    private static void WriteWarningLine(string line = null)
    {
        WriteLine(line, warningColor);
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
