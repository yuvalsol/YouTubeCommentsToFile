namespace YouTubeCommentsToFile;

internal partial class Program
{
    private static StringBuilder HandledArgumentException(ArgumentException ex)
    {
        ex = ReplaceParamaterNameWithOptionLongName(ex);
        return new StringBuilder(ex.Message);
    }

    private static ArgumentException ReplaceParamaterNameWithOptionLongName(ArgumentException ex)
    {
        if (string.IsNullOrEmpty(ex.ParamName))
            return ex;

        string optionLongName = OptionsHelper.GetOptionLongName(ex.ParamName);
        if (string.IsNullOrEmpty(optionLongName))
            return ex;

        string message = ex.Message;
        int startIndex = message.IndexOf("(Parameter '");
        if (startIndex == -1)
            return ex;

        int endIndex = message.IndexOf("')", startIndex);
        if (endIndex == -1)
            return ex;

        message = message
            .Remove(startIndex, (endIndex + 2) - startIndex)
            .Insert(startIndex, $"(Option '{optionLongName}')");

        return new ArgumentException(message);
    }

    private static StringBuilder UnhandledException(Exception ex)
    {
        var errorMessage = new StringBuilder();

        try
        {
            errorMessage.Append(ex.GetUnhandledExceptionErrorMessage()).AppendLine();
        }
        catch
        {
            while (ex != null)
            {
                errorMessage.AppendLine().Append("ERROR: ").AppendLine(ex.Message);

#if DEBUG
                errorMessage.Append("ERROR TYPE: ").AppendLine(ex.GetType().ToString());
#endif

                ex = ex.InnerException;
            }
        }

        return errorMessage;
    }
}
