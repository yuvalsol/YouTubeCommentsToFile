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

    #endregion

    #region Write Property

    private static void WriteProperty(string name, params object[] values)
    {
        WriteVerbose(GetMessages(name, values));
    }

    private static void WriteProperty(string name, params string[] values)
    {
        WriteVerbose(GetMessages(name, values));
    }

    private static void WriteProperty(string name, Color valuesColor, params object[] values)
    {
        WriteVerbose(GetMessages(name, valuesColor, values));
    }

    private static void WriteProperty(string name, Color valuesColor, params string[] values)
    {
        WriteVerbose(GetMessages(name, valuesColor, values));
    }

    #endregion

    #region Write Property Lines

    private static void WritePropertyLines(string name, params object[] values)
    {
        WriteVerboseLines(GetMessages(name, values));
    }

    private static void WritePropertyLines(string name, params string[] values)
    {
        WriteVerboseLines(GetMessages(name, values));
    }

    private static void WritePropertyLines(string name, Color valuesColor, params object[] values)
    {
        WriteVerboseLines(GetMessages(name, valuesColor, values));
    }

    private static void WritePropertyLines(string name, Color valuesColor, params string[] values)
    {
        WriteVerboseLines(GetMessages(name, valuesColor, values));
    }

    #endregion

    #region Get Messages

    private static IEnumerable<(string message, Color color)> GetMessages(string name, params object[] values)
    {
        return GetMessages(name, CommentsWriter.NameDefaultColor, CommentsWriter.ValuesDefaultColor, [.. values.Select(value => value?.ToString())]);
    }

    private static IEnumerable<(string message, Color color)> GetMessages(string name, Color valuesColor, params object[] values)
    {
        return GetMessages(name, CommentsWriter.NameDefaultColor, valuesColor, [.. values.Select(value => value?.ToString())]);
    }

    private static IEnumerable<(string message, Color color)> GetMessages(string name, Color nameColor, Color valuesColor, params object[] values)
    {
        return GetMessages(name, nameColor, valuesColor, [.. values.Select(value => value?.ToString())]);
    }

    private static IEnumerable<(string message, Color color)> GetMessages(string name, params string[] values)
    {
        return GetMessages(name, CommentsWriter.NameDefaultColor, CommentsWriter.ValuesDefaultColor, values);
    }

    private static IEnumerable<(string message, Color color)> GetMessages(string name, Color valuesColor, params string[] values)
    {
        return GetMessages(name, CommentsWriter.NameDefaultColor, valuesColor, values);
    }

    private static IEnumerable<(string message, Color color)> GetMessages(string name, Color nameColor, Color valuesColor, params string[] values)
    {
        if (string.IsNullOrWhiteSpace(name))
            return values.Select(message => (message, valuesColor));
        else
            return values.Select(message => (message, valuesColor)).Prepend(($"{name}:", nameColor));
    }

    #endregion
}
