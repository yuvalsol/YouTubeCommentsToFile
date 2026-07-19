namespace YouTubeCommentsToFile;

internal static partial class Program
{
    private const ConsoleColor ERROR_COLOR = ConsoleColor.Red;
    private const ConsoleColor WARNING_COLOR = ConsoleColor.Yellow;
    private static readonly ConsoleColor FOREGROUND_COLOR;

    static Program()
    {
        FOREGROUND_COLOR = VALUES_COLOR = Console.ForegroundColor;
    }

    private static void WriteError(string value, IVerbosityOptions verbosityOptions = null)
    {
        Write(value, ERROR_COLOR, verbosityOptions);
    }

    private static void WriteError(StringBuilder value, IVerbosityOptions verbosityOptions = null)
    {
        Write(value, ERROR_COLOR, verbosityOptions);
    }

    private static void WriteErrorLine(string value, IVerbosityOptions verbosityOptions = null)
    {
        WriteLine(value, ERROR_COLOR, verbosityOptions);
    }

    private static void WriteErrorLine(StringBuilder value, IVerbosityOptions verbosityOptions = null)
    {
        WriteLine(value, ERROR_COLOR, verbosityOptions);
    }

    private static void WriteWarning(string value, IVerbosityOptions verbosityOptions = null)
    {
        Write(value, WARNING_COLOR, verbosityOptions);
    }

    private static void WriteWarning(StringBuilder value, IVerbosityOptions verbosityOptions = null)
    {
        Write(value, WARNING_COLOR, verbosityOptions);
    }

    private static void WriteWarningLine(string value, IVerbosityOptions verbosityOptions = null)
    {
        WriteLine(value, WARNING_COLOR, verbosityOptions);
    }

    private static void WriteWarningLine(StringBuilder value, IVerbosityOptions verbosityOptions = null)
    {
        WriteLine(value, WARNING_COLOR, verbosityOptions);
    }

    private static void Write(string value, ConsoleColor color, IVerbosityOptions verbosityOptions = null)
    {
        Console.ForegroundColor = color;
        Write(value, verbosityOptions);
        Console.ForegroundColor = FOREGROUND_COLOR;
    }

    private static void Write(StringBuilder value, ConsoleColor color, IVerbosityOptions verbosityOptions = null)
    {
        Console.ForegroundColor = color;
        Write(value, verbosityOptions);
        Console.ForegroundColor = FOREGROUND_COLOR;
    }

    private static void WriteLine(string value, ConsoleColor color, IVerbosityOptions verbosityOptions = null)
    {
        Console.ForegroundColor = color;
        WriteLine(value, verbosityOptions);
        Console.ForegroundColor = FOREGROUND_COLOR;
    }

    private static void WriteLine(StringBuilder value, ConsoleColor color, IVerbosityOptions verbosityOptions = null)
    {
        Console.ForegroundColor = color;
        WriteLine(value, verbosityOptions);
        Console.ForegroundColor = FOREGROUND_COLOR;
    }

    private static void Write(string value, IVerbosityOptions verbosityOptions = null)
    {
        GetOutputStream(verbosityOptions)?.Write(value);
    }

    private static void Write(StringBuilder value, IVerbosityOptions verbosityOptions = null)
    {
        GetOutputStream(verbosityOptions)?.Write(value);
    }

    private static void WriteLine(string value, IVerbosityOptions verbosityOptions = null)
    {
        GetOutputStream(verbosityOptions)?.WriteLine(value);
    }

    private static void WriteLine(StringBuilder value, IVerbosityOptions verbosityOptions = null)
    {
        GetOutputStream(verbosityOptions)?.WriteLine(value);
    }

    private static void WriteLine(IVerbosityOptions verbosityOptions = null)
    {
        GetOutputStream(verbosityOptions)?.WriteLine();
    }

    private static TextWriter GetOutputStream(IVerbosityOptions verbosityOptions = null)
    {
        return
            (verbosityOptions == null ? Console.Out :
            (verbosityOptions.Verbose && verbosityOptions.Quiet ? Console.Error :
            (verbosityOptions.Quiet ? null : Console.Out)));
    }

    private static void PressAnyKey(IVerbosityOptions verbosityOptions = null)
    {
        WriteLine("Press any key to continue . . .", verbosityOptions);
        Console.ReadKey(true);
    }
}
