using Newtonsoft.Json;

namespace YouTubeCommentsToFile.Library;

public partial class CommentsWriter(Settings settings)
{
    #region Fields

    private readonly Settings settings = settings ?? new();

    private Encoding encoding;
    private WebsiteInfo websiteInfo;
    private VideoInfo videoInfo;
    private string jsonFile;
    private List<Comment> comments;
    private List<string> allAuthors;
    private bool hasYouTube;

#if DEBUG
    private static readonly bool HighlightAllComments = false;
    private static readonly bool FilterAndHighlightCommentsWithEmoji = false;
#endif

    #endregion

    #region Events

    public EventHandler Start;
    public EventHandler<StartDownloadCommentsEventArgs> StartDownloadComments;
    public EventHandler<DownloadCommentsProgressEventArgs> DownloadCommentsProgressAsync;
    public EventHandler<FinishDownloadCommentsEventArgs> FinishDownloadComments;
    public EventHandler<StartDownloadVideoInfoEventArgs> StartDownloadVideoInfo;
    public EventHandler<FinishDownloadVideoInfoEventArgs> FinishDownloadVideoInfo;
    public EventHandler<StartLoadJsonFileEventArgs> StartLoadJsonFile;
    public EventHandler<FinishLoadJsonFileEventArgs> FinishLoadJsonFile;
    public EventHandler<StartProcessCommentsEventArgs> StartProcessComments;
    public EventHandler<FinishProcessCommentsEventArgs> FinishProcessComments;
    public EventHandler<StartWriteCommentsEventArgs> StartWriteComments;
    public EventHandler<WriteCommentErrorEventArgs> WriteCommentError;
    public EventHandler<FinishWriteCommentsEventArgs> FinishWriteComments;
    public EventHandler<StartDeleteJsonFileEventArgs> StartDeleteJsonFile;
    public EventHandler<FinishDeleteJsonFileEventArgs> FinishDeleteJsonFile;
    public EventHandler<FinishEventArgs> Finish;
    public EventHandler<TracepointEventArgs> Tracepoint;

    #endregion

    #region HTML Colors

    private static readonly Color TextLightThemeColor = Color.FromArgb(15, 15, 15);
    private static readonly Color BackgroundLightThemeColor = Color.White;
    private static readonly Color AuthorLightThemeColor = Color.FromArgb(15, 128, 15);
    private static readonly Color SmallElemLightThemeColor = Color.FromArgb(96, 96, 96);
    private static readonly Color FavoritedBgLightThemeColor = Color.FromArgb(229, 235, 238);
    private static readonly Color HeaderBgLightThemeColor = Color.FromArgb(242, 242, 242);
    private static readonly Color HighlightLightThemeColor = Color.FromArgb(15, 15, 15);
    private static readonly Color HighlightBgLightThemeColor = Color.FromArgb(255, 228, 196);
    private static readonly Color HighlightBorderLightThemeColor = Color.FromArgb(183, 136, 0);
    private static readonly Color LinkLightThemeColor = Color.FromArgb(0, 0, 238);
    private static readonly Color CopyTextSuccessLightThemeColor = Color.FromArgb(15, 170, 15);
    private static readonly Color CopyTextFailureLightThemeColor = Color.FromArgb(255, 15, 15);

    private static readonly Color TextDarkThemeColor = Color.FromArgb(241, 241, 241);
    private static readonly Color BackgroundDarkThemeColor = Color.FromArgb(15, 15, 15);
    private static readonly Color AuthorDarkThemeColor = Color.FromArgb(15, 128, 15);
    private static readonly Color SmallElemDarkThemeColor = Color.FromArgb(170, 170, 170);
    private static readonly Color FavoritedBgDarkThemeColor = Color.FromArgb(96, 96, 96);
    private static readonly Color HeaderBgDarkThemeColor = Color.FromArgb(39, 39, 39);
    private static readonly Color HighlightDarkThemeColor = Color.FromArgb(15, 15, 15);
    private static readonly Color HighlightBgDarkThemeColor = Color.FromArgb(105, 137, 134);
    private static readonly Color HighlightBorderDarkThemeColor = Color.FromArgb(0, 114, 124);
    private static readonly Color LinkDarkThemeColor = Color.FromArgb(62, 166, 255);
    private static readonly Color CopyTextSuccessDarkThemeColor = Color.FromArgb(15, 170, 15);
    private static readonly Color CopyTextFailureDarkThemeColor = Color.FromArgb(213, 15, 15);

    private static readonly Color HeartColor = Color.FromArgb(255, 0, 51);
    private static readonly Color YouTubeColor = Color.White;
    private static readonly Color YouTubeBgColor = Color.Red;

    #endregion

    #region Indent Size, Text Line Length

    private const int INDENT_MIN_SIZE = 2;
    private const int INDENT_MAX_SIZE = 10;
    private const int INDENT_SIZE_TEXT = 4;
    private const int INDENT_SIZE_HTML = 4;

    internal const int TEXT_LINE_MIN_LENGTH = 80;
    internal const int TEXT_LINE_MAX_LENGTH = 320;
    private const int TEXT_LINE_LENGTH_TEXT = 120;
    private const int TEXT_LINE_LENGTH_HTML = 150;

    #endregion

    #region yt-dlp

    private const int TIMEOUT = 10 * 60 * 1000;

    private static Process GetYtDlpProcess(string ytDlp, string ytDlpCommandLine, Encoding encoding = null)
    {
        var process = new Process();
        process.StartInfo.FileName = ytDlp;
        process.StartInfo.Arguments = ytDlpCommandLine;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.StandardOutputEncoding = encoding ?? Encoding.Latin1;
        process.StartInfo.StandardErrorEncoding = encoding ?? Encoding.Latin1;
        return process;
    }

    [GeneratedRegex(@"^\d+[.-]\d+[.-]\d+$")]
    private static partial Regex RegexYtDlpVersion();

    private static bool IsYtDlpAccessible()
    {
        try
        {
            string version = null;
            int exitCode = -1;

            {
                using var process = GetYtDlpProcess("yt-dlp", "--version");
                process.Start();
                process.WaitForExit(TIMEOUT);

                version = process.StandardOutput.ReadToEnd().Trim();
                exitCode = process.ExitCode;
            }

            return
                exitCode == 0 &&
                string.IsNullOrEmpty(version) == false &&
                RegexYtDlpVersion().IsMatch(version);
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Write Comments To File

    public void WriteCommentsToFile()
    {
        long startTime = Stopwatch.GetTimestamp();

        FixDownloadPath();

        FixYtDlp();

        bool isYtDlpAccessible;
        if (isYtDlpAccessible = string.IsNullOrEmpty(settings.YtDlp) && IsYtDlpAccessible())
            settings.YtDlp = "yt-dlp";

        if (ValidateAndCleanURL() == false)
            throw new ArgumentException($"Not a video URL '{settings.URL}'.", nameof(settings.URL));

        if (settings.Download)
        {
            if (string.IsNullOrEmpty(settings.URL))
                throw new ArgumentException("URL not specified.", nameof(settings.URL));

            if (isYtDlpAccessible == false)
            {
                if (string.IsNullOrEmpty(settings.YtDlp))
                    throw new ArgumentException("Path to yt-dlp not specified.", nameof(settings.YtDlp));

                if (string.Compare(Path.GetFileNameWithoutExtension(settings.YtDlp), "yt-dlp", StringComparison.OrdinalIgnoreCase) != 0)
                    throw new ArgumentException($"Not yt-dlp '{settings.YtDlp}'.", nameof(settings.YtDlp));

                if (File.Exists(settings.YtDlp) == false)
                    throw new ArgumentException($"Could not find yt-dlp '{settings.YtDlp}'.", nameof(settings.YtDlp));
            }

            if (string.IsNullOrEmpty(settings.DownloadPath))
                throw new ArgumentException("Download path not specified.", nameof(settings.DownloadPath));

            if (settings.TrimTitle != null && settings.TrimTitle.Value < 1)
                throw new ArgumentOutOfRangeException(nameof(settings.TrimTitle), settings.TrimTitle.Value, "Minimum length is 1 character.");
        }
        else
        {
            if (string.IsNullOrEmpty(settings.JsonFile))
                throw new ArgumentException("JSON file not specified.", nameof(settings.JsonFile));

            if (string.Compare(Path.GetExtension(settings.JsonFile), ".json", StringComparison.OrdinalIgnoreCase) != 0)
                throw new ArgumentException($"Not a JSON file '{settings.JsonFile}'.", nameof(settings.JsonFile));

            if (File.Exists(settings.JsonFile) == false)
                throw new ArgumentException($"Could not find JSON file '{settings.JsonFile}'.", nameof(settings.JsonFile));

            if (isYtDlpAccessible == false)
            {
                if (string.IsNullOrEmpty(settings.YtDlp) == false)
                {
                    if (string.Compare(Path.GetFileNameWithoutExtension(settings.YtDlp), "yt-dlp", StringComparison.OrdinalIgnoreCase) != 0)
                        throw new ArgumentException($"Not yt-dlp '{settings.YtDlp}'.", nameof(settings.YtDlp));

                    if (File.Exists(settings.YtDlp) == false)
                        throw new ArgumentException($"Could not find yt-dlp '{settings.YtDlp}'.", nameof(settings.YtDlp));
                }
            }
        }

        if (settings.IndentSize != null)
        {
            if (settings.IndentSize.Value < INDENT_MIN_SIZE)
                throw new ArgumentOutOfRangeException(nameof(settings.IndentSize), settings.IndentSize.Value, $"Minimum indentation size is {INDENT_MIN_SIZE} characters.");

            if (settings.IndentSize.Value > INDENT_MAX_SIZE)
                throw new ArgumentOutOfRangeException(nameof(settings.IndentSize), settings.IndentSize.Value, $"Maximum indentation size is {INDENT_MAX_SIZE} characters.");
        }

        if (settings.TextLineLength != null)
        {
            if (settings.TextLineLength.Value < TEXT_LINE_MIN_LENGTH)
                throw new ArgumentOutOfRangeException(nameof(settings.TextLineLength), settings.TextLineLength.Value, $"Minimum line length is {TEXT_LINE_MIN_LENGTH} characters.");

            if (settings.TextLineLength.Value > TEXT_LINE_MAX_LENGTH)
                throw new ArgumentOutOfRangeException(nameof(settings.TextLineLength), settings.TextLineLength.Value, $"Maximum line length is {TEXT_LINE_MAX_LENGTH} characters.");
        }

        if (settings.EncodingCodePage != null)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            encoding = Encoding.GetEncoding(settings.EncodingCodePage.Value);
        }

        Start?.Raise(this, () => EventArgs.Empty);

        #region Get Video Info

        // running GetVideoInfoAsync() in the background,
        // while downloading lots of comments, is going to save running time

        CancellationTokenSource cts = null;
        CancellationToken ct = default;
        Task<VideoInfo> taskVideoInfo = null;
        Exception downloadVideoInfoError = null;

        bool isDownloadVideoInfo =
            string.IsNullOrEmpty(settings.URL) == false &&
            string.IsNullOrEmpty(settings.YtDlp) == false;

        if (isDownloadVideoInfo)
        {
            try
            {
                cts = new CancellationTokenSource();
                ct = cts.Token;
                taskVideoInfo = GetVideoInfoAsync(ct);
            }
            catch (Exception ex)
            {
                isDownloadVideoInfo = false;
                downloadVideoInfoError = ex;
            }
        }

        #endregion

        #region Download Comments

        if (settings.Download)
        {
            string ytDlpCommandLine = GetYtDlpCommandLine();

            StartDownloadComments?.Raise(this, () => new StartDownloadCommentsEventArgs(settings.URL, ytDlpCommandLine));

            try
            {
                if (Directory.Exists(settings.DownloadPath) == false)
                    Directory.CreateDirectory(settings.DownloadPath);

                if (DownloadComments(ytDlpCommandLine, out jsonFile, out string errorMessage))
                {
                    FinishDownloadComments?.Raise(this, () => new FinishDownloadCommentsEventArgs(settings.URL, ytDlpCommandLine, jsonFile));

                    if (settings.OnlyDownloadComments)
                    {
                        TimeSpan pt = Stopwatch.GetElapsedTime(startTime);
                        Finish?.Raise(this, () => new FinishEventArgs(pt));
                        return;
                    }
                }
                else
                {
                    cts?.Cancel();

                    var error = new Exception(errorMessage);
                    FinishDownloadComments?.Raise(this, () => new FinishDownloadCommentsEventArgs(settings.URL, ytDlpCommandLine, jsonFile, error));
                    Finish?.Raise(this, () => new FinishEventArgs(TimeSpan.Zero, error));
                    return;
                }
            }
            catch (Exception ex)
            {
                cts?.Cancel();

                FinishDownloadComments?.Raise(this, () => new FinishDownloadCommentsEventArgs(settings.URL, ytDlpCommandLine, jsonFile, ex));
                Finish?.Raise(this, () => new FinishEventArgs(TimeSpan.Zero, ex));
                return;
            }
        }
        else
        {
            jsonFile = settings.JsonFile;
        }

        #endregion

        #region Download Video Info

        if (isDownloadVideoInfo)
        {
            StartDownloadVideoInfo?.Raise(this, () => new StartDownloadVideoInfoEventArgs(videoInfo));

            try
            {
                if (taskVideoInfo.Wait(TIMEOUT, ct) && taskVideoInfo.IsCanceled == false)
                    videoInfo = taskVideoInfo.Result;
            }
            catch (Exception ex)
            {
                downloadVideoInfoError = ex;

                videoInfo = new VideoInfo()
                {
                    Title = Path.GetFileNameWithoutExtension(jsonFile).CleanText()
                };
            }

            FinishDownloadVideoInfo?.Raise(this, () => new FinishDownloadVideoInfoEventArgs(videoInfo, downloadVideoInfoError));
        }

        videoInfo ??= new VideoInfo()
        {
            Title = Path.GetFileNameWithoutExtension(jsonFile).CleanText()
        };

        #endregion

        #region Get Comments

        StartLoadJsonFile?.Raise(this, () => new StartLoadJsonFileEventArgs(videoInfo, jsonFile));

        try
        {
            comments = GetComments();
            FinishLoadJsonFile?.Raise(this, () => new FinishLoadJsonFileEventArgs(videoInfo, jsonFile));
        }
        catch (Exception ex)
        {
            FinishLoadJsonFile?.Raise(this, () => new FinishLoadJsonFileEventArgs(videoInfo, jsonFile, ex));
            Finish?.Raise(this, () => new FinishEventArgs(TimeSpan.Zero, ex));
            return;
        }

        #endregion

        #region Process Comments

        int commentsCount = 0;
        int totalComments = 0;

        StartProcessComments?.Raise(this, () => new StartProcessCommentsEventArgs(videoInfo));

        try
        {
            ProcessComments(out string uploaderId, out allAuthors, out hasYouTube, out commentsCount, out totalComments);

            if (string.IsNullOrEmpty(videoInfo.UploaderID) && string.IsNullOrEmpty(uploaderId) == false)
            {
                videoInfo.UploaderID = uploaderId;
                videoInfo.UploaderURL = $"{websiteInfo.ChannelURL}/{uploaderId}";
            }

            FinishProcessComments?.Raise(this, () => new FinishProcessCommentsEventArgs(videoInfo, commentsCount, totalComments));
        }
        catch (Exception ex)
        {
            FinishProcessComments?.Raise(this, () => new FinishProcessCommentsEventArgs(videoInfo, commentsCount, totalComments, ex));
            Finish?.Raise(this, () => new FinishEventArgs(TimeSpan.Zero, ex));
            return;
        }

        #endregion

        #region Write Comments

        if (settings.ToHTMLAndText || settings.ToHTML == false)
        {
            if (WriteCommentsFile(false) == false)
                return;
        }

        if (settings.ToHTMLAndText || settings.ToHTML)
        {
            if (WriteCommentsFile(true) == false)
                return;
        }

        #endregion

        #region Delete Json File

        if (settings.DeleteJsonFile && File.Exists(jsonFile))
        {
            StartDeleteJsonFile?.Raise(this, () => new StartDeleteJsonFileEventArgs(jsonFile));

            try
            {
                DeleteJsonFile();
                FinishDeleteJsonFile?.Raise(this, () => new FinishDeleteJsonFileEventArgs(jsonFile));
            }
            catch (Exception ex)
            {
                FinishDeleteJsonFile?.Raise(this, () => new FinishDeleteJsonFileEventArgs(jsonFile, ex));
            }
        }

        #endregion

        TimeSpan processTime = Stopwatch.GetElapsedTime(startTime);
        Finish?.Raise(this, () => new FinishEventArgs(processTime));
    }

    private void FixDownloadPath()
    {
        if (string.IsNullOrEmpty(settings.DownloadPath))
            settings.DownloadPath = AppContext.BaseDirectory;
        settings.DownloadPath = Path.TrimEndingDirectorySeparator(settings.DownloadPath);
    }

    private void FixYtDlp()
    {
        if (string.IsNullOrEmpty(settings.YtDlp) == false)
            return;

        settings.YtDlp =
            Directory.GetFiles(AppContext.BaseDirectory)
            .FirstOrDefault(file => string.Compare(Path.GetFileName(file), "yt-dlp.exe", StringComparison.OrdinalIgnoreCase) == 0);
    }

    private bool ValidateAndCleanURL()
    {
        if (string.IsNullOrEmpty(settings.URL))
        {
            websiteInfo = WebsiteInfo.YouTube;
            return true;
        }

        UriBuilder uriBuilder;
        try
        {
            uriBuilder = new UriBuilder(settings.URL);
        }
        catch
        {
            return false;
        }

        if (string.IsNullOrEmpty(uriBuilder.Host))
            return false;

        if (IsYouTubeURL(uriBuilder))
        {
            if (ValidateYouTubeURL(uriBuilder) == false)
                return false;

            settings.URL = CleanYouTubeURL(uriBuilder);
            websiteInfo = WebsiteInfo.YouTube;
            return true;
        }

        if (IsBiliBiliURL(uriBuilder) || IsBiliBiliGlobalURL(uriBuilder))
        {
            if (ValidateBiliBiliURL(uriBuilder) == false)
                return false;

            settings.URL = CleanBiliBiliURL(uriBuilder);
            websiteInfo = (IsBiliBiliGlobalURL(uriBuilder) ? WebsiteInfo.BiliBiliGlobal : WebsiteInfo.BiliBili);
            return true;
        }

        websiteInfo = new WebsiteInfo(uriBuilder);
        return true;
    }

    private static bool IsYouTubeURL(UriBuilder uriBuilder)
    {
        return
            uriBuilder.Host.Contains("youtube.") ||
            uriBuilder.Host.Contains(".youtube");
    }

    private static bool ValidateYouTubeURL(UriBuilder uriBuilder)
    {
        if (string.IsNullOrEmpty(uriBuilder.Query))
            return false;

        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        if (string.IsNullOrEmpty(query["v"]))
            return false;

        return true;
    }

    private static string CleanYouTubeURL(UriBuilder uriBuilder)
    {
        CleanQueryString(uriBuilder, "v");
        return uriBuilder.Uri.ToString();
    }

    private static bool IsBiliBiliURL(UriBuilder uriBuilder)
    {
        return
            uriBuilder.Host.Contains("bilibili.com.") ||
            uriBuilder.Host.Contains(".bilibili.com");
    }

    private static bool IsBiliBiliGlobalURL(UriBuilder uriBuilder)
    {
        return
            uriBuilder.Host.Contains("bilibili.tv.") ||
            uriBuilder.Host.Contains(".bilibili.tv");
    }

    private static bool ValidateBiliBiliURL(UriBuilder uriBuilder)
    {
        if (string.IsNullOrEmpty(uriBuilder.Path))
            return false;

        if ((uriBuilder.Path.Contains("/video/") || uriBuilder.Path.Contains("/bangumi/play/")) == false)
            return false;

        return true;
    }

    private static string CleanBiliBiliURL(UriBuilder uriBuilder)
    {
        CleanQueryString(uriBuilder, "p");
        return uriBuilder.Uri.ToString();
    }

    private static void CleanQueryString(UriBuilder uriBuilder, params string[] parametersToKeep)
    {
        if (parametersToKeep.IsNullOrEmpty())
            return;

        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        var parametersToRemove = query.AllKeys.Except(parametersToKeep);
        if (parametersToRemove.IsNullOrEmpty())
            return;

        foreach (var parameter in parametersToRemove)
            query.Remove(parameter);
        uriBuilder.Query = query.ToString();
    }

    #endregion

    #region Download Comments

    private const string JSON_TEMPLATE = "%(comments)#+j";
    private const string JSON_FILE = "%(uploader&{{}} - |)s%(title){0}s.json";

    private string GetYtDlpCommandLine()
    {
        string jsonFileName = string.Format(JSON_FILE,
            settings.TrimTitle != null && settings.TrimTitle.Value >= 1 ?
            $".{settings.TrimTitle.Value}" :
            string.Empty
        );

        string ytDlpCommandLine = $@"--write-comments --print-to-file ""{JSON_TEMPLATE}"" ""{jsonFileName}"" --skip-download --no-write-info-json -P ""{settings.DownloadPath}""";

        string maxComments = (settings.MaxComments != null ? settings.MaxComments.ToString() : "all");
        string maxParents = (settings.MaxParents != null ? settings.MaxParents.ToString() : "all");
        string maxReplies = (settings.MaxReplies != null ? settings.MaxReplies.ToString() : "all");
        string maxRepliesPerThread = (settings.MaxRepliesPerThread != null ? settings.MaxRepliesPerThread.ToString() : "all");
        string max_comments = $"max_comments={maxComments},{maxParents},{maxReplies},{maxRepliesPerThread}";
        if (max_comments == "max_comments=all,all,all,all")
            max_comments = null;

        string comment_sort = (settings.SortTop ? "comment_sort=top" : (settings.SortNew ? "comment_sort=new" : null));

        if (max_comments != null || comment_sort != null)
        {
            string ytDlpExtractor =
                new UriBuilder(settings.URL).Host
                .ToLowerInvariant()
                .Replace("www.", string.Empty)
                .Replace(".com", string.Empty)
                .Replace(".net", string.Empty)
                .Replace(".org", string.Empty)
                .Replace(".tv", string.Empty);

            if (max_comments != null && comment_sort != null)
                ytDlpCommandLine += $" --extractor-args {ytDlpExtractor}:{max_comments};{comment_sort}";
            else if (max_comments != null && comment_sort == null)
                ytDlpCommandLine += $" --extractor-args {ytDlpExtractor}:{max_comments}";
            else if (max_comments == null && comment_sort != null)
                ytDlpCommandLine += $" --extractor-args {ytDlpExtractor}:{comment_sort}";
        }

        if (settings.FilenameSanitization)
            ytDlpCommandLine += " --compat-options filename-sanitization";

        if (settings.RestrictFilenames)
            ytDlpCommandLine += " --restrict-filenames";

        if (settings.WindowsFilenames)
            ytDlpCommandLine += " --windows-filenames";

        if (settings.EncodingCodePage != null)
            ytDlpCommandLine += $" --encoding {settings.EncodingCodePage.Value}";

        if (settings.UpdateYtDlp)
            ytDlpCommandLine += " -U";

        if (string.IsNullOrEmpty(settings.YtDlpOptions) == false)
            ytDlpCommandLine += $" {settings.YtDlpOptions}";

        ytDlpCommandLine += $@" ""{settings.URL}""";

        return ytDlpCommandLine;
    }

    private bool DownloadComments(string ytDlpCommandLine, out string jsonFile, out string errorMessage)
    {
        var outputLines = new List<string>();
        int exitCode = -1;

        {
            using var process = GetYtDlpProcess(settings.YtDlp, ytDlpCommandLine, encoding);

            process.OutputDataReceived += (sender, e) =>
            {
                outputLines.Add(e.Data);
                DownloadCommentsProgressAsync?.RaiseAsync(this, () => new DownloadCommentsProgressEventArgs(e.Data));
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                outputLines.Add(e.Data);
                DownloadCommentsProgressAsync?.RaiseAsync(this, () => new DownloadCommentsProgressEventArgs(e.Data, true));
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit(TIMEOUT);
            exitCode = process.ExitCode;
        }

        if (exitCode == 0)
        {
            string infoLineText = $"[info] Writing '{JSON_TEMPLATE}' to: ";
            string infoLine = outputLines.LastOrDefault(line => line?.StartsWith(infoLineText) ?? false);
            if (string.IsNullOrEmpty(infoLine) == false)
            {
                string jsonFile1 = infoLine[infoLineText.Length..];
                if (string.IsNullOrEmpty(jsonFile1) == false)
                {
                    string newJsonFile = null;
                    if (VerifyJsonFile(jsonFile1, ref newJsonFile))
                    {
                        jsonFile = newJsonFile ?? jsonFile1;
                        errorMessage = null;
                        return true;
                    }
                }
            }

            string jsonFile2 = outputLines.LastOrDefault(line => (line?.Contains(settings.DownloadPath) ?? false) && (line?.Contains(".json") ?? false));
            if (string.IsNullOrEmpty(jsonFile2) == false)
            {
                int index1 = jsonFile2.IndexOf(settings.DownloadPath);
                if (index1 != -1)
                {
                    int index2 = jsonFile2.IndexOf(".json", index1 + settings.DownloadPath.Length);
                    if (index2 != -1)
                    {
                        jsonFile2 = jsonFile2.Substring(index1, index2 - index1 + 5 /* ".json".Length */);

                        string newJsonFile = null;
                        if (VerifyJsonFile(jsonFile2, ref newJsonFile))
                        {
                            jsonFile = newJsonFile ?? jsonFile2;
                            errorMessage = null;
                            return true;
                        }
                    }
                }
            }

            errorMessage = "yt-dlp failed. Failed to retrieve JSON file.";
        }
        else
        {
            /*if (exitCode == 1)
                errorMessage = "yt-dlp failed. General error.";
            else*/
            if (exitCode == 2)
                errorMessage = "yt-dlp failed. Error in user-provided options.";
            else if (exitCode == 100)
                errorMessage = "yt-dlp failed. yt-dlp must restart for update to complete.";
            else if (exitCode == 101)
                errorMessage = "yt-dlp failed. Download was cancelled.";
            else
                errorMessage = $"yt-dlp failed. Exit code {exitCode}.";
        }

        jsonFile = null;
        return false;
    }

    private bool VerifyJsonFile(string jsonFile, ref string newJsonFile)
    {
        if (File.Exists(jsonFile))
            return true;

        // some chars are not allowed in file names
        // so, the json file could be saved under a different name

        string[] files = Directory.GetFiles(settings.DownloadPath, "*.json");
        if (files.IsNullOrEmpty())
            return false;

        var fileDistances = files
            .Select(file => new
            {
                file,
                distance = ComputeLevenshteinDistance(jsonFile, file)
            })
            .Where(x => x.distance < 10)
            .OrderBy(x => x.distance);

        if (fileDistances.IsNullOrEmpty())
            return false;

        newJsonFile = fileDistances.First().file;
        return true;
    }

    // https://www.dotnetperls.com/levenshtein
    private static int ComputeLevenshteinDistance(string s, string t)
    {
        int n = (s ?? string.Empty).Length;
        int m = (t ?? string.Empty).Length;

        // Verify arguments
        if (n == 0)
            return m;

        if (m == 0)
            return n;

        int[,] d = new int[n + 1, m + 1];

        // Initialize arrays
        for (int i = 0; i <= n; d[i, 0] = i++) ;
        for (int j = 0; j <= m; d[0, j] = j++) ;

        // Begin looping
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                // Compute cost
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost
                );
            }
        }

        // Return cost
        return d[n, m];
    }

    #endregion

    #region Download Video Info

    private async Task<VideoInfo> GetVideoInfoAsync(CancellationToken ct)
    {
        try
        {
            string ytDlpCommandLine = GetVideoInfoCommandLine(settings.URL, settings.HideVideoDescription, settings.EncodingCodePage);

            ct.ThrowIfCancellationRequested();

            string outputText = null;
            int exitCode = -1;

            {
                using var process = GetYtDlpProcess(settings.YtDlp, ytDlpCommandLine, encoding);
                process.Start();
                await process.WaitForExitAsync(ct);

                outputText = process.StandardOutput.ReadToEnd();
                exitCode = process.ExitCode;
            }

            ct.ThrowIfCancellationRequested();

            var videoInfo = GetVideoInfoFromOutputText(outputText, exitCode);

            ct.ThrowIfCancellationRequested();

            return videoInfo;
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == ct)
        {
            // swallow the exception
            // and finish the task gracefully
            return null;
        }
        catch
        {
            throw;
        }
    }

#if DEBUG
    public static VideoInfo GetVideoInfo(string url, string ytDlp = null, int? encodingCodePage = null)
    {
        if (string.IsNullOrEmpty(ytDlp))
        {
            ytDlp =
                Directory.GetFiles(AppContext.BaseDirectory)
                .FirstOrDefault(file => string.Compare(Path.GetFileName(file), "yt-dlp.exe", StringComparison.OrdinalIgnoreCase) == 0);
        }

        if (string.IsNullOrEmpty(ytDlp) && IsYtDlpAccessible())
            ytDlp = "yt-dlp";

        if (string.IsNullOrEmpty(ytDlp))
            return null;

        Encoding encoding = null;
        if (encodingCodePage != null)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            encoding = Encoding.GetEncoding(encodingCodePage.Value);
        }

        string ytDlpCommandLine = GetVideoInfoCommandLine(url, false, encodingCodePage);

        string outputText = null;
        int exitCode = -1;

        {
            using var process = GetYtDlpProcess(ytDlp, ytDlpCommandLine, encoding);
            process.Start();
            process.WaitForExit(TIMEOUT);

            outputText = process.StandardOutput.ReadToEnd();
            exitCode = process.ExitCode;
        }

        return GetVideoInfoFromOutputText(outputText, exitCode);
    }
#endif

    private static string GetVideoInfoCommandLine(string url, bool hideVideoDescription, int? encodingCodePage)
    {
        var fields = typeof(VideoInfo)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
            .Select(getter => getter.GetCustomAttribute<YtDlpFieldAttribute>().Field);

        if (hideVideoDescription)
            fields = fields.Where(field => field != "description");

        return $@"--print ""{string.Join(Environment.NewLine, fields.Select(field => $@"[{field}]%({field}|)s[{field}]"))}"" --skip-download --no-write-info-json{(encodingCodePage != null ? $" --encoding {encodingCodePage.Value}" : null)} ""{url}""";
    }

    private static VideoInfo GetVideoInfoFromOutputText(string outputText, int exitCode)
    {
        if (exitCode != 0 || string.IsNullOrEmpty(outputText))
            return null;

        string GetFieldValue(string field)
        {
            int startIndex = outputText.IndexOf($"[{field}]");
            if (startIndex == -1)
                return null;

            startIndex += $"[{field}]".Length;

            int toIndex = outputText.IndexOf($"[{field}]", startIndex);
            if (toIndex == -1)
                return null;

            string value = outputText[startIndex..toIndex];

            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Contains('\n') == false)
                return value;

            return string.Join(
                Environment.NewLine,
                value.Split([Environment.NewLine, "\n"], StringSplitOptions.TrimEntries)
            );
        }

        var videoInfo = new VideoInfo();

        var properties = typeof(VideoInfo).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
        foreach (var property in properties)
        {
            var ytDlpField = property.GetCustomAttribute<YtDlpFieldAttribute>();
            string value = GetFieldValue(ytDlpField.Field);
            property.SetValue(videoInfo, value);
        }

        videoInfo.Title = videoInfo.Title?.CleanText().Replace(" l ", " | ");
        videoInfo.Uploader = videoInfo.Uploader?.Trim();
        videoInfo.Description = videoInfo.Description?.CleanText();

        return videoInfo;
    }

    #endregion

    #region Get Comments

    private List<Comment> GetComments()
    {
        using var jsonStream = new StreamReader(jsonFile);
        using var jsonReader = new JsonTextReader(jsonStream);
        return new JsonSerializer().Deserialize<List<Comment>>(jsonReader);
    }

    #endregion

    #region Process Comments

    private void ProcessComments(
        out string uploaderId,
        out List<string> allAuthors,
        out bool hasYouTube,
        out int commentsCount,
        out int totalComments)
    {
        totalComments = comments.Count;

        uploaderId = comments.FirstOrDefault(c => c.AuthorIsUploader)?.Author;
        hasYouTube = comments.HasAny(c => c.AuthorIsYouTube);

        allAuthors = [.. comments
            .Select(c => c.Author)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(a => a, StringComparer.OrdinalIgnoreCase)
        ];

#if DEBUG
        if (FilterAndHighlightCommentsWithEmoji)
            comments.RemoveAll(c => c.Text.HasEmoji() == false);
#endif

        if (settings.HideReplies)
            comments.RemoveAll(c => c.IsReply);
        else
            AddRepliesToTopLevelComments();

        // only top-level comments from this point on

        ThreadComments();

        FilterComments();

        EnumerateAndHighlightComments(out commentsCount);
    }

    private void AddRepliesToTopLevelComments()
    {
        var topLevelComments = comments.Where(c => c.IsTopLevelComment);
        var replies = comments.Select((r, i) => (r, i)).Where(x => x.r.IsReply);

        var items = topLevelComments.Join(
            replies,
            c => c.Id,
            x => x.r.Parent,
            (parent, reply) => (parent, reply)
        );

        var indexes = new List<int>();

        foreach (var (parent, reply) in items)
        {
            parent.Add(reply.r);
            indexes.Add(reply.i);
        }

        if (indexes.HasAny())
        {
            for (int i = indexes.Count - 1; i >= 0; i--)
            {
                int endIndex = indexes[i];

                int startIndex = endIndex;
                for (int j = i - 1; j >= 0; j--)
                {
                    int index = indexes[j];
                    if (startIndex - 1 == index)
                    {
                        startIndex = index;
                        i = j;
                    }
                    else
                    {
                        break;
                    }
                }

                if (startIndex < endIndex)
                    comments.RemoveRange(startIndex, endIndex - startIndex + 1);
                else
                    comments.RemoveAt(startIndex);
            }
        }

        comments.RemoveAll(c => c.IsReply);
    }

    private void ThreadComments()
    {
        if (settings.DisableThreading)
            return;

        if (comments.IsNullOrEmpty())
            return;

        foreach (var comment in comments.Where(c => c.Count > 0))
            ThreadReplies(comment);
    }

    private static void ThreadReplies(Comment comment)
    {
        var repliesToThread = new List<(Comment reply, Comment parentComment)>();

        foreach (var reply in comment)
        {
            if (reply.Text.GetRepliedAuthor(out Author repliedAuthor))
            {
                var parentComment = comment
                    .TakeWhile(r => r != reply)
                    // CompareOptions.IgnoreNonSpace: ignore diacritics (e == é)
                    .LastOrDefault(r => string.Compare(r.Author, repliedAuthor.Name, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0);

                if (parentComment != null)
                    repliesToThread.Add((reply, parentComment));
            }
        }

        foreach (var (reply, parentComment) in repliesToThread)
        {
            comment.Remove(reply);
            parentComment.Add(reply);
        }
    }

    private void FilterComments()
    {
        if (comments.IsNullOrEmpty())
            return;

        var filterItems = settings.SearchItems.Where(s => s.Filter);
        if (filterItems.IsNullOrEmpty())
            return;

        var filterItemsArr = filterItems.ToArray();
        var filteredComments = comments.SelectMany(comment => FilterComment(comment, filterItemsArr)).ToList();

        // add parent replies all the way up to the top-level comment
        foreach (var comment in filteredComments.Take(filteredComments.Count).Where(c => c.IsReply))
        {
            Comment c = comment;
            while (c.ParentComment != null)
            {
                // there will be duplicates but it doesn't matter
                // binary search with some duplicates, looking for Ids, is far less of a hit
                // than calling Contains for each found ParentComment
                /*if (filteredComments.Contains(c.ParentComment) == false)*/
                filteredComments.Add(c.ParentComment);
                c = c.ParentComment;
            }
        }

        // sort by Id
        filteredComments.Sort();

        // remove all top-level comments that them and their replies are not filtered
        comments.RemoveAll(c => filteredComments.BinarySearch(c) < 0);

        foreach (var comment in comments)
            RemoveUnfilteredReplies(comment, filteredComments);
    }

    private static IEnumerable<Comment> FilterComment(Comment comment, SearchItem[] filterItems)
    {
        if (filterItems.Any(s => s.IsMatch(comment)))
            yield return comment;

        if (comment.Count == 0)
            yield break;

        foreach (var reply in comment)
        {
            foreach (var filteredReply in FilterComment(reply, filterItems))
                yield return filteredReply;
        }
    }

    private static void RemoveUnfilteredReplies(Comment comment, List<Comment> filteredComments)
    {
        // remove all replies that them and their replies are not filtered
        comment.RemoveAll(c => filteredComments.BinarySearch(c) < 0);

        if (comment.Count == 0)
            return;

        foreach (var reply in comment)
            RemoveUnfilteredReplies(reply, filteredComments);
    }

    private void EnumerateAndHighlightComments(out int commentsCount)
    {
        commentsCount = 0;

        if (comments.IsNullOrEmpty())
            return;

        int commentNumber = 1;
        foreach (var comment in comments)
        {
            int replyNumber = 0;
            EnumerateAndHighlightComment(comment, commentNumber++, ref replyNumber, ref commentsCount);
        }
    }

    private void EnumerateAndHighlightComment(Comment comment, int commentNumber, ref int replyNumber, ref int commentsCount)
    {
        comment.CommentNumber = commentNumber;
        comment.ReplyNumber = replyNumber++;
        commentsCount++;

        comment.IsHighlight =
            settings.SearchItems.Where(s => s.Highlight)
            .HasAny(s => s.IsMatch(comment));

#if DEBUG
        if (HighlightAllComments)
            comment.IsHighlight = true;
        else if (FilterAndHighlightCommentsWithEmoji)
            comment.IsHighlight = comment.Text.HasEmoji();
#endif

        if (comment.Count == 0)
            return;

        foreach (var reply in comment)
            EnumerateAndHighlightComment(reply, commentNumber, ref replyNumber, ref commentsCount);
    }

    #endregion

    #region Write Comments

    private bool WriteCommentsFile(bool writeToHTML)
    {
        string commentsFile = Path.Combine(
            Path.GetDirectoryName(jsonFile),
            Path.GetFileNameWithoutExtension(jsonFile) + (writeToHTML ? ".html" : ".txt")
        );

        StartWriteComments?.Raise(this, () => new StartWriteCommentsEventArgs(videoInfo, commentsFile, writeToHTML));

        try
        {
            WriteComments(commentsFile, writeToHTML);
            FinishWriteComments?.Raise(this, () => new FinishWriteCommentsEventArgs(videoInfo, commentsFile, writeToHTML));
            return true;
        }
        catch (Exception ex)
        {
            FinishWriteComments?.Raise(this, () => new FinishWriteCommentsEventArgs(videoInfo, commentsFile, writeToHTML, ex));
            Finish?.Raise(this, () => new FinishEventArgs(TimeSpan.Zero, ex));
            return false;
        }
    }

    private void WriteComments(string commentsFile, bool writeToHTML)
    {
        int indentSize = GetIndentSize(writeToHTML);
        int textLineLength = GetTextLineLength(writeToHTML);

        // calculate the textLineLength at X replies deep
        // pass that point, textLineLength is too small and the text is spread vertically
        int repliesDeep = 8;
        int textLineLengthRepliesDeep = TEXT_LINE_MIN_LENGTH - (repliesDeep * indentSize);
        if (textLineLengthRepliesDeep < TEXT_LINE_MIN_LENGTH / 2)
            textLineLengthRepliesDeep = TEXT_LINE_MIN_LENGTH / 2;

        var commentSeparator = (settings.HideCommentSeparators ? null :
            (writeToHTML ? "<hr />" : new string('─', textLineLength)));

        using var srtStream = File.Open(commentsFile, FileMode.Create);
        using var writer = new StreamWriter(srtStream, Encoding.UTF8);

        if (writeToHTML)
        {
            string charset = "utf-8";
            if (encoding != null)
                try { charset = encoding.WebName; } catch { }

            Color textColor = (settings.DarkTheme ? TextDarkThemeColor : TextLightThemeColor);
            Color backgroundColor = (settings.DarkTheme ? BackgroundDarkThemeColor : BackgroundLightThemeColor);
            Color authorColor = (settings.DarkTheme ? AuthorDarkThemeColor : AuthorLightThemeColor);
            Color smallElemColor = (settings.DarkTheme ? SmallElemDarkThemeColor : SmallElemLightThemeColor);
            Color favoritedBgColor = (settings.DarkTheme ? FavoritedBgDarkThemeColor : FavoritedBgLightThemeColor);
            Color headerBgColor = (settings.DarkTheme ? HeaderBgDarkThemeColor : HeaderBgLightThemeColor);
            Color highlightColor = (settings.DarkTheme ? HighlightDarkThemeColor : HighlightLightThemeColor);
            Color highlightBgColor = (settings.DarkTheme ? HighlightBgDarkThemeColor : HighlightBgLightThemeColor);
            Color highlightBorderColor = (settings.DarkTheme ? HighlightBorderDarkThemeColor : HighlightBorderLightThemeColor);
            Color linkColor = (settings.DarkTheme ? LinkDarkThemeColor : LinkLightThemeColor);
            Color copyTextSuccess = (settings.DarkTheme ? CopyTextSuccessDarkThemeColor : CopyTextSuccessLightThemeColor);
            Color copyTextFailure = (settings.DarkTheme ? CopyTextFailureDarkThemeColor : CopyTextFailureLightThemeColor);

            var s4 = new string(' ', 4);
            var s8 = new string(' ', 8);

            writer.WriteLine("<!DOCTYPE html>");
            writer.WriteLine("<html lang='en' xmlns='http://www.w3.org/1999/xhtml'>");
            writer.WriteLine("<head>");
            writer.WriteLine($"{s4}<meta charset='{charset}' />");
            writer.WriteLine($"{s4}<title>{HttpUtility.HtmlEncode(videoInfo.Title)}</title>");
            writer.WriteLine($"{s4}<style>");
            writer.WriteLine($"{s8}* {{ font-family: Consolas, monospace; }}");
            writer.WriteLine($"{s8}html, body {{ color: rgb({textColor.ToRgb()}); background-color: rgb({backgroundColor.ToRgb()}); line-height: 1; }}");
            writer.WriteLine($"{s8}pre * {{ display: inline-block; }}");
            writer.WriteLine($"{s8}a:link {{ color: rgb({linkColor.ToRgb()}); }}");
            if (settings.HideHeader == false)
                writer.WriteLine($"{s8}.header {{ background-color: rgb({headerBgColor.ToRgb()}); min-width: {textLineLength}ch; max-width: {textLineLength}ch; }}");
            writer.WriteLine($"{s8}.comment {{ }}");
            writer.WriteLine($"{s8}.reply {{ }}");
            if (hasYouTube)
                writer.WriteLine($"{s8}.youtube {{ color: rgb({YouTubeColor.ToRgb()}); background-color: rgb({YouTubeBgColor.ToRgb()}); font-weight: bold; padding: 0px 6px 0px 6px; }}");
            writer.WriteLine($"{s8}.uploader {{ color: rgb({backgroundColor.ToRgb()}); background-color: rgb({textColor.ToRgb()}); font-weight: bold; padding: 0px 6px 0px 6px; }}");
            writer.WriteLine($"{s8}.author {{ color: rgb({textColor.ToRgb()}); font-weight: bold; }}");
            writer.WriteLine($"{s8}.replied-author {{ color: rgb({authorColor.ToRgb()}); font-weight: bold; }}");
            writer.WriteLine($"{s8}.other-author {{ color: rgb({authorColor.ToRgb()}); }}");
            writer.WriteLine($"{s8}.pinned, .time, .likes, .comment-link-text {{ color: rgb({smallElemColor.ToRgb()}); font-size: smaller; }}");
            writer.WriteLine($"{s8}.favorited {{ color: rgb({textColor.ToRgb()}); background-color: rgb({favoritedBgColor.ToRgb()}); padding: 3px 6px 3px 6px; border-radius: 15px; }}");
            writer.WriteLine($"{s8}.heart {{ color: rgb({HeartColor.ToRgb()}); }}");

            var highlightWidthCh =
                comments.SelectMany(comment => GetHighlightWidthCh(comment, indentSize, textLineLength, indentCount: 0))
                .Distinct()
                .OrderBy(widthCh => widthCh);

            if (highlightWidthCh.HasAny())
            {
                writer.WriteLine($"{s8}.highlight {{ color: rgb({highlightColor.ToRgb()}); background-color: rgb({highlightBgColor.ToRgb()}); }}");
                writer.WriteLine($"{s8}.highlight-border-top, .highlight-border-middle, .highlight-border-bottom {{ border: solid 1px rgb({highlightBorderColor.ToRgb()}); padding-left: 1px; padding-right: 1px; }}");
                writer.WriteLine($"{s8}.highlight-border-top {{ border-bottom: none; padding-top: 1px; }}");
                writer.WriteLine($"{s8}.highlight-border-middle {{ border-top: none; border-bottom: none; }}");
                writer.WriteLine($"{s8}.highlight-border-bottom {{ border-top: none; margin-bottom: 1px; }}");

                foreach (var widthCh in highlightWidthCh)
                    writer.WriteLine($"{s8}.highlight-{widthCh} {{ min-width: {widthCh}ch; max-width: {widthCh}ch; }}");
            }

            if (settings.ShowCopyLinks)
            {
                writer.WriteLine($"{s8}.copy-text {{ color: rgb({linkColor.ToRgb()}); font-size: smaller; cursor: pointer; }}");
                writer.WriteLine($"{s8}.copy-text-success {{ color: rgb({copyTextSuccess.ToRgb()}); }}");
                writer.WriteLine($"{s8}.copy-text-failure {{ color: rgb({copyTextFailure.ToRgb()}); }}");
                writer.WriteLine($"{s8}.copy-text-success, .copy-text-failure {{ font-weight: bold; pointer-events: none; cursor: default; }}");
            }

            writer.WriteLine($"{s8}.comment-link {{ font-size: smaller; }}");
            writer.WriteLine($"{s8}.nav-comment {{ }}");
            writer.WriteLine($"{s8}.link {{ text-decoration: none; }}");
            writer.WriteLine($"{s8}.link:link, .link:visited {{ color: rgb({linkColor.ToRgb()}); }}");
            writer.WriteLine($"{s8}.disabled-link {{ color: rgb({linkColor.ToRgb()}); pointer-events: none; cursor: default; opacity: 0.5; }}");
            writer.WriteLine($"{s4}</style>");
            writer.WriteLine("</head>");
            writer.WriteLine("<body>");
            writer.WriteLine();
        }

        if (settings.HideHeader == false)
        {
            if (writeToHTML)
            {
                writer.WriteLine("<pre class='header'>");

                if (string.IsNullOrEmpty(settings.URL))
                    writer.WriteLine(HttpUtility.HtmlEncode(videoInfo.Title));
                else
                    writer.WriteLine($"<a href='{HttpUtility.HtmlAttributeEncode(settings.URL)}'>{HttpUtility.HtmlEncode(videoInfo.Title)}</a>");

                if (string.IsNullOrEmpty(videoInfo.Uploader) == false)
                {
                    if (string.IsNullOrEmpty(videoInfo.UploaderURL))
                        writer.WriteLine(HttpUtility.HtmlEncode(videoInfo.Uploader));
                    else
                        writer.WriteLine($"<a href='{HttpUtility.HtmlAttributeEncode(videoInfo.UploaderURL)}'>{HttpUtility.HtmlEncode(videoInfo.Uploader)}</a>");
                }

                if (string.IsNullOrEmpty(videoInfo.UploaderID) == false)
                {
                    if (string.IsNullOrEmpty(videoInfo.UploaderURL))
                        writer.WriteLine(HttpUtility.HtmlEncode(videoInfo.UploaderID));
                    else
                        writer.WriteLine($"<a href='{HttpUtility.HtmlAttributeEncode(videoInfo.UploaderURL)}'>{HttpUtility.HtmlEncode(videoInfo.UploaderID)}</a>");
                }
            }
            else
            {
                writer.WriteLine(videoInfo.Title);

                if (string.IsNullOrEmpty(videoInfo.Uploader) == false)
                    writer.WriteLine(videoInfo.Uploader);

                if (string.IsNullOrEmpty(videoInfo.UploaderID) == false)
                    writer.WriteLine(videoInfo.UploaderID);

                if (string.IsNullOrEmpty(settings.URL) == false)
                    writer.WriteLine(settings.URL);
            }

            if (string.IsNullOrEmpty(videoInfo.Description) == false)
            {
                writer.WriteLine();
                foreach (var line in GetDescriptionLines(textLineLength, writeToHTML))
                    writer.WriteLine(line);
            }

            if (writeToHTML)
                writer.WriteLine("</pre>");
        }

        if (comments.HasAny())
        {
            WriteTopLevelComment(
                comments.First(),
                writer,
                isFirstComment: true,
                isLastComment: (comments.Count == 1),
                commentSeparator,
                indentSize,
                textLineLength,
                textLineLengthRepliesDeep,
                writeToHTML
            );

            foreach (var comment in comments.Take(comments.Count - 1).Skip(1))
            {
                WriteTopLevelComment(
                    comment,
                    writer,
                    isFirstComment: false,
                    isLastComment: false,
                    commentSeparator,
                    indentSize,
                    textLineLength,
                    textLineLengthRepliesDeep,
                    writeToHTML
                );
            }

            if (comments.Count > 1)
            {
                WriteTopLevelComment(
                    comments.Last(),
                    writer,
                    isFirstComment: false,
                    isLastComment: true,
                    commentSeparator,
                    indentSize,
                    textLineLength,
                    textLineLengthRepliesDeep,
                    writeToHTML
                );
            }
        }

        if (writeToHTML)
        {
            if (settings.ShowCopyLinks)
            {
                writer.WriteLine();
                writer.WriteLine("<script>");
                {
                    Type type = typeof(CommentsWriter);
                    var stream = type.Assembly.GetManifestResourceStream($"{type.Namespace}.CopyCommentText.js");
                    var reader = new StreamReader(stream);
                    writer.WriteLine(reader.ReadToEnd());
                }
                writer.WriteLine("</script>");
            }

            writer.WriteLine();
            writer.WriteLine("</body>");
            writer.WriteLine("</html>");
        }
    }

    private int GetIndentSize(bool writeToHTML)
    {
        return
            (settings.IndentSize != null && INDENT_MIN_SIZE <= settings.IndentSize.Value && settings.IndentSize.Value <= INDENT_MAX_SIZE ? settings.IndentSize.Value :
            (writeToHTML ? INDENT_SIZE_HTML : INDENT_SIZE_TEXT));
    }

    private int GetTextLineLength(bool writeToHTML)
    {
        return
            (settings.TextLineLength != null && TEXT_LINE_MIN_LENGTH <= settings.TextLineLength.Value && settings.TextLineLength.Value <= TEXT_LINE_MAX_LENGTH ? settings.TextLineLength.Value :
            (writeToHTML ? TEXT_LINE_LENGTH_HTML : TEXT_LINE_LENGTH_TEXT));
    }

    private static IEnumerable<int> GetHighlightWidthCh(Comment comment, int indentSize, int textLineLength, int indentCount)
    {
        if (comment.IsHighlight)
            yield return textLineLength - indentCount;

        if (comment.Count == 0)
            yield break;

        indentCount += indentSize;

        foreach (var reply in comment)
        {
            foreach (var widthCh in GetHighlightWidthCh(reply, indentSize, textLineLength, indentCount))
                yield return widthCh;
        }
    }

    private IEnumerable<string> GetDescriptionLines(int textLineLength, bool writeToHTML)
    {
        string[] lines =
            (writeToHTML ? videoInfo.Description : videoInfo.Description.CleanTextForTextFile())
            .Split([Environment.NewLine, "\n"], StringSplitOptions.None);

        for (int i = 0; i < lines.Length; i++)
        {
            string line = (lines[i] ?? string.Empty).Trim();

            int lineLength = (writeToHTML ? line.Length : line.GetTextLengthForTextFile());

            if (lineLength <= textLineLength)
            {
                if (writeToHTML)
                    yield return line.ToHtml(settings.URL, websiteInfo.HashtagURL, websiteInfo.LowerHashtag, checkForRepliedAuthor: false, allAuthors);
                else
                    yield return line;
            }
            else
            {
                if (writeToHTML)
                {
                    string[] innerLines = line.SplitToHtmlLines(textLineLength, settings.URL, websiteInfo.HashtagURL, websiteInfo.LowerHashtag, checkForRepliedAuthor: false, allAuthors);
                    for (int k = 0; k < innerLines.Length; k++)
                        yield return (innerLines[k] ?? string.Empty).Trim();
                }
                else
                {
                    string[] innerLines = line.SplitToLines(textLineLength);
                    for (int k = 0; k < innerLines.Length; k++)
                        yield return (innerLines[k] ?? string.Empty).Trim();
                }
            }
        }
    }

    private void WriteTopLevelComment(
        Comment comment,
        StreamWriter writer,
        bool isFirstComment,
        bool isLastComment,
        string commentSeparator,
        int indentSize,
        int textLineLength,
        int textLineLengthRepliesDeep,
        bool writeToHTML)
    {
        if ((settings.HideHeader && isFirstComment) == false)
        {
            if (settings.HideCommentSeparators == false)
            {
                writer.WriteLine();
                writer.WriteLine(commentSeparator);
            }

            writer.WriteLine();
        }

        if (writeToHTML)
        {
            writer.WriteLine($"<pre id='c{comment.CommentNumber}'>");

            if (settings.ShowCommentNavigationLinks)
            {
                if ((isFirstComment && isLastComment) == false)
                {
                    string prev = (isFirstComment ?
                        $"<a class='link disabled-link'><span class='nav-comment'>{HttpUtility.HtmlEncode("↑ Prev")}</span></a>" :
                        $"<a class='link' href='{HttpUtility.HtmlAttributeEncode($"#c{comment.CommentNumber - 1}")}' title='{HttpUtility.HtmlAttributeEncode($"Previous Comment #{comment.CommentNumber - 1}")}'><span class='nav-comment'>{HttpUtility.HtmlEncode("↑ Prev")}</span></a>"
                    );

                    string next = (isLastComment ?
                        $"<a class='link disabled-link'><span class='nav-comment'>{HttpUtility.HtmlEncode("↓ Next")}</span></a>" :
                        $"<a class='link' href='{HttpUtility.HtmlAttributeEncode($"#c{comment.CommentNumber + 1}")}' title='{HttpUtility.HtmlAttributeEncode($"Next Comment #{comment.CommentNumber + 1}")}'><span class='nav-comment'>{HttpUtility.HtmlEncode("↓ Next")}</span></a>"
                    );

                    writer.WriteLine($"{next}  {prev}");
                }
            }
        }

        WriteCommentAndReplies(
            comment,
            writer,
            indentCount: 0,
            indentSize,
            textLineLength,
            textLineLengthRepliesDeep,
            isReply: false,
            isLastReply: [],
            writeToHTML
        );

        if (writeToHTML)
            writer.WriteLine("</pre>");
    }

    private void WriteCommentAndReplies(
        Comment comment,
        StreamWriter writer,
        int indentCount,
        int indentSize,
        int textLineLength,
        int textLineLengthRepliesDeep,
        bool isReply,
        List<bool> isLastReply,
        bool writeToHTML)
    {
        try
        {
            WriteComment(
                comment,
                writer,
                indentCount,
                indentSize,
                textLineLength,
                textLineLengthRepliesDeep,
                isReply,
                isLastReply,
                writeToHTML
            );
        }
        catch (Exception ex)
        {
            int lostComments = 0;
            CountLostComments(comment, ref lostComments);
            WriteCommentError?.Raise(this, () => new WriteCommentErrorEventArgs(comment.ToError(), ex, lostComments));
            return;
        }

        if (comment.Count == 0)
            return;

        indentCount += indentSize;

        var indent = $"{new string(' ', indentCount - indentSize)}│";
        indent = AddPipesToIndent(indent, indentSize, isLastReply);

        foreach (var reply in comment.Take(comment.Count - 1))
        {
            isLastReply.Add(false);
            writer.WriteLine(indent);
            WriteCommentAndReplies(
                reply,
                writer,
                indentCount,
                indentSize,
                textLineLength,
                textLineLengthRepliesDeep,
                isReply: true,
                isLastReply,
                writeToHTML
            );
            isLastReply.RemoveAt(isLastReply.Count - 1);
        }

        isLastReply.Add(true);
        var lastReply = comment.Last();
        writer.WriteLine(indent);
        WriteCommentAndReplies(
            lastReply,
            writer,
            indentCount,
            indentSize,
            textLineLength,
            textLineLengthRepliesDeep,
            isReply: true,
            isLastReply,
            writeToHTML
        );
        isLastReply.RemoveAt(isLastReply.Count - 1);
    }

    private void WriteComment(
        Comment comment,
        StreamWriter writer,
        int indentCount,
        int indentSize,
        int textLineLength,
        int textLineLengthRepliesDeep,
        bool isReply,
        List<bool> isLastReply,
        bool writeToHTML)
    {
        if (isReply)
        {
            string indentFirstLine = $"{new string(' ', indentCount - indentSize)}{(isLastReply.Last() ? '└' : '├')}{new string('─', indentSize - 2)} ";
            string indentSecondLine = $"{new string(' ', indentCount - indentSize)}{(isLastReply.Last() ? ' ' : '│')}{new string(' ', indentSize - 1)}";
            string indentRestOfLines = indentSecondLine;

            if (comment.IsHighlight)
            {
                if (writeToHTML)
                {
                    indentFirstLine = $"{indentFirstLine[..^1]}─";
                }
                else
                {
                    indentFirstLine = $"{new string(' ', indentCount - indentSize)}│{new string(' ', indentSize - 1)}";
                    indentSecondLine = $"{new string(' ', indentCount - indentSize)}{(isLastReply.Last() ? '└' : '├')}{new string('─', indentSize - 1)}";
                }
            }

            indentFirstLine = AddPipesToIndent(indentFirstLine, indentSize, isLastReply);
            indentSecondLine = AddPipesToIndent(indentSecondLine, indentSize, isLastReply);
            indentRestOfLines = AddPipesToIndent(indentRestOfLines, indentSize, isLastReply);

            var lines = comment.ToLines(settings, websiteInfo, videoInfo.UploaderID, indentCount, textLineLength, textLineLengthRepliesDeep, allAuthors, writeToHTML);

            writer.Write(indentFirstLine);
            writer.WriteLine(lines.First());

            writer.Write(indentSecondLine);
            writer.WriteLine(lines.Skip(1).First());

            foreach (var line in lines.Skip(2))
            {
                writer.Write(indentRestOfLines);
                writer.WriteLine(line);
            }
        }
        else
        {
            foreach (var line in comment.ToLines(settings, websiteInfo, videoInfo.UploaderID, indentCount: 0, textLineLength, textLineLengthRepliesDeep, allAuthors, writeToHTML))
                writer.WriteLine(line);
        }
    }

    private static void CountLostComments(Comment comment, ref int lostComments)
    {
        lostComments += 1;
        if (comment.Count == 0)
            return;
        foreach (var reply in comment)
            CountLostComments(reply, ref lostComments);
    }

    private static readonly Dictionary<(string indent, int indentSize, bool isLastReply0, bool isLastReply1, bool isLastReply2, bool isLastReply3, bool isLastReply4), string> pipedIndents = [];

    private static string AddPipesToIndent(string indent, int indentSize, List<bool> isLastReply)
    {
        if (string.IsNullOrEmpty(indent) || isLastReply.IsNullOrEmpty())
            return indent;

        if (isLastReply.Count > 5)
            return GetPipedIndent(indent, indentSize, isLastReply);

        var key = (indent, indentSize,
            (isLastReply.Count > 0 && isLastReply[0]),
            (isLastReply.Count > 1 && isLastReply[1]),
            (isLastReply.Count > 2 && isLastReply[2]),
            (isLastReply.Count > 3 && isLastReply[3]),
            (isLastReply.Count > 4 && isLastReply[4]));

        if (pipedIndents.TryGetValue(key, out string pipedIndent))
            return pipedIndent;

        pipedIndent = GetPipedIndent(indent, indentSize, isLastReply);
        pipedIndents.Add(key, pipedIndent);

        return pipedIndent;
    }

    private static string GetPipedIndent(string indent, int indentSize, List<bool> isLastReply)
    {
        var chars = indent.ToCharArray();

        var zip =
            Enumerable.Range(0, indent.Length / indentSize)
            .Select(n => n * indentSize)
            .Zip(isLastReply);

        foreach (var (pos, isLast) in zip)
        {
            if (isLast == false &&
                pos < chars.Length &&
                chars[pos] == ' ')
                chars[pos] = '│';
        }

        return new(chars);
    }

    #endregion

    #region Delete JSON File

    private void DeleteJsonFile()
    {
#if WINDOWS_BUILD
        try
        {
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                jsonFile,
                Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin,
                Microsoft.VisualBasic.FileIO.UICancelOption.ThrowException
            );

            return;
        }
        catch
        {
        }
#endif

        File.Delete(jsonFile);
    }

    #endregion
}
