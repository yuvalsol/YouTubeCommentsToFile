namespace YouTubeCommentsToFile;

internal partial class Program
{
    private const ConsoleColor ERROR_COLOR = ConsoleColor.Red;
    private const ConsoleColor WARNING_COLOR = ConsoleColor.Yellow;
    private static readonly ConsoleColor FOREGROUND_COLOR;

    static Program()
    {
        FOREGROUND_COLOR = VALUES_COLOR = Console.ForegroundColor;
    }

    private void WriteError(string value)
    {
        Write(value, ERROR_COLOR);
    }

    private void WriteError(StringBuilder value)
    {
        Write(value, ERROR_COLOR);
    }

    private void WriteErrorLine(string value)
    {
        WriteLine(value, ERROR_COLOR);
    }

    private void WriteErrorLine(StringBuilder value)
    {
        WriteLine(value, ERROR_COLOR);
    }

    private void WriteWarning(string value)
    {
        Write(value, WARNING_COLOR);
    }

    private void WriteWarning(StringBuilder value)
    {
        Write(value, WARNING_COLOR);
    }

    private void WriteWarningLine(string value)
    {
        WriteLine(value, WARNING_COLOR);
    }

    private void WriteWarningLine(StringBuilder value)
    {
        WriteLine(value, WARNING_COLOR);
    }

    private void Write(string value, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Write(value);
        Console.ForegroundColor = FOREGROUND_COLOR;
    }

    private void Write(StringBuilder value, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Write(value);
        Console.ForegroundColor = FOREGROUND_COLOR;
    }

    private void WriteLine(string value, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        WriteLine(value);
        Console.ForegroundColor = FOREGROUND_COLOR;
    }

    private void WriteLine(StringBuilder value, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        WriteLine(value);
        Console.ForegroundColor = FOREGROUND_COLOR;
    }

    private void Write(string value)
    {
        GetOutputStream()?.Write(value);
    }

    private void Write(StringBuilder value)
    {
        GetOutputStream()?.Write(value);
    }

    private void WriteLine(string value)
    {
        GetOutputStream()?.WriteLine(value);
    }

    private void WriteLine(StringBuilder value)
    {
        GetOutputStream()?.WriteLine(value);
    }

    private void WriteLine()
    {
        GetOutputStream()?.WriteLine();
    }

    private TextWriter GetOutputStream()
    {
        return
            (operationOptions == null ? Console.Out :
            (operationOptions.Verbose && operationOptions.Quiet ? Console.Error :
            (operationOptions.Quiet ? null : Console.Out)));
    }

    private void PressAnyKey()
    {
        WriteLine("Press any key to continue . . .");
        Console.ReadKey(true);
    }
}
