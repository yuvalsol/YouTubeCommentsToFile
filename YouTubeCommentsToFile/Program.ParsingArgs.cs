using CommandLine;
using YouTubeCommentsToFile.Library;

namespace YouTubeCommentsToFile;

internal static partial class Program
{
    #region Run

    private static int Run(string[] args, bool verbose = false)
    {
        int returnCode = 0;

        try
        {
            WriteVerboseCommandLine(args, verbose);

            var parser = GetParser();

            var (operationOptions, parsingResult) = ParseArgs(parser, args);
            if (parsingResult == ParsingResult.Error)
                returnCode = -1;
            if (parsingResult != ParsingResult.Success)
                return returnCode;

            if (operationOptions.StopConfig)
            {
                operationOptions.ConfigLocations = null;
            }
            else
            {
                string homeConfigurationDirectory = operationOptions.GetHomeConfigurationDirectory(FixPath);
                if (string.IsNullOrWhiteSpace(homeConfigurationDirectory))
                    homeConfigurationDirectory = null;
                else
                    WriteVerboseHomeConfigurationDirectory(homeConfigurationDirectory, verbose);

                AddConfigurationStackToConfigLocations(operationOptions, homeConfigurationDirectory, verbose);

                parsingResult = MergeConfigLocations(operationOptions, parser, verbose);
                if (parsingResult == ParsingResult.Error)
                    returnCode = -1;
            }

            if (parsingResult == ParsingResult.Success)
            {
                operationOptions.FixPaths(FixPath);
                WriteVerboseOptions(operationOptions.NameOf(), operationOptions.ToArgs(parser), verbose);
                new CommentsHandler(operationOptions.ToSettings()).WriteCommentsToFile();
            }
        }
        catch (ArgumentException ex)
        {
            returnCode = -1;
            WriteErrorLine(HandledArgumentException(ex).ToString());
            PressAnyKey();
        }
        catch (Exception ex)
        {
            returnCode = -1;
            WriteErrorLine(UnhandledException(ex).ToString());
            PressAnyKey();
        }
        finally
        {
            if (Debugger.IsAttached)
                PressAnyKey();
        }

        return returnCode;
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

    private static (IOperationOptions operationOptions, ParsingResult parsingResult) ParseArgs(Parser parser, string[] args)
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
            .WithNotParsed(errors => parsingResult = NotParsed(args, errors));

        return (operationOptions, parsingResult);
    }

    private static void AddQuotationsToYtDlpOptions(string[] args)
    {
        string ytDlpOptions = $"--{OptionsHelper.GetOptionLongName(nameof(IDownloadOptions.YtDlpOptions))}";

        var argYtDlpOptions = args.Select((arg, index) => (arg, index)).FirstOrDefault(x => string.Compare(x.arg, ytDlpOptions, StringComparison.OrdinalIgnoreCase) == 0);
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

    #endregion

    #region Not Parsed

    private static ParsingResult NotParsed(string[] args, IEnumerable<Error> errors)
    {
        if (errors.IsVersion())
        {
            WriteVersion();
            return ParsingResult.Version;
        }
        else if (errors.IsHelp())
        {
            WriteHelp();
            return ParsingResult.Help;
        }
        else if (errors.HasAny())
        {
            WriteParsingErrors(args, errors);
            return ParsingResult.Error;
        }

        return ParsingResult.ArgumentsNotParsed;
    }

    private static void WriteVersion()
    {
        Console.WriteLine(GetVersion());
    }

    private static string GetVersion()
    {
        return $"{nameof(YouTubeCommentsToFile)}, Version {Assembly.GetExecutingAssembly().GetName().Version.ToString(2)}";
    }

    private static void WriteHelp()
    {
        Console.WriteLine(GetHelp());
    }

    private static void WriteParsingErrors(string[] args, IEnumerable<Error> errors)
    {
        WriteErrorLine(GetVersion());
        WriteErrorLine($"{nameof(YouTubeCommentsToFile)} {string.Join(' ', args.Select(arg => arg.Contains(' ') ? $@"""{arg}""" : arg))}");
        WriteErrorLine($"{nameof(YouTubeCommentsToFile)} Errors:");
        foreach (Error error in errors)
        {
            string tag = error.Tag.ToString().ToWords();

            if (error is TokenError tokenError)
                WriteErrorLine($"{tag}: {tokenError.Token}");
            else if (error is NamedError namedError)
                WriteErrorLine($"{tag}: {namedError.NameInfo.NameText}");
            else
                WriteErrorLine(tag);
        }
        WriteErrorLine();
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

    private static void WriteVerboseCommandLine(string[] args, bool verbose)
    {
        WriteVerboseArgs("Command Line", args, verbose);
    }

    private static void WriteVerboseOptions(string optionsName, string[] args, bool verbose)
    {
        WriteVerboseArgs(optionsName.PrettifyName(), args, verbose);
    }

    private static void WriteVerboseArgs(string property, string[] args, bool verbose)
    {
        WriteVerboseArgs(property, CommentsWriter.ValuesDefaultColor, args, verbose);
    }

    private static void WriteVerboseArgs(string property, Color valuesColor, string[] args, bool verbose)
    {
        if (verbose == false)
            return;

        args = RearrangeArgs(args);

        string ignoreConfig = $"--{OptionsHelper.GetOptionLongName(nameof(IConfigurationOptions.IgnoreConfig))}";
        var ignoreConfigIndexes = args.Select((arg, i) => (arg, i)).Where(x => x.arg == ignoreConfig).Select(x => x.i);

        string stopConfig = $"--{OptionsHelper.GetOptionLongName(nameof(IConfigurationOptions.StopConfig))}";
        var stopConfigIndexes = args.Select((arg, i) => (arg, i)).Where(x => x.arg == stopConfig).Select(x => x.i);

        if (ignoreConfigIndexes.IsNullOrEmpty() && stopConfigIndexes.IsNullOrEmpty())
        {
            WritePropertyLines(property, args);
            return;
        }

        var messages = GetValueMessages(valuesColor, args);

        ColorArgs(ignoreConfigIndexes, Color.Yellow);
        ColorArgs(stopConfigIndexes, Color.Red);

        void ColorArgs(IEnumerable<int> indexes, Color color)
        {
            foreach (var index in indexes)
            {
                messages =
                    messages.Take(index)
                    .Append((messages.ElementAt(index).message, color))
                    .Concat(messages.Skip(index + 1));
            }
        }

        messages = messages.Prepend(GetPropertyMessage(property, CommentsWriter.PropertyDefaultColor)).Append(GetVerboseNewLine());

        WriteVerboseLines(messages);
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
