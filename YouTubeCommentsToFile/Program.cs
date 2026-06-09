using CommandLine;
using CommandLine.Text;
using YouTubeCommentsToFile;
using YouTubeCommentsToFile.Library;

Console.OutputEncoding = Encoding.UTF8;
Console.Clear();

#if RELEASE
return ParseArgs(args);
#elif DEBUG
Debug();
return 0;
#endif

static int ParseArgs(string[] args)
{
    int returnCode = 0;

    try
    {
        AddQuotationsToYtDlpOptions(args);

        var parser = GetParser();

        var parserResult = parser.ParseArguments<ConvertOptions, DownloadOptions>(args);

        parserResult
            .WithParsed<ConvertOptions>(options => WriteCommentsToFile(options.ToSettings()))
            .WithParsed<DownloadOptions>(options =>
            {
                RemoveQuotationsFromYtDlpOptions(options);
                WriteCommentsToFile(options.ToSettings());
            })
            .WithNotParsed(errors =>
            {
                if (errors.IsVersion())
                {
                    Console.WriteLine(GetVersion());
                    Console.WriteLine();
                }
                else if (errors.IsHelp())
                {
                    Console.WriteLine(GetHelp());
                }
                else if (errors.HasAny())
                {
                    returnCode = -1;

                    WriteErrorLine(GetVersion());
                    WriteErrorLine();

                    WriteErrorLine($"{nameof(YouTubeCommentsToFile)} Errors:");
                    foreach (Error error in errors)
                    {
                        if (error is TokenError tokenError)
                            WriteErrorLine($"{tokenError.Tag} {tokenError.Token}");
                        else if (error is NamedError namedError)
                            WriteErrorLine($"{namedError.Tag} {namedError.NameInfo.NameText}");
                        else
                            WriteErrorLine(error.Tag.ToString());
                    }
                    WriteErrorLine();
                }
            });
    }
    catch (ArgumentException ex)
    {
        returnCode = -1;
        WriteErrorLine(HandledArgumentException(ReplaceOptionLongName(ex)));
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

static void AddQuotationsToYtDlpOptions(string[] args)
{
    var argYtDlpOptions = args.Select((arg, index) => (arg, index)).FirstOrDefault(x => string.Compare(x.arg, "--yt-dlp-options", StringComparison.OrdinalIgnoreCase) == 0);
    if (argYtDlpOptions == default)
        return;

    int index = argYtDlpOptions.index + 1;
    if (index >= args.Length)
        return;

    string value = args[index];
    if ((value.StartsWith('"') && value.EndsWith('"')) == false)
        args[index] = $@"""{value}""";
}

static void RemoveQuotationsFromYtDlpOptions(DownloadOptions options)
{
    if (string.IsNullOrEmpty(options.YtDlpOptions))
        return;

    if (options.YtDlpOptions.StartsWith('"') && options.YtDlpOptions.EndsWith('"'))
        options.YtDlpOptions = options.YtDlpOptions[1..^1];
}

static void WriteCommentsToFile(Settings settings)
{
    var commentsWriter = new CommentsWriter(settings);

    commentsWriter.Start += (sender, e) =>
    {
        Console.WriteLine(GetVersion());
    };

    bool isDownloadComments = false;

    commentsWriter.StartDownloadComments += (sender, e) =>
    {
        Console.WriteLine(e.URL);
        Console.WriteLine("Downloading comments...");
        Console.WriteLine("yt-dlp command line:");
        Console.WriteLine($"yt-dlp {e.YtDlpCommandLine}");
        isDownloadComments = true;
    };

    var lockObj = new object();

    commentsWriter.DownloadCommentsProgressAsync += (sender, e) =>
    {
        lock (lockObj)
        {
            if (e.IsError)
                WriteErrorLine(e.Line);
            else
                Console.WriteLine(e.Line);
        }
    };

    bool isPrintedJsonFile = false;
    bool pauseOnErrors = false;

    commentsWriter.FinishDownloadComments += (sender, e) =>
    {
        lock (lockObj)
        {
            if (e.Error == null)
            {
                Console.WriteLine("Comments JSON file downloaded successfully.");
                Console.WriteLine($"JSON file: {e.JsonFile}");
                isPrintedJsonFile = true;
            }
            else
            {
                WriteErrorLine("Failed to download comments JSON file.");
                pauseOnErrors = true;
            }
        }
    };

    commentsWriter.StartDownloadVideoInfo += (sender, e) =>
    {
        Console.Write("Downloading video information...");
    };

    bool isPrintedVideoTitle = false;

    commentsWriter.FinishDownloadVideoInfo += (sender, e) =>
    {
        if (e.Error == null)
        {
            Console.WriteLine(" Done.");
            Console.WriteLine($"Title: {e.VideoInfo.Title}");
            if (string.IsNullOrEmpty(e.VideoInfo.Uploader) == false)
                Console.WriteLine($"Uploader: {e.VideoInfo.Uploader}");
            else if (string.IsNullOrEmpty(e.VideoInfo.UploaderID) == false)
                Console.WriteLine($"Uploader: {e.VideoInfo.UploaderID}");
            isPrintedVideoTitle = true;
        }
        else
        {
            Console.WriteLine();

#if RELEASE
            WriteWarningLine("Failed to download video information.");
            WriteWarningLine($"Error: {e.Error.Message}");

            Exception ex = e.Error.InnerException;
            while (ex != null)
            {
                WriteWarningLine($"Error: {ex.Message}");
                ex = ex.InnerException;
            }
#elif DEBUG
            WriteWarningLine(e.Error.GetExceptionErrorMessage("Failed to download video information."));
#endif

            pauseOnErrors = true;
        }
    };

    commentsWriter.StartLoadJsonFile += (sender, e) =>
    {
        if (isPrintedVideoTitle = false && isDownloadComments == false)
        {
            Console.WriteLine($"Title: {e.VideoInfo.Title}");
            if (string.IsNullOrEmpty(e.VideoInfo.Uploader) == false)
                Console.WriteLine($"Uploader: {e.VideoInfo.Uploader}");
            else if (string.IsNullOrEmpty(e.VideoInfo.UploaderID) == false)
                Console.WriteLine($"Uploader: {e.VideoInfo.UploaderID}");
        }
        Console.Write("Loading JSON file...");
    };

    commentsWriter.FinishLoadJsonFile += (sender, e) =>
    {
        if (e.Error == null)
        {
            Console.WriteLine(" Done.");
            if (isPrintedJsonFile == false)
                Console.WriteLine($"JSON file: {e.JsonFile}");
        }
        else
        {
            Console.WriteLine();
            WriteErrorLine("Failed to load JSON file.");
            if (isPrintedJsonFile == false)
                WriteErrorLine($"JSON file: {e.JsonFile}");
            pauseOnErrors = true;
        }
    };

    commentsWriter.StartProcessComments += (sender, e) =>
    {
        Console.Write("Processing comments...");
    };

    commentsWriter.FinishProcessComments += (sender, e) =>
    {
        if (e.Error == null)
        {
            Console.WriteLine(" Done.");

            if (e.CommentsCount == e.TotalComments)
            {
                Console.WriteLine($"Comments: {e.CommentsCount:N0}");
            }
            else
            {
                Console.WriteLine($"Comments: {e.CommentsCount:N0}");
                Console.WriteLine($"Discarded Comments: {e.TotalComments - e.CommentsCount:N0}");
                Console.WriteLine($"Total Comments: {e.TotalComments:N0}");
            }
        }
        else
        {
            Console.WriteLine();
            WriteErrorLine("Failed to process comments.");
            pauseOnErrors = true;
        }
    };

    commentsWriter.StartWriteComments += (sender, e) =>
    {
        Console.Write($"Writing to {(e.WriteToHTML ? "HTML" : "text")} file...");
    };

    int lostComments = 0;
    var warningMessages = new List<string>();

    commentsWriter.WriteCommentError += (sender, e) =>
    {
        warningMessages.Add(e.Error.GetExceptionErrorMessage($"COMMENT:{Environment.NewLine}{e.Comment}"));
        lostComments += e.LostComments;
        pauseOnErrors = true;
    };

    commentsWriter.FinishWriteComments += (sender, e) =>
    {
        if (e.Error == null && lostComments == 0)
        {
            Console.WriteLine(" Done.");
            Console.WriteLine($"{(e.WriteToHTML ? "HTML" : "Text")} file: {e.CommentsFile}");
        }
        else
        {
            Console.WriteLine();

            if (e.Error != null)
            {
                WriteErrorLine($"Failed to write {(e.WriteToHTML ? "HTML" : "text")} file.");
                WriteErrorLine($"{(e.WriteToHTML ? "HTML" : "Text")} file: {e.CommentsFile}");
            }

            if (lostComments > 0)
            {
                WriteWarningLine($"Lost comments: {lostComments}");
                WriteWarningLine("-----------------");
                WriteWarningLine(string.Join($"{Environment.NewLine}-----------------{Environment.NewLine}", warningMessages));
            }

            pauseOnErrors = true;
        }
    };

    commentsWriter.StartDeleteJsonFile += (sender, e) =>
    {
        Console.Write("Deleting JSON file...");
    };

    commentsWriter.FinishDeleteJsonFile += (sender, e) =>
    {
        if (e.Error == null)
        {
            Console.WriteLine(" Done.");
        }
        else
        {
            Console.WriteLine();
            WriteErrorLine("Failed to delete JSON file.");
            pauseOnErrors = true;
        }
    };

    commentsWriter.Finish += (sender, e) =>
    {
        if (e.Error == null)
        {
            if (lostComments == 0)
                Console.WriteLine("Finished successfully.");
            else
                Console.WriteLine("Finished with warnings.");

            string processTime = e.ProcessTime.ToString(e.ProcessTime.Days > 0 ? "d':'hh':'mm':'ss'.'fff" : e.ProcessTime.Hours > 0 ? "h':'mm':'ss'.'fff" : "m':'ss'.'fff");
            Console.WriteLine($"Process Time: {processTime}");
        }
        else
        {
#if RELEASE
            WriteErrorLine("Failed to write comments file.");
            WriteErrorLine($"Error: {e.Error.Message}");

            Exception ex = e.Error.InnerException;
            while (ex != null)
            {
                WriteErrorLine($"Error: {ex.Message}");
                ex = ex.InnerException;
            }
#elif DEBUG
            WriteErrorLine(e.Error.GetExceptionErrorMessage("Failed to write comments file."));
#endif

            pauseOnErrors = true;
        }
    };

    commentsWriter.Tracepoint += (sender, e) =>
    {
        WriteWarningLine(e.Message);
    };

    commentsWriter.WriteCommentsToFile();

    if (pauseOnErrors)
        PressAnyKey();
}

static ArgumentException ReplaceOptionLongName(ArgumentException ex)
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
        .Insert(startIndex, $"(Option '--{optionLongName}')");

    return new ArgumentException(message);
}

#pragma warning disable CS8321 // Local function is declared but never used

#if DEBUG
static void Debug()
{
    //Help();

    //Run(new DownloadOptions
    //{
    //    URL = "",
    //    YtDlp = @"",
    //    DownloadPath = @"",
    //    ToHTML = true,
    //    SortTop = true,
    //    HighlightUploader = true,
    //    ShowCommentNavigationLinks = true,
    //    DeleteJsonFile = false
    //});

    //Run(new ConvertOptions
    //{
    //    JsonFile = @"",
    //    URL = "",
    //    YtDlp = @"",
    //    ToHTML = true,
    //    HighlightUploader = true,
    //    ShowCommentNavigationLinks = true,
    //    DeleteJsonFile = false
    //});

    //GetVideoInfo("", @"");
}

static void Help()
{
    Console.WriteLine(GetHelp());
}

static void Run<T>(T options)
{
    ParseArgs(GetParser().FormatCommandLineArgs(options));
    Console.WriteLine();
}

static void GetVideoInfo(string url, string ytDlp = null, int codePage = -1, bool updateYtDlp = false)
{
    var videoInfo = CommentsWriter.GetVideoInfo(url, ytDlp, codePage, updateYtDlp);
    if (videoInfo != null)
        Console.WriteLine(videoInfo);
    else
        WriteErrorLine("Failed to get video info.");
}
#endif

#pragma warning restore CS8321 // Local function is declared but never used

static void WriteErrorLine(string line = null)
{
    ConsoleColor foregroundColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine(line);
    Console.ForegroundColor = foregroundColor;
}

static void WriteWarningLine(string line = null)
{
    ConsoleColor foregroundColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(line);
    Console.ForegroundColor = foregroundColor;
}

static void PressAnyKey()
{
    Console.WriteLine("Press any key to continue . . .");
    Console.ReadKey(true);
}

static Parser GetParser()
{
    return new Parser(with =>
    {
        with.CaseSensitive = false;
        with.IgnoreUnknownArguments = true;
        with.HelpWriter = null;
    });
}

static string GetHelp()
{
    return GetHelpMessage(
        GetParser(),
        ["--help"]
    );
}

static string GetHelpMessage(Parser parser, string[] args)
{
    string[] optionsPrecedence = OptionsHelper.GetOptionsPrecedence();

    var sb = new StringBuilder();
    sb.AppendLine(GetVersion());
    sb.AppendLine();
    sb.AppendLine(GetHelpText<ConvertHelpOptions>(parser, args, optionsPrecedence, "Convert"));
    sb.AppendLine(GetHelpText<DownloadHelpOptions>(parser, args, optionsPrecedence, "Download"));
    sb.AppendLine(GetHelpText<SharedHelpOptions>(parser, args, optionsPrecedence, "Shared Options"));
    sb.AppendLine(GetHelpText<UploaderHelpOptions>(parser, args, optionsPrecedence, "Uploader"));
    sb.AppendLine(GetHelpText<AuthorsHelpOptions>(parser, args, optionsPrecedence, "Authors"));
    sb.AppendLine(GetHelpText<TextsHelpOptions>(parser, args, optionsPrecedence, "Texts"));
    sb.AppendLine(GetHelpText<EncodingHelpOptions>(parser, args, optionsPrecedence, "Encoding"));
    sb.AppendLine(GetHelpText<HTMLColorsHelpOptions>(parser, args, optionsPrecedence, "HTML Colors"));
    sb.AppendLine(GetHelpText<ConvertExamples>(parser, args, optionsPrecedence, "Convert Examples"));
    sb.AppendLine();
    sb.AppendLine(GetHelpText<DownloadExamples>(parser, args, optionsPrecedence, "Download Examples"));
    sb.AppendLine();
    sb.AppendLine(GetHelpText<UploaderExamples>(parser, args, optionsPrecedence, "Uploader Examples"));
    sb.AppendLine();
    sb.AppendLine(GetHelpText<AuthorsExamples>(parser, args, optionsPrecedence, "Authors Examples"));
    sb.AppendLine();
    sb.AppendLine(GetHelpText<TextsExamples>(parser, args, optionsPrecedence, "Texts Examples"));
    sb.AppendLine();
    return sb.ToString();
}

static string GetHelpText<T>(Parser parser, string[] args, string[] optionsPrecedence, string heading = null, params string[] descriptionLines)
{
    return HelpText.AutoBuild(
        parser.ParseArguments<T>(args),
        h =>
        {
            if (descriptionLines.HasAny())
                h.AddPreOptionsLines(descriptionLines);

            var optionDescriptions = typeof(T).GetInterfaces().SelectMany(it => it.GetCustomAttributes<OptionDescriptionAttribute>().Select(x => x.Description));
            if (optionDescriptions.HasAny())
                h.AddPreOptionsLines(optionDescriptions);

            h.Heading = (
                string.IsNullOrEmpty(heading) ?
                string.Empty :
                $"{heading}{Environment.NewLine}{new string('-', heading.Length)}"
            );

            h.Copyright = string.Empty;
            h.AdditionalNewLineAfterOption = false;
            h.MaximumDisplayWidth = 120;
            h.AddNewLineBetweenHelpSections = true;
            h.AddDashesToOption = true;
            h.AutoHelp = false;
            h.AutoVersion = false;
            h.OptionComparison = (attr1, attr2) =>
            {
                int OrderOptions(string longName)
                {
                    if (attr1.LongName == longName)
                        return -1;
                    if (attr2.LongName == longName)
                        return 1;
                    return 0;
                }

                foreach (var option in optionsPrecedence)
                {
                    int value;
                    if ((value = OrderOptions(option)) != 0)
                        return value;
                }

                return attr1.LongName.CompareTo(attr2.LongName);
            };

            return h;
        },
        e => e
    );
}

static string GetVersion()
{
    return $"{nameof(YouTubeCommentsToFile)}, Version {Assembly.GetExecutingAssembly().GetName().Version.ToString(2)}";
}

static string HandledArgumentException(Exception ex)
{
    var errorMessage = new StringBuilder();

    var assemblyName = Assembly.GetExecutingAssembly().GetName();
    errorMessage.AppendLine($"Error - {assemblyName.Name} {assemblyName.Version.ToString(2)}");
    errorMessage.AppendLine();
    errorMessage.AppendLine(ex.Message);

    return errorMessage.ToString();
}

static string UnhandledException(Exception ex)
{
    var errorMessage = new StringBuilder();

    var assemblyName = Assembly.GetExecutingAssembly().GetName();
    errorMessage.AppendLine($"Unhandled Error - {assemblyName.Name} {assemblyName.Version.ToString(2)}");

    try
    {
        errorMessage.AppendLine(ex.GetUnhandledExceptionErrorMessage());
    }
    catch
    {
        while (ex != null)
        {
            errorMessage.AppendLine();
            errorMessage.AppendLine($"ERROR TYPE: {ex.GetType()}");
            errorMessage.AppendLine($"ERROR: {ex.Message}");

            ex = ex.InnerException;
        }
    }

    return errorMessage.ToString();
}
