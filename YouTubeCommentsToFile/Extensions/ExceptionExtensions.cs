#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static partial class ExceptionExtensions
{
    public static StringBuilder GetUnhandledExceptionErrorWithApplicationTerminationMessage(this Exception ex, bool withoutDebuggingInfo = false)
    {
        if (ex == null)
            return null;

        var errorMessage = new StringBuilder();
        errorMessage.AppendLine("An unhandled error occurred.");
        errorMessage.AppendLine("The application will terminate now.");
        errorMessage.AppendLine();
        return GetExceptionErrorMessage(errorMessage, ex, withoutDebuggingInfo);
    }

    public static StringBuilder GetUnhandledExceptionErrorMessage(this Exception ex, bool withoutDebuggingInfo = false)
    {
        if (ex == null)
            return null;

        var errorMessage = new StringBuilder();
        errorMessage.AppendLine("An unhandled error occurred.");
        errorMessage.AppendLine();
        return GetExceptionErrorMessage(errorMessage, ex, withoutDebuggingInfo);
    }

    public static StringBuilder GetExceptionErrorMessage(this Exception ex, string mainMessage = null, bool withoutDebuggingInfo = false)
    {
        if (ex == null)
            return null;

        var errorMessage = new StringBuilder();
        if (string.IsNullOrEmpty(mainMessage) == false)
            errorMessage.AppendLine(mainMessage);
        return GetExceptionErrorMessage(errorMessage, ex, withoutDebuggingInfo);
    }

    private static StringBuilder GetExceptionErrorMessage(StringBuilder errorMessage, Exception ex, bool withoutDebuggingInfo)
    {
        AppendToExceptionErrorMessage(errorMessage, ex, withoutDebuggingInfo);

        while (ex.InnerException != null)
        {
            ex = ex.InnerException;
            AppendToExceptionErrorMessage(errorMessage, ex, withoutDebuggingInfo);
        }

        TrimEndNewLine(errorMessage);

        return errorMessage;
    }

    private static void AppendToExceptionErrorMessage(StringBuilder errorMessage, Exception ex, bool withoutDebuggingInfo)
    {
        errorMessage.Append("ERROR: ");
        errorMessage.AppendLine(ex.Message);

        if (withoutDebuggingInfo)
            return;

        errorMessage.Append("ERROR TYPE: ");
        errorMessage.AppendLine(ex.GetType().ToString());

        var stackTrace = ex.GetFormattedStackTrace();
        if (stackTrace.Length > 0)
        {
            errorMessage.AppendLine("STACK TRACE:");
            errorMessage.Append(stackTrace);
            errorMessage.AppendLine();
        }
    }

    public static StringBuilder GetFormattedStackTrace(this Exception ex)
    {
        var stackTrace = new StringBuilder();

        bool isFoundNamespaceNotSystem = false;

        var st = new StackTrace(ex, true);
        for (int i = 0; i < st.FrameCount; i++)
        {
            StackFrame sf = st.GetFrame(i);
            if (sf != null)
            {
                MethodBase method = sf.GetMethod();
                if (method != null)
                {
                    Type reflectedType = method.ReflectedType;
                    if (reflectedType != null)
                    {
                        bool isNotSystemNamespace = string.IsNullOrEmpty(reflectedType.Namespace) || reflectedType.Namespace.StartsWith("System") == false;

                        if (isFoundNamespaceNotSystem == false || isNotSystemNamespace)
                        {
                            isFoundNamespaceNotSystem = isNotSystemNamespace;

                            MethodInfo mi = method as MethodInfo;
                            if (mi != null)
                            {
                                stackTrace.Append(i + 1);
                                stackTrace.Append(". ");
                                stackTrace.Append(mi.GetSignature());

                                int lineNumber = sf.GetFileLineNumber();
                                if (lineNumber > 0)
                                {
                                    stackTrace.Append(" at line ");
                                    stackTrace.Append(lineNumber);
                                }

                                stackTrace.Append(Environment.NewLine);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        TrimEndNewLine(stackTrace);

        return stackTrace;
    }

    private static void TrimEndNewLine(StringBuilder sb)
    {
        int countCharsToRemove = 0;

        if (sb.Length >= 2 && sb[^2] == '\r')
            countCharsToRemove++;

        if (sb.Length >= 1 && sb[^1] == '\n')
            countCharsToRemove++;

        if (countCharsToRemove == 0)
            return;

        sb.Length -= countCharsToRemove;
    }
}
