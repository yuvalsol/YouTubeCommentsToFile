namespace YouTubeCommentsToFile;

internal static partial class Program
{
    #region Message Colors

    private const ConsoleColor PROPERTY_COLOR = ConsoleColor.Cyan;
    private static readonly ConsoleColor VALUES_COLOR;

    #endregion

    #region Property Messages

    private static IEnumerable<(string message, ConsoleColor color)> GetPropertyMessages(string property, object[] values)
    {
        return GetPropertyMessages(property, PROPERTY_COLOR, [.. values.Select(value => value?.ToString())], VALUES_COLOR);
    }

    private static IEnumerable<(string message, ConsoleColor color)> GetPropertyMessages(string property, object[] values, ConsoleColor valuesColor)
    {
        return GetPropertyMessages(property, PROPERTY_COLOR, [.. values.Select(value => value?.ToString())], valuesColor);
    }

    private static IEnumerable<(string message, ConsoleColor color)> GetPropertyMessages(string property, ConsoleColor propertyColor, object[] values, ConsoleColor valuesColor)
    {
        return GetPropertyMessages(property, propertyColor, [.. values.Select(value => value?.ToString())], valuesColor);
    }

    private static IEnumerable<(string message, ConsoleColor color)> GetPropertyMessages(string property, string[] values)
    {
        return GetPropertyMessages(property, PROPERTY_COLOR, values, VALUES_COLOR);
    }

    private static IEnumerable<(string message, ConsoleColor color)> GetPropertyMessages(string property, string[] values, ConsoleColor valuesColor)
    {
        return GetPropertyMessages(property, PROPERTY_COLOR, values, valuesColor);
    }

    private static IEnumerable<(string message, ConsoleColor color)> GetPropertyMessages(string property, ConsoleColor propertyColor, string[] values, ConsoleColor valuesColor)
    {
        if (string.IsNullOrWhiteSpace(property))
            return GetValueMessages(values, valuesColor);
        else
            return GetValueMessages(values, valuesColor).Prepend(GetPropertyMessage(property, propertyColor));
    }

    private static (string message, ConsoleColor color) GetPropertyMessage(string property, ConsoleColor propertyColor)
    {
        return ($"{property}:", propertyColor);
    }

    private static IEnumerable<(string message, ConsoleColor color)> GetValueMessages(string[] values, ConsoleColor valuesColor)
    {
        return values.Select(message => (message, valuesColor));
    }

    #endregion

    #region Write Messages

    private static void WriteMessages(IEnumerable<IEnumerable<(string message, ConsoleColor color)>> messagesGroups, IVerbosityOptions verbosityOptions, bool singleLine)
    {
        if (singleLine)
        {
            foreach (var messages in messagesGroups)
                WriteMessages(messages, verbosityOptions, singleLine);
        }
        else
        {
            foreach (var messages in messagesGroups)
            {
                WriteMessages(messages, verbosityOptions, singleLine);
                WriteLine(verbosityOptions);
            }
        }
    }

    private static void WriteMessages(IEnumerable<(string message, ConsoleColor color)> messages, IVerbosityOptions verbosityOptions, bool singleLine)
    {
        if (singleLine)
        {
            foreach (var (message, color) in messages.SkipLast(1))
            {
                Write(message, color, verbosityOptions);
                Write(" ", verbosityOptions);
            }

            var last = messages.Last();
            WriteLine(last.message, last.color, verbosityOptions);
        }
        else
        {
            foreach (var (message, color) in messages)
                WriteLine(message, color, verbosityOptions);
        }
    }

    #endregion
}
