using CommandLine;

namespace YouTubeCommentsToFile;

internal partial class Program(string[] args)
{
    #region Fields

    private Parser parser = GetParser();
    private IOperationOptions operationOptions;
    private string homeConfigurationDirectory;
    private List<(string message, ConsoleColor color)[]> verboseMessages = [];
    private bool finishedWithErrors;

    #endregion

    #region Run

    private int Run()
    {
        try
        {
            WriteVerboseCommandLine();

            var (parsingOptions, parsingResult) = ParseArgs(args);
            operationOptions = parsingOptions;

            if (parsingResult == ParsingResult.Version)
            {
                WriteVersion();
                return 0;
            }
            else if (parsingResult == ParsingResult.Help)
            {
                WriteHelp();
                return 0;
            }
            else if (parsingResult == ParsingResult.Error)
            {
                finishedWithErrors = true;
                return -1;
            }

            if (operationOptions.StopConfig == false)
            {
                homeConfigurationDirectory = operationOptions.GetHomeConfigurationDirectory(FixPath);
                if (string.IsNullOrWhiteSpace(homeConfigurationDirectory))
                    homeConfigurationDirectory = null;
                else
                    WriteVerboseHomeConfigurationDirectory();

                AddConfigurationStackToConfigLocations();

                ParsingResult mergeParsingResult = MergeConfigLocations();
                if (mergeParsingResult == ParsingResult.Error)
                    parsingResult = ParsingResult.Error;
            }

            operationOptions.FixPaths(FixPath);
            WriteVerboseOptions();

            if (parsingResult == ParsingResult.Error)
            {
                if (operationOptions.Verbose)
                    WriteMessages(verboseMessages, singleLine: false);
                finishedWithErrors = true;
                return -1;
            }

            if (operationOptions.Version)
            {
                WriteVersion();
            }
            else if (operationOptions.Help)
            {
                WriteHelp();
            }
            else
            {
                if (operationOptions.Verbose)
                    WriteMessages(verboseMessages, singleLine: false);

                WriteCommentsToFile();
            }

            return 0;
        }
        catch (ArgumentException ex)
        {
            operationOptions ??= GetAdHocOperationOptions();
            WriteErrorLine(HandledArgumentException(ex));
            finishedWithErrors = true;
            return -1;
        }
        catch (Exception ex)
        {
            operationOptions ??= GetAdHocOperationOptions();
            WriteErrorLine(UnhandledException(ex));
            finishedWithErrors = true;
            return -1;
        }
        finally
        {
            operationOptions ??= GetAdHocOperationOptions();

            if (operationOptions.Quiet == false && (operationOptions.Beep || (operationOptions.BeepOnErrors && finishedWithErrors)))
                Console.Beep();

            if (Debugger.IsAttached)
                PressAnyKey();
        }
    }

    private IOperationOptions GetAdHocOperationOptions()
    {
        string verbose1 = OptionsHelper.GetOptionShortName(nameof(IVerbosityOptions.Verbose));
        string verbose2 = OptionsHelper.GetOptionLongName(nameof(IVerbosityOptions.Verbose));

        string quiet1 = OptionsHelper.GetOptionShortName(nameof(IVerbosityOptions.Quiet));
        string quiet2 = OptionsHelper.GetOptionLongName(nameof(IVerbosityOptions.Quiet));

        string ytDlpQuiet = OptionsHelper.GetOptionLongName(nameof(IVerbosityOptions.YtDlpQuiet));

        string beep = OptionsHelper.GetOptionLongName(nameof(IGeneralOptions.Beep));
        string beepOnErrors = OptionsHelper.GetOptionLongName(nameof(IGeneralOptions.BeepOnErrors));

        return new ConvertOptions()
        {
            Verbose = args.HasAny(arg => arg == verbose1 || arg == verbose2),
            Quiet = args.HasAny(arg => arg == quiet1 || arg == quiet2),
            YtDlpQuiet = args.HasAny(arg => arg == ytDlpQuiet),
            Beep = args.HasAny(arg => arg == beep),
            BeepOnErrors = args.HasAny(arg => arg == beepOnErrors)
        };
    }

    #endregion

    #region Parser

    private static Parser GetParser()
    {
        return new Parser(with =>
        {
            with.AllowMultiInstance = true;
            with.IgnoreUnknownArguments = true;
            with.HelpWriter = null;
        });
    }

    #endregion

    #region Parse Args

    private (IOperationOptions parsingOptions, ParsingResult parsingResult) ParseArgs(string[] args)
    {
        IOperationOptions parsingOptions = null;
        ParsingResult parsingResult = ParsingResult.ArgumentsNotParsed;

        AddQuotationsToYtDlpOptions();

        parser.ParseArguments<ConvertOptions, DownloadOptions>(args)
            .WithParsed<ConvertOptions>(options =>
            {
                parsingOptions = options;
                parsingResult = ParsingResult.Success;
            })
            .WithParsed<DownloadOptions>(options =>
            {
                RemoveQuotationsFromYtDlpOptions(options);
                parsingOptions = options;
                parsingResult = ParsingResult.Success;
            })
            .WithNotParsed(errors =>
            {
                parsingResult =
                    (errors.IsVersion() ? ParsingResult.Version :
                    (errors.IsHelp() ? ParsingResult.Help :
                    (errors.HasAny() ? ParsingResult.Error :
                    ParsingResult.ArgumentsNotParsed)));

                if (parsingResult == ParsingResult.Error)
                    WriteParsingErrors(errors);
            });

        return (parsingOptions, parsingResult);
    }

    private void AddQuotationsToYtDlpOptions()
    {
        string ytDlpOptions = OptionsHelper.GetOptionLongName(nameof(IDownloadOptions.YtDlpOptions));
        var argYtDlpOptions = args.Select((arg, index) => (arg, index)).FirstOrDefault(x => string.Compare(x.arg, ytDlpOptions) == 0);
        if (argYtDlpOptions == default)
            return;

        int index = argYtDlpOptions.index + 1;
        if (index >= args.Length)
            return;

        string value = args[index];
        if ((value.StartsWith('"') && value.EndsWith('"')) == false)
            args[index] = $@"""{value}""";
    }

    private static void RemoveQuotationsFromYtDlpOptions(DownloadOptions options)
    {
        if (string.IsNullOrEmpty(options.YtDlpOptions))
            return;

        if (options.YtDlpOptions.StartsWith('"') && options.YtDlpOptions.EndsWith('"'))
            options.YtDlpOptions = options.YtDlpOptions[1..^1];
    }

    private static void WriteVersion()
    {
        Console.WriteLine(GetVersion());
    }

    private static string GetVersion()
    {
        return $"{nameof(YouTubeCommentsToFile)}, Version {Assembly.GetExecutingAssembly().GetName().Version.ToString(2)}";
    }

    private void WriteHelp()
    {
        Console.WriteLine(GetHelp());
    }

    private void WriteParsingErrors(IEnumerable<Error> errors)
    {
        List<string> errorMessages = [
            GetVersion(),
            $"{nameof(YouTubeCommentsToFile)} {string.Join(' ', args.Select(arg => arg.Contains(' ') ? $@"""{arg}""" : arg))}",
            $"{nameof(YouTubeCommentsToFile)} Errors:"
        ];

        foreach (var error in errors)
        {
            string tag = error.Tag.ToString().ToWords();

            if (error is TokenError tokenError)
                errorMessages.Add($"{tag}: {tokenError.Token}");
            else if (error is NamedError namedError)
                errorMessages.Add($"{tag}: {namedError.NameInfo.NameText}");
            else
                errorMessages.Add(tag);
        }

        verboseMessages.Add([.. errorMessages.Select(errorMessage => (errorMessage, ERROR_COLOR))]);
    }

    #endregion

    #region Parsing Result

    private enum ParsingResult
    {
        Success = 0,
        Version = 1,
        Help = 2,
        ArgumentsNotParsed = 3,
        Error = -1
    }

    #endregion

    #region Write Verbose Args

    private void WriteVerboseCommandLine()
    {
        WriteVerboseArgs("Command Line", args);
    }

    private void WriteVerboseOptions()
    {
        WriteVerboseArgs(operationOptions.NameOf().PrettifyName(), operationOptions.ToArgs(parser));
    }

    private void WriteVerboseArgs(string property, string[] args)
    {
        WriteVerboseArgs(property, args, VALUES_COLOR);
    }

    private void WriteVerboseArgs(string property, string[] args, ConsoleColor valuesColor)
    {
        if (args.IsNullOrEmpty())
            return;

        args = [.. args.RearrangeArgs().OrderByPrecedence()];

        string ignoreConfig = OptionsHelper.GetOptionLongName(nameof(IConfigurationOptions.IgnoreConfig));
        var ignoreConfigIndexes = args.Select((arg, i) => (arg, i)).Where(x => x.arg == ignoreConfig).Select(x => x.i);

        string stopConfig = OptionsHelper.GetOptionLongName(nameof(IConfigurationOptions.StopConfig));
        var stopConfigIndexes = args.Select((arg, i) => (arg, i)).Where(x => x.arg == stopConfig).Select(x => x.i);

        if (ignoreConfigIndexes.IsNullOrEmpty() && stopConfigIndexes.IsNullOrEmpty())
        {
            verboseMessages.Add([.. GetPropertyMessages(property, args, valuesColor)]);
            return;
        }

        var messages = GetValueMessages(args, valuesColor);

        ColorArgs(ignoreConfigIndexes, ConsoleColor.DarkYellow);
        ColorArgs(stopConfigIndexes, ConsoleColor.DarkRed);

        void ColorArgs(IEnumerable<int> indexes, ConsoleColor color)
        {
            foreach (var index in indexes)
            {
                messages =
                    messages.Take(index)
                    .Append((messages.ElementAt(index).message, color))
                    .Concat(messages.Skip(index + 1));
            }
        }

        messages = messages.Prepend(GetPropertyMessage(property, PROPERTY_COLOR));
        verboseMessages.Add([.. messages]);
    }

    #endregion
}
