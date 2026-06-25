using CommandLine;
using YouTubeCommentsToFile.Library;

namespace YouTubeCommentsToFile;

internal partial class Program
{
    private static int Run(string[] args)
    {
        int returnCode = 0;

        try
        {
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

                parsingResult = MergeConfigLocations(convertOptions, parser);
                if (parsingResult == ParsingResult.Success)
                    settings = convertOptions.ToSettings();
            }
            else if (downloadOptions != null)
            {
                string homeConfigurationDirectory = null;
                if (string.IsNullOrWhiteSpace(downloadOptions.DownloadPath) == false)
                    homeConfigurationDirectory = Path.TrimEndingDirectorySeparator(downloadOptions.DownloadPath);

                AddConfigurationStackToConfigLocations(downloadOptions, homeConfigurationDirectory);

                parsingResult = MergeConfigLocations(downloadOptions, parser);
                if (parsingResult == ParsingResult.Success)
                    settings = downloadOptions.ToSettings();
            }

            if (parsingResult == ParsingResult.Success)
                new CommentsHandler(settings).WriteCommentsToFile();
            else if (parsingResult == ParsingResult.Error)
                returnCode = -1;
        }
        catch (ArgumentException ex)
        {
            returnCode = -1;
            WriteErrorLine(HandledArgumentException(ex));
            PressAnyKey();
        }
        catch (Exception ex)
        {
            returnCode = -1;
            WriteErrorLine(UnhandledException(ex));
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

    [GeneratedRegex(@"([a-z0-9])([A-Z])")]
    private static partial Regex RegexCamelToWords();

    private static void WriteParsingErrors(string[] args, IEnumerable<Error> errors)
    {
        WriteErrorLine(GetVersion());
        WriteErrorLine();

        WriteErrorLine($"{nameof(YouTubeCommentsToFile)} {string.Join(' ', args.Select(arg => arg.Contains(' ') ? $@"""{arg}""" : arg))}");
        WriteErrorLine();

        WriteErrorLine($"{nameof(YouTubeCommentsToFile)} Errors:");
        foreach (Error error in errors)
        {
            string tag = RegexCamelToWords().Replace(error.Tag.ToString(), "$1 $2");

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

    private enum ParsingResult
    {
        Success = 0,
        Version = 1,
        Help = 2,
        ArgumentsNotParsed = 3,
        Error = -1
    }
}
