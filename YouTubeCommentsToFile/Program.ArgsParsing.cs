using CommandLine;

namespace YouTubeCommentsToFile;

internal static partial class Program
{
    #region Run

    private static int Run(string[] args)
    {
        IOperationOptions operationOptions = null;
        List<(string message, ConsoleColor color)[]> verboseMessages = [];

        try
        {
            WriteVerboseCommandLine(args, verboseMessages);

            var parser = GetParser();

            var (parsingOptions, parsingResult) = ParseArgs(parser, args, verboseMessages);
            operationOptions = parsingOptions;

            if (parsingResult == ParsingResult.Version)
            {
                WriteVersion();
                return 0;
            }
            else if (parsingResult == ParsingResult.Help)
            {
                WriteHelp(parser);
                return 0;
            }
            else if (parsingResult == ParsingResult.Error)
            {
                return -1;
            }

            if (operationOptions.StopConfig == false)
            {
                string homeConfigurationDirectory = operationOptions.GetHomeConfigurationDirectory(FixPath);
                if (string.IsNullOrWhiteSpace(homeConfigurationDirectory))
                    homeConfigurationDirectory = null;
                else
                    WriteVerboseHomeConfigurationDirectory(homeConfigurationDirectory, verboseMessages);

                AddConfigurationStackToConfigLocations(operationOptions, homeConfigurationDirectory, verboseMessages);

                ParsingResult mergeParsingResult = MergeConfigLocations(operationOptions, parser, verboseMessages);
                if (mergeParsingResult == ParsingResult.Error)
                    parsingResult = ParsingResult.Error;
            }

            operationOptions.FixPaths(FixPath);
            WriteVerboseOptions(operationOptions.NameOf().PrettifyName(), operationOptions.ToArgs(parser), verboseMessages);

            if (parsingResult == ParsingResult.Error)
            {
                if (operationOptions.Verbose)
                    WriteMessages(verboseMessages, operationOptions, singleLine: false);
                return -1;
            }

            if (operationOptions.Version)
            {
                WriteVersion();
            }
            else if (operationOptions.Help)
            {
                WriteHelp(parser);
            }
            else
            {
                if (operationOptions.Verbose)
                    WriteMessages(verboseMessages, operationOptions, singleLine: false);
                WriteCommentsToFile(operationOptions);
            }

            return 0;
        }
        catch (ArgumentException ex)
        {
            WriteErrorLine(HandledArgumentException(ex), operationOptions ?? GetOperationOptions(args));
            return -1;
        }
        catch (Exception ex)
        {
            WriteErrorLine(UnhandledException(ex), operationOptions ?? GetOperationOptions(args));
            return -1;
        }
        finally
        {
            var options = operationOptions ?? GetOperationOptions(args);

            if (options.Beep && options.Quiet == false)
                Console.Beep();

            if (Debugger.IsAttached)
                PressAnyKey(options);
        }
    }

    private static IOperationOptions GetOperationOptions(string[] args)
    {
        string verbose1 = OptionsHelper.GetOptionShortName(nameof(IVerbosityOptions.Verbose));
        string verbose2 = OptionsHelper.GetOptionLongName(nameof(IVerbosityOptions.Verbose));

        string quiet1 = OptionsHelper.GetOptionShortName(nameof(IVerbosityOptions.Quiet));
        string quiet2 = OptionsHelper.GetOptionLongName(nameof(IVerbosityOptions.Quiet));

        string beep = OptionsHelper.GetOptionLongName(nameof(IGeneralOptions.Beep));

        return new ConvertOptions()
        {
            Verbose = args.HasAny(arg => arg == verbose1 || arg == verbose2),
            Quiet = args.HasAny(arg => arg == quiet1 || arg == quiet2),
            Beep = args.HasAny(arg => arg == beep)
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

    private static (IOperationOptions operationOptions, ParsingResult parsingResult) ParseArgs(Parser parser, string[] args, List<(string message, ConsoleColor color)[]> verboseMessages)
    {
        IOperationOptions operationOptions = null;
        ParsingResult parsingResult = ParsingResult.ArgumentsNotParsed;

        AddQuotationsToYtDlpOptions(args);

        parser.ParseArguments<ConvertOptions, DownloadOptions>(args)
            .WithParsed<ConvertOptions>(options =>
            {
                operationOptions = options;
                parsingResult = ParsingResult.Success;
            })
            .WithParsed<DownloadOptions>(options =>
            {
                RemoveQuotationsFromYtDlpOptions(options);
                operationOptions = options;
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
                    WriteParsingErrors(errors, args, verboseMessages);
            });

        return (operationOptions, parsingResult);
    }

    private static void AddQuotationsToYtDlpOptions(string[] args)
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
        WriteLine(GetVersion());
    }

    private static string GetVersion()
    {
        return $"{nameof(YouTubeCommentsToFile)}, Version {Assembly.GetExecutingAssembly().GetName().Version.ToString(2)}";
    }

    private static void WriteHelp(Parser parser)
    {
        WriteLine(GetHelp(parser));
    }

    private static void WriteParsingErrors(IEnumerable<Error> errors, string[] args, List<(string message, ConsoleColor color)[]> verboseMessages)
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

    private static void WriteVerboseCommandLine(string[] args, List<(string message, ConsoleColor color)[]> verboseMessages)
    {
        WriteVerboseArgs("Command Line", args, verboseMessages);
    }

    private static void WriteVerboseOptions(string optionsName, string[] args, List<(string message, ConsoleColor color)[]> verboseMessages)
    {
        WriteVerboseArgs(optionsName, args, verboseMessages);
    }

    private static void WriteVerboseArgs(string property, string[] args, List<(string message, ConsoleColor color)[]> verboseMessages)
    {
        WriteVerboseArgs(property, args, VALUES_COLOR, verboseMessages);
    }

    private static void WriteVerboseArgs(string property, string[] args, ConsoleColor valuesColor, List<(string message, ConsoleColor color)[]> verboseMessages)
    {
        if (args.IsNullOrEmpty())
            return;

        args = RearrangeArgs(args);

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

    /* RegexIsOption:
     *
     * short name: -[A-Za-z0-9]
     * single dash, single char
     * or
     * long name: --[A-Za-z0-9][A-Za-z0-9-]*
     * double dash, first char is not dash
     */
    [GeneratedRegex(@"^(?:-[A-Za-z0-9]|--[A-Za-z0-9][A-Za-z0-9-]*)$")]
    private static partial Regex RegexIsOption();

    private static string[] RearrangeArgs(string[] args)
    {
        int counter = 0;

        return [.. args
            .Select((arg, i) => (arg, n: RegexIsOption().IsMatch(arg) ? ++counter : counter))
            .GroupBy(x => x.n)
            .Select(g =>
                (arg: string.Join(' ', g.Skip(1).Select(x =>
                {
                     if (x.arg.Contains(' '))
                         return $@"""{x.arg}""";
                     return x.arg;
                })
                .Prepend(g.First().arg)), n: g.Key)
            )
            .OrderBy(x => x.n)
            .Select(x => x.arg)];
    }

    #endregion
}
