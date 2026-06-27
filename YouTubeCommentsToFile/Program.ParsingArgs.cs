using CommandLine;
using YouTubeCommentsToFile.Library;

namespace YouTubeCommentsToFile;

internal static partial class Program
{
    private static int Run(string[] args, bool verbose = false)
    {
        int returnCode = 0;

        try
        {
            if (verbose)
                WritePropertyLines("Command Line", RearrangeArgs(args));

            var parser = GetParser();

            var parsingResult = ParseArgs(parser, args, out ConvertOptions convertOptions, out DownloadOptions downloadOptions);
            if (parsingResult == ParsingResult.Error)
                returnCode = -1;
            if (parsingResult != ParsingResult.Success)
                return returnCode;

            Settings settings = null;

            if (convertOptions != null)
            {
                string homeConfigurationDirectory = null;
                if (string.IsNullOrWhiteSpace(convertOptions.JsonFile) == false)
                    homeConfigurationDirectory = Path.GetDirectoryName(convertOptions.JsonFile);

                AddConfigurationStackToConfigLocations(convertOptions, homeConfigurationDirectory);

                parsingResult = MergeConfigLocations(convertOptions, parser, verbose);
                if (parsingResult == ParsingResult.Success)
                {
                    settings = convertOptions.ToSettings();
                    if (verbose)
                        WritePropertyLines(nameof(convertOptions).PrettifyName(), RearrangeArgs(convertOptions.ToArgs(parser)));
                }
            }
            else if (downloadOptions != null)
            {
                string homeConfigurationDirectory = null;
                if (string.IsNullOrWhiteSpace(downloadOptions.DownloadPath) == false)
                    homeConfigurationDirectory = Path.TrimEndingDirectorySeparator(downloadOptions.DownloadPath);

                AddConfigurationStackToConfigLocations(downloadOptions, homeConfigurationDirectory);

                parsingResult = MergeConfigLocations(downloadOptions, parser, verbose);
                if (parsingResult == ParsingResult.Success)
                {
                    settings = downloadOptions.ToSettings();
                    if (verbose)
                        WritePropertyLines(nameof(downloadOptions).PrettifyName(), RearrangeArgs(downloadOptions.ToArgs(parser)));
                }
            }

            if (parsingResult == ParsingResult.Success)
                new CommentsHandler(settings).WriteCommentsToFile();
            else if (parsingResult == ParsingResult.Error)
                returnCode = -1;
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

    private static Parser GetParser()
    {
        return new Parser(with =>
        {
            with.IgnoreUnknownArguments = true;
            with.HelpWriter = null;
        });
    }

    private static ParsingResult ParseArgs(Parser parser, string[] args, out ConvertOptions convertOptions)
    {
        return ParseArgs(parser, args, out convertOptions, out _);
    }

    private static ParsingResult ParseArgs(Parser parser, string[] args, out DownloadOptions downloadOptions)
    {
        return ParseArgs(parser, args, out _, out downloadOptions);
    }

    private static ParsingResult ParseArgs(Parser parser, string[] args, out ConvertOptions convertOptions, out DownloadOptions downloadOptions)
    {
        ParsingResult parsingResult = ParsingResult.ArgumentsNotParsed;

        ConvertOptions tempConvertOptions = null;
        DownloadOptions tempDownloadOptions = null;

        AddQuotationsToYtDlpOptions(args);

        parser.ParseArguments<ConvertOptions, DownloadOptions>(args)
            .WithParsed<ConvertOptions>(options =>
            {
                tempConvertOptions = options;
                parsingResult = ParsingResult.Success;
            })
            .WithParsed<DownloadOptions>(options =>
            {
                RemoveQuotationsFromYtDlpOptions(options);
                tempDownloadOptions = options;
                parsingResult = ParsingResult.Success;
            })
            .WithNotParsed(errors => parsingResult = NotParsed(args, errors));

        convertOptions = tempConvertOptions;
        downloadOptions = tempDownloadOptions;

        return parsingResult;
    }

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
        Console.WriteLine();
    }

    private static void WriteHelp()
    {
        Console.WriteLine(GetHelp());
    }

    private static void WriteParsingErrors(string[] args, IEnumerable<Error> errors)
    {
        WriteErrorLine(GetVersion());
        WriteErrorLine();

        WriteErrorLine($"{nameof(YouTubeCommentsToFile)} {string.Join(' ', args.Select(arg => arg.Contains(' ') ? $@"""{arg}""" : arg))}");
        WriteErrorLine();

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

    private static string GetVersion()
    {
        return $"{nameof(YouTubeCommentsToFile)}, Version {Assembly.GetExecutingAssembly().GetName().Version.ToString(2)}";
    }

    [GeneratedRegex(@"^(?:-[A-Za-z0-9]|--[A-Za-z0-9][A-Za-z0-9-]*)$")]
    private static partial Regex RegexIsOption();

    private static string[] RearrangeArgs(string[] args)
    {
        var items = args.Select((arg, i) => (arg, i, isOption: RegexIsOption().IsMatch(arg))).ToArray();

        var indexes =
            items.Where(item => item.isOption).Join(
                items.Where(item => item.isOption == false),
                option => option.i,
                value => value.i - 1,
                (option, value) => option.i
            )
            .ToArray();

        return [.. items.Select(x =>
        {
            if (indexes.Contains(x.i))
            {
                string value = args[x.i + 1];
                if (value.Contains('"'))
                    value = value.Replace("\"", "\\\"");
                if (value.Contains(' '))
                    value = $@"""{value}""";
                return $"{x.arg} {value}";
            }
            else if (indexes.Contains(x.i - 1))
            {
                return null;
            }
            else
            {
                return x.arg;
            }
        })
        .Where(arg => arg != null)];
    }

    private enum ParsingResult
    {
        Success = 0,
        Version = 1,
        Help = 2,
        ArgumentsNotParsed = 3,
        Error = -1
    }
}
