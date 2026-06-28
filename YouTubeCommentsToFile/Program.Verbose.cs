using YouTubeCommentsToFile.Library;
using Console = TrueColorConsole.VTConsole;

namespace YouTubeCommentsToFile;

internal static partial class Program
{
    #region Write Verbose

    private static void WriteVerbose(IEnumerable<(string message, Color color)> messages)
    {
        if (messages.IsNullOrEmpty())
            return;

        int count = 1;

        foreach (var (message, color) in messages.SkipLast(1))
        {
            if (count++ > 1)
                Console.Write(" ");
            Write(message, color);
        }

        var last = messages.Last();
        if (count++ > 1)
            Console.Write(" ");
        WriteLine(last.message, last.color);
    }

    private static void WriteVerboseLines(IEnumerable<(string message, Color color)> messages)
    {
        if (messages.IsNullOrEmpty())
            return;

        foreach (var (message, color) in messages)
            WriteLine(message, color);
    }

    private static void WriteVerboseNewLine()
    {
        WriteVerboseLines([GetVerboseNewLine()]);
    }

    private static (string message, Color color) GetVerboseNewLine()
    {
        return (null, CommentsWriter.ValuesDefaultColor);
    }

    #endregion

    #region Write Property

    private static void WriteProperty(string property, params object[] values)
    {
        WriteVerbose(GetPropertyMessages(property, values));
    }

    private static void WriteProperty(string property, params string[] values)
    {
        WriteVerbose(GetPropertyMessages(property, values));
    }

    private static void WriteProperty(string property, Color valuesColor, params object[] values)
    {
        WriteVerbose(GetPropertyMessages(property, valuesColor, values));
    }

    private static void WriteProperty(string property, Color valuesColor, params string[] values)
    {
        WriteVerbose(GetPropertyMessages(property, valuesColor, values));
    }

    #endregion

    #region Write Property Lines

    private static void WritePropertyLines(string property, params object[] values)
    {
        WriteVerboseLines(GetPropertyMessages(property, values));
        WriteVerboseNewLine();
    }

    private static void WritePropertyLines(string property, params string[] values)
    {
        WriteVerboseLines(GetPropertyMessages(property, values));
        WriteVerboseNewLine();
    }

    private static void WritePropertyLines(string property, Color valuesColor, params object[] values)
    {
        WriteVerboseLines(GetPropertyMessages(property, valuesColor, values));
        WriteVerboseNewLine();
    }

    private static void WritePropertyLines(string property, Color valuesColor, params string[] values)
    {
        WriteVerboseLines(GetPropertyMessages(property, valuesColor, values));
        WriteVerboseNewLine();
    }

    #endregion

    #region Get Property Messages

    private static IEnumerable<(string message, Color color)> GetPropertyMessages(string property, params object[] values)
    {
        return GetPropertyMessages(property, CommentsWriter.PropertyDefaultColor, CommentsWriter.ValuesDefaultColor, [.. values.Select(value => value?.ToString())]);
    }

    private static IEnumerable<(string message, Color color)> GetPropertyMessages(string property, Color valuesColor, params object[] values)
    {
        return GetPropertyMessages(property, CommentsWriter.PropertyDefaultColor, valuesColor, [.. values.Select(value => value?.ToString())]);
    }

    private static IEnumerable<(string message, Color color)> GetPropertyMessages(string property, Color propertyColor, Color valuesColor, params object[] values)
    {
        return GetPropertyMessages(property, propertyColor, valuesColor, [.. values.Select(value => value?.ToString())]);
    }

    private static IEnumerable<(string message, Color color)> GetPropertyMessages(string property, params string[] values)
    {
        return GetPropertyMessages(property, CommentsWriter.PropertyDefaultColor, CommentsWriter.ValuesDefaultColor, values);
    }

    private static IEnumerable<(string message, Color color)> GetPropertyMessages(string property, Color valuesColor, params string[] values)
    {
        return GetPropertyMessages(property, CommentsWriter.PropertyDefaultColor, valuesColor, values);
    }

    private static IEnumerable<(string message, Color color)> GetPropertyMessages(string property, Color propertyColor, Color valuesColor, params string[] values)
    {
        if (string.IsNullOrWhiteSpace(property))
            return GetValueMessages(valuesColor, values);
        else
            return GetValueMessages(valuesColor, values).Prepend(GetPropertyMessage(property, propertyColor));
    }

    private static (string message, Color color) GetPropertyMessage(string property, Color propertyColor)
    {
        return ($"{property}:", propertyColor);
    }

    private static IEnumerable<(string message, Color color)> GetValueMessages(Color valuesColor, params string[] values)
    {
        return values.Select(message => (message, valuesColor));
    }

    #endregion
}
