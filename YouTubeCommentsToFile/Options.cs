using CommandLine;
using CommandLine.Text;
using YouTubeCommentsToFile.Library;

namespace YouTubeCommentsToFile;

#region Interfaces

internal interface IYouTubeCommentsToFileOptions { }

[OptionDescription("Converts comments JSON file to text file or HTML file.")]
[OptionDescription($"Uses verb 'convert'. Verb is required to specify.")]
[OptionPrecedence(1)]
internal interface IConvertOptions : IYouTubeCommentsToFileOptions
{
    [Option("json-file", HelpText = "Path to comments JSON file.")]
    [OptionPrecedence(1)]
    string JsonFile { get; set; }

    [Option("url", HelpText = "Video URL. Since the video URL can't be extracted from the comments JSON file, by providing it, it makes the output file more robust by writing links to the video, to its comments and replies. URL is not required in order to write to output file.")]
    [OptionPrecedence(2)]
    string URL { get; set; }

    [Option("yt-dlp", HelpText = $"Path to yt-dlp. If not set, search for yt-dlp in {nameof(YouTubeCommentsToFile)} directory. If not found, check if yt-dlp is accessible in the current environment. yt-dlp is used to retrieve information about the video, therefore is not required to write to output file.")]
    [OptionPrecedence(3)]
    string YtDlp { get; set; }

    [Option("deno", HelpText = $"Path to Deno. If not set, search for Deno in {nameof(YouTubeCommentsToFile)} directory. If not found, check if Deno is accessible in the current environment. If Deno is at the same location as yt-dlp, there is no need to specify a path to Deno.")]
    [OptionPrecedence(4)]
    string Deno { get; set; }
}

[OptionDescription("Downloads comments to text file or HTML file.")]
[OptionDescription($"Uses verb 'download'. Verb is optional to specify.")]
[OptionPrecedence(2)]
internal interface IDownloadOptions : IYouTubeCommentsToFileOptions
{
    [Option("url", HelpText = "Video URL to download its comments.")]
    [OptionPrecedence(1)]
    string URL { get; set; }

    [Option("yt-dlp", HelpText = $"Path to yt-dlp. If not set, search for yt-dlp in {nameof(YouTubeCommentsToFile)} directory. If not found, check if yt-dlp is accessible in the current environment.")]
    [OptionPrecedence(2)]
    string YtDlp { get; set; }

    [Option("deno", HelpText = $"Path to Deno. If not set, search for Deno in {nameof(YouTubeCommentsToFile)} directory. If not found, check if Deno is accessible in the current environment. If Deno is at the same location as yt-dlp, there is no need to specify a path to Deno.")]
    [OptionPrecedence(3)]
    string Deno { get; set; }

    [Option("download-path", HelpText = $"Path to download and to convert comments. If not set, download path is set to {nameof(YouTubeCommentsToFile)} directory.")]
    [OptionPrecedence(4)]
    string DownloadPath { get; set; }

    [Option("max-comments", HelpText = "Maximum number of top-level comments and replies to extract. If not set, extracts all.")]
    [OptionPrecedence(5)]
    uint? MaxComments { get; set; }

    [Option("max-parents", HelpText = "Maximum number of top-level comments to extract. If not set, extracts all.")]
    [OptionPrecedence(6)]
    uint? MaxParents { get; set; }

    [Option("max-replies", HelpText = "Maximum number of replies to extract. If not set, extracts all.")]
    [OptionPrecedence(7)]
    uint? MaxReplies { get; set; }

    [Option("max-replies-per-thread", HelpText = "Maximum number of replies per top-level comment to extract. If not set, extracts all.")]
    [OptionPrecedence(8)]
    uint? MaxRepliesPerThread { get; set; }

    [Option("sort-new", HelpText = "Sort comments by new comments first. This is the default sorting of yt-dlp.")]
    [OptionPrecedence(9)]
    bool SortNew { get; set; }

    [Option("sort-top", HelpText = "Sort comments by top comments first.")]
    [OptionPrecedence(10)]
    bool SortTop { get; set; }

    [Option("filename-sanitization", HelpText = "Sanitize Windows reserved characters by removing them or by replacing with comparable character or by replacing with underscore. Doesn't replace with Unicode characters.")]
    bool FilenameSanitization { get; set; }

    [Option("only-download-comments", HelpText = "Whether to download comments JSON file and stop. Don't write text and HTML files.")]
    bool OnlyDownloadComments { get; set; }

    [Option("restrict-filenames", HelpText = "Restrict filenames to only ASCII characters, and avoid '&' and spaces in filenames.")]
    bool RestrictFilenames { get; set; }

    [Option("trim-title", HelpText = "Limit the length of the video title in the filename to the specified number of characters. Doesn't limit the length of the video uploader.")]
    uint? TrimTitle { get; set; }

    [Option("update-yt-dlp", HelpText = "Whether to update yt-dlp to the latest version. Updates to 'nightly' release channel.")]
    bool UpdateYtDlp { get; set; }

    [Option("windows-filenames", HelpText = "Force filenames to be Windows-compatible by replacing Windows reserved characters with lookalike Unicode characters.")]
    bool WindowsFilenames { get; set; }

    [Option("yt-dlp-options", HelpText = "Options added to yt-dlp command line.")]
    string YtDlpOptions { get; set; }
}

[OptionDescription("Shared options of Convert and Download.")]
[OptionPrecedence(3)]
internal interface ISharedOptions : IYouTubeCommentsToFileOptions
{
    [Option("to-html", HelpText = "Whether to write comments to HTML file. When both '--to-html' and '--to-html-and-text' are disabled, writes only to text file.")]
    [OptionPrecedence(1)]
    bool ToHTML { get; set; }

    [Option("to-html-and-text", HelpText = "Whether to write comments to HTML file and to text file.")]
    [OptionPrecedence(2)]
    bool ToHTMLAndText { get; set; }

    [Option("dark-theme", HelpText = "Whether to use dark theme in HTML file.")]
    bool DarkTheme { get; set; }

    [Option("delete-json-file", HelpText = "Whether to delete the comments JSON file on successful completion.")]
    bool DeleteJsonFile { get; set; }

    [Option("disable-threading", HelpText = "Whether to disable comment threading.")]
    bool DisableThreading { get; set; }

    [Option("hide-comment-separators", HelpText = "Whether to hide separators between top-level comments.")]
    bool HideCommentSeparators { get; set; }

    [Option("hide-header", HelpText = "Whether to hide the video information header, containing title, URL, uploader, description.")]
    bool HideHeader { get; set; }

    [Option("hide-likes", HelpText = "Whether to hide likes count.")]
    bool HideLikes { get; set; }

    [Option("hide-replies", HelpText = "Whether to hide replies and leave only top-level comments.")]
    bool HideReplies { get; set; }

    [Option("hide-time", HelpText = "Whether to hide the posted time of the comment or reply.")]
    bool HideTime { get; set; }

    [Option("hide-video-description", HelpText = "Whether to hide the video description from the video information header.")]
    bool HideVideoDescription { get; set; }

    [Option("indent-size", HelpText = "The indentation size, in number of characters, between comment and reply. Indentation size is between 2 and 10 characters.")]
    int? IndentSize { get; set; }

    [Option("show-comment-link", HelpText = "Whether to show direct links comments and replies.")]
    bool ShowCommentLink { get; set; }

    [Option("show-comment-navigation-links", HelpText = "Whether to show next comment and previous comment navigation links in HTML file.")]
    bool ShowCommentNavigationLinks { get; set; }

    [Option("show-copy-links", HelpText = "Whether to show copy text links for comments and replies in HTML file.")]
    bool ShowCopyLinks { get; set; }

    [Option("text-line-length", HelpText = "The maximum length of a line of text, in number of characters. A comment's line of text, longer than that, is split into several lines. Line length is between 80 and 320 characters.")]
    int? TextLineLength { get; set; }
}

[OptionPrecedence(4)]
internal interface IUploaderOptions : IYouTubeCommentsToFileOptions
{
    [Option('u', "uh", HelpText = "Highlight uploader. Search for comments and replies written by the uploader of the video. If found, highlight the comment or reply.")]
    [OptionPrecedence(1)]
    bool HighlightUploader { get; set; }

    [Option("uf", HelpText = "Filter uploader. Search for comments and replies written by the uploader of the video. If found, keep the conversation (comment and its replies). Otherwise, remove the conversation, if it didn't match any other filter items.")]
    [OptionPrecedence(2)]
    bool FilterUploader { get; set; }

    [Option("uhf", HelpText = "Highlight and filter uploader.")]
    bool HighlightAndFilterUploader { get; set; }
}

[OptionDescription("Author name can be written with or without a leading @.")]
[OptionPrecedence(5)]
internal interface IAuthorsOptions : IYouTubeCommentsToFileOptions
{
    [Option("ah", HelpText = "Highlight authors. Search for comments and replies written by any of the specified authors. If found, highlight the comment or reply.")]
    [OptionPrecedence(1)]
    IEnumerable<string> HighlightAuthors { get; set; }

    [Option("af", HelpText = "Filter authors. Search for comments and replies written by any of the specified authors. If found, keep the conversation (comment and its replies). Otherwise, remove the conversation, if it didn't match any other filter items.")]
    [OptionPrecedence(2)]
    IEnumerable<string> FilterAuthors { get; set; }

    [Option("ahf", HelpText = "Highlight and filter authors.")]
    IEnumerable<string> HighlightAndFilterAuthors { get; set; }

    [Option('a', "ahi", HelpText = "Highlight authors. Author search is case-insensitive.")]
    IEnumerable<string> HighlightAuthorsIgnoreCase { get; set; }

    [Option("afi", HelpText = "Filter authors. Author search is case-insensitive.")]
    IEnumerable<string> FilterAuthorsIgnoreCase { get; set; }

    [Option("ahfi", HelpText = "Highlight and filter authors. Author search is case-insensitive.")]
    IEnumerable<string> HighlightAndFilterAuthorsIgnoreCase { get; set; }
}

[OptionPrecedence(6)]
internal interface ITextsOptions : IYouTubeCommentsToFileOptions
{
    [Option("th", HelpText = "Highlight texts. Search for any of the specified texts in the comment or reply. If found, highlight the comment or reply.")]
    [OptionPrecedence(1)]
    IEnumerable<string> HighlightTexts { get; set; }

    [Option("tf", HelpText = "Filter texts. Search for any of the specified texts in the comment or reply. If found, keep the conversation (comment and its replies). Otherwise, remove the conversation, if it didn't match any other filter items.")]
    [OptionPrecedence(2)]
    IEnumerable<string> FilterTexts { get; set; }

    [Option("thf", HelpText = "Highlight and filter texts.")]
    IEnumerable<string> HighlightAndFilterTexts { get; set; }

    [Option('t', "thi", HelpText = "Highlight texts. Text search is case-insensitive.")]
    IEnumerable<string> HighlightTextsIgnoreCase { get; set; }

    [Option("tfi", HelpText = "Filter texts. Text search is case-insensitive.")]
    IEnumerable<string> FilterTextsIgnoreCase { get; set; }

    [Option("thfi", HelpText = "Highlight and filter texts. Text search is case-insensitive.")]
    IEnumerable<string> HighlightAndFilterTextsIgnoreCase { get; set; }
}

[OptionPrecedence(7)]
internal interface IEncodingOptions : IYouTubeCommentsToFileOptions
{
    [Option("encoding-code-page", HelpText = "The code page of the video page encoding.")]
    [OptionPrecedence(1)]
    int? EncodingCodePage { get; set; }

    [Option("utf8", HelpText = "UTF-8. Code page 65001.")]
    [OptionPrecedence(2)]
    bool UTF8 { get; set; }

    [Option("arabic-iso", HelpText = "Arabic (ISO-8859-6). Code page 28596. Arabic, Persian, Urdu.")]
    bool ArabicISO { get; set; }

    [Option("arabic-windows", HelpText = "Arabic (Windows-1256). Code page 1256.")]
    bool ArabicWindows { get; set; }

    [Option("baltic-iso-4", HelpText = "Baltic (ISO-8859-4). Code page 28594. Estonian, Latvian, Lithuanian, Greenlandic, Sámi.")]
    bool BalticISO4 { get; set; }

    [Option("baltic-iso-13", HelpText = "Baltic (ISO-8859-13). Code page 28603. Lithuanian, Latvian, Estonian.")]
    bool BalticISO13 { get; set; }

    [Option("baltic-windows", HelpText = "Baltic (Windows-1257). Code page 1257.")]
    bool BalticWindows { get; set; }

    [Option("central-european", HelpText = "Central European (Windows-1250). Code page 1250.")]
    bool CentralEuropean { get; set; }

    [Option("chinese-simplified", HelpText = "Chinese Simplified (GB2312). Code page 936.")]
    bool ChineseSimplified { get; set; }

    [Option("chinese-traditional", HelpText = "Chinese Traditional (Big5). Code page 950.")]
    bool ChineseTraditional { get; set; }

    [Option("cyrillic-iso", HelpText = "Cyrillic (ISO-8859-5). Code page 28595. Russian, Ukrainian, Bulgarian, Belarusian, Macedonian, Serbian.")]
    bool CyrillicISO { get; set; }

    [Option("cyrillic-windows", HelpText = "Cyrillic (Windows-1251). Code page 1251.")]
    bool CyrillicWindows { get; set; }

    [Option("eastern-european", HelpText = "Eastern European (ISO-8859-2). Code page 28592. Czech, Hungarian, Polish, Romanian, Croatian, Slovak, Slovenian.")]
    bool EasternEuropean { get; set; }

    [Option("greek-iso", HelpText = "Greek (ISO-8859-7). Code page 28597.")]
    bool GreekISO { get; set; }

    [Option("greek-windows", HelpText = "Greek (Windows-1253). Code page 1253.")]
    bool GreekWindows { get; set; }

    [Option("hebrew-iso", HelpText = "Hebrew (ISO-8859-8). Code page 28598.")]
    bool HebrewISO { get; set; }

    [Option("hebrew-windows", HelpText = "Hebrew (Windows-1255). Code page 1255.")]
    bool HebrewWindows { get; set; }

    [Option("japanese", HelpText = "Japanese (Shift-JIS). Code page 932.")]
    bool Japanese { get; set; }

    [Option("korean", HelpText = "Korean (Windows-949). Code page 949.")]
    bool Korean { get; set; }

    [Option("south-european", HelpText = "South European (ISO-8859-3). Code page 28593. Maltese, Esperanto, Turkish.")]
    bool SouthEuropean { get; set; }

    [Option("thai", HelpText = "Thai (TIS-620). Code page 874.")]
    bool Thai { get; set; }

    [Option("turkish-iso", HelpText = "Turkish (ISO-8859-9). Code page 28599.")]
    bool TurkishISO { get; set; }

    [Option("turkish-windows", HelpText = "Turkish (Windows-1254). Code page 1254.")]
    bool TurkishWindows { get; set; }

    [Option("vietnamese", HelpText = "Vietnamese (Windows-1258). Code page 1258.")]
    bool Vietnamese { get; set; }

    [Option("western-european-iso-1", HelpText = "Western European (ISO-8859-1). Code page 28591. English, French, Spanish, German, Italian, Portuguese.")]
    bool WesternEuropeanISO1 { get; set; }

    [Option("western-european-iso-15", HelpText = "Western European (ISO-8859-15). Code page 28605. English, French, German, Spanish, Finnish, Danish, Swedish, Italian, Dutch.")]
    bool WesternEuropeanISO15 { get; set; }

    [Option("western-european-windows", HelpText = "Western European (Windows-1252). Code page 1252.")]
    bool WesternEuropeanWindows { get; set; }
}

[OptionDescription("A color value can be a known name ('Honeydew'), hex code ('#F0FFF0'; 'F0FFF0') or RGB ('240, 255, 240'; '240 255 240').")]
[OptionPrecedence(8)]
internal interface IHTMLColorsOptions : IYouTubeCommentsToFileOptions
{
    [Option("text-color", HelpText = "The text color of comments.")]
    [OptionPrecedence(1)]
    string TextColor { get; set; }

    [Option("background-color", HelpText = "The background color of comments.")]
    [OptionPrecedence(2)]
    string BackgroundColor { get; set; }

    [Option("author-color", HelpText = "The text color of the author of the comment.")]
    [OptionPrecedence(3)]
    string AuthorColor { get; set; }

    [Option("replied-author-color", HelpText = "The text color of an author, of other comment, appearing at the start of the comment.")]
    [OptionPrecedence(4)]
    string RepliedAuthorColor { get; set; }

    [Option("other-author-color", HelpText = "The text color of an author, of other comment, appearing anywhere else but the start of the comment.")]
    [OptionPrecedence(5)]
    string OtherAuthorColor { get; set; }

    [Option("small-elements-color", HelpText = "The text color of small elements, such as pinned comment, comment time, likes.")]
    [OptionPrecedence(6)]
    string SmallElementsColor { get; set; }

    [Option("favorited-background-color", HelpText = "The background color of favorited by the uploader of the video.")]
    [OptionPrecedence(7)]
    string FavoritedBackgroundColor { get; set; }

    [Option("header-background-color", HelpText = "The background color of the header.")]
    [OptionPrecedence(8)]
    string HeaderBackgroundColor { get; set; }

    [Option("highlight-uploader-text-color", HelpText = "The text color of highlighted comments written by the uploader of the video.")]
    [OptionPrecedence(9)]
    string HighlightUploaderTextColor { get; set; }

    [Option("highlight-uploader-background-color", HelpText = "The background color of highlighted comments written by the uploader of the video.")]
    [OptionPrecedence(10)]
    string HighlightUploaderBackgroundColor { get; set; }

    [Option("highlight-uploader-border-color", HelpText = "The color of the border around highlighted comments written by the uploader of the video.")]
    [OptionPrecedence(11)]
    string HighlightUploaderBorderColor { get; set; }

    [Option("highlight-author-text-color", HelpText = "The text color of highlighted comments written by any of the searched authors.")]
    [OptionPrecedence(12)]
    string HighlightAuthorTextColor { get; set; }

    [Option("highlight-author-background-color", HelpText = "The background color of highlighted comments written by any of the searched authors.")]
    [OptionPrecedence(13)]
    string HighlightAuthorBackgroundColor { get; set; }

    [Option("highlight-author-border-color", HelpText = "The color of the border around highlighted comments written by any of the searched authors.")]
    [OptionPrecedence(14)]
    string HighlightAuthorBorderColor { get; set; }

    [Option("highlight-text-text-color", HelpText = "The text color of highlighted comments containing any of the searched texts.")]
    [OptionPrecedence(15)]
    string HighlightTextTextColor { get; set; }

    [Option("highlight-text-background-color", HelpText = "The background color of highlighted comments containing any of the searched texts.")]
    [OptionPrecedence(16)]
    string HighlightTextBackgroundColor { get; set; }

    [Option("highlight-text-border-color", HelpText = "The color of the border around highlighted comments containing any of the searched texts.")]
    [OptionPrecedence(17)]
    string HighlightTextBorderColor { get; set; }

    [Option("link-color", HelpText = "The text color of HTML links.")]
    [OptionPrecedence(18)]
    string LinkColor { get; set; }

    [Option("copy-text-success-color", HelpText = "The text color of a success message after copying a comment to the clipboard.")]
    [OptionPrecedence(19)]
    string CopyTextSuccessColor { get; set; }

    [Option("copy-text-failure-color", HelpText = "The text color of an error message after failing to copy a comment to the clipboard.")]
    [OptionPrecedence(20)]
    string CopyTextFailureColor { get; set; }
}

[OptionPrecedence(9)]
internal interface IConfigurationOptions : IYouTubeCommentsToFileOptions
{
    [Option("config-locations", HelpText = "Paths to configuration files or to directories containing configuration files. Can be used multiple times and inside other configuration files.")]
    [OptionPrecedence(1)]
    IEnumerable<string> ConfigLocations { get; set; }

    [Option("ignore-config", HelpText = "Don't load the config location and continue loading the rest of the configuration stack. In the command line, all config locations, specified with '--config-locations', are not loaded. In a configuration file, the file is not loaded.")]
    bool IgnoreConfig { get; set; }

    [Option("stop-config", HelpText = "Don't load the config location and stop loading the rest of the configuration stack. In the command line, don't load the whole configuration stack, including all config locations, specified with '--config-locations'. In a configuration file, don't load the file and stop loading the rest of the configuration stack.")]
    bool StopConfig { get; set; }
}

[OptionPrecedence(10)]
internal interface IVerbosityOptions : IYouTubeCommentsToFileOptions
{
    [Option('v', "verbose", HelpText = "Print debugging information.")]
    [OptionPrecedence(1)]
    bool Verbose { get; set; }

    [Option('q', "quiet", HelpText = "Don't print anything. If used with '--verbose', print to standard error (stderr).")]
    [OptionPrecedence(2)]
    bool Quiet { get; set; }

    [Option("yt-dlp-quiet", HelpText = "Don't print output from yt-dlp.")]
    bool YtDlpQuiet { get; set; }

    [Option("no-write-comments", HelpText = "Don't download or write comments.")]
    public bool NoWriteComments { get; set; }
}

[OptionPrecedence(11)]
internal interface IGeneralOptions : IYouTubeCommentsToFileOptions
{
    [Option("beep", HelpText = "Make a beep sound when finished. If used with '--quiet', make no sound.")]
    [OptionPrecedence(1)]
    bool Beep { get; set; }

    [Option("beep-on-errors", HelpText = "Make a beep sound when finished with errors. If used with '--quiet', make no sound.")]
    [OptionPrecedence(2)]
    bool BeepOnErrors { get; set; }

    [Option("pause-on-errors", HelpText = "Prevent the console from closing until a key is pressed when finished with errors. If used with '--quiet', this option is disabled.")]
    [OptionPrecedence(3)]
    bool PauseOnErrors { get; set; }

    [Option('h', "help", HelpText = "Display help screen.")]
    bool Help { get; set; }

    [Option("version", HelpText = "Display version information.")]
    bool Version { get; set; }
}

#if DEBUG
[OptionPrecedence(12)]
internal interface IDebugOptions : IYouTubeCommentsToFileOptions
{
    [Option("comments-with-emoji", HelpText = "Filter and highlight comments with emoji.")]
    public bool FilterAndHighlightCommentsWithEmoji { get; set; }

    [Option("hac", HelpText = "Highlight all comments.")]
    public bool HighlightAllComments { get; set; }
}
#endif

#endregion

#region Options

internal interface IOperationOptions :
    IConfigurationOptions,
    IVerbosityOptions,
    IGeneralOptions
#if DEBUG
    , IDebugOptions
#endif
{
    string Verb { get; }
    bool IsDefaultVerb { get; }
    string GetHomeConfigurationDirectory(Func<string, string> FixPath);
    void FixPaths(Func<string, string> FixPath);
    string NameOf();
    Settings ToSettings();
    string ToString();
    string ToString(Parser parser);
    string[] ToArgs(Parser parser);
}

internal interface IOperationOptions<TOptions> : IOperationOptions
    where TOptions : SharedOptions, IOperationOptions
{
    void MergeOptions(TOptions options);
}

[Verb("convert", false)]
internal class ConvertOptions : SharedOptions, IConvertOptions, IOperationOptions<ConvertOptions>
{
    public string Verb => "convert";
    public bool IsDefaultVerb => false;

    public string JsonFile { get; set; }
    public string URL { get; set; }
    public string YtDlp { get; set; }
    public string Deno { get; set; }

    public string GetHomeConfigurationDirectory(Func<string, string> FixPath)
    {
        return Path.GetDirectoryName(FixPath(JsonFile));
    }

    public void FixPaths(Func<string, string> FixPath)
    {
        JsonFile = FixPath(JsonFile);
        YtDlp = FixPath(YtDlp);
        Deno = FixPath(Deno);
    }

    public string NameOf()
    {
        return nameof(ConvertOptions);
    }

    public override Settings ToSettings()
    {
        var settings = base.ToSettings();
        settings.Download = false;
        settings.JsonFile = JsonFile;
        settings.URL = URL;
        settings.YtDlp = YtDlp;
        settings.Deno = Deno;
        return settings;
    }

    public void MergeOptions(ConvertOptions options)
    {
        if (options == null)
            return;

        base.MergeOptions(options);
        if (string.IsNullOrEmpty(JsonFile)) JsonFile = options.JsonFile;
        if (string.IsNullOrEmpty(URL)) URL = options.URL;
        if (string.IsNullOrEmpty(YtDlp)) YtDlp = options.YtDlp;
        if (string.IsNullOrEmpty(Deno)) Deno = options.Deno;
    }
}

[Verb("download", true)]
internal class DownloadOptions : SharedOptions, IDownloadOptions, IOperationOptions<DownloadOptions>
{
    public string Verb => "download";
    public bool IsDefaultVerb => true;

    public string URL { get; set; }
    public string YtDlp { get; set; }
    public string Deno { get; set; }
    public string DownloadPath { get; set; }
    public uint? MaxComments { get; set; }
    public uint? MaxParents { get; set; }
    public uint? MaxReplies { get; set; }
    public uint? MaxRepliesPerThread { get; set; }
    public bool SortNew { get; set; }
    public bool SortTop { get; set; }
    public bool FilenameSanitization { get; set; }
    public bool OnlyDownloadComments { get; set; }
    public bool RestrictFilenames { get; set; }
    public uint? TrimTitle { get; set; }
    public bool UpdateYtDlp { get; set; }
    public bool WindowsFilenames { get; set; }
    public string YtDlpOptions { get; set; }

    public string GetHomeConfigurationDirectory(Func<string, string> FixPath)
    {
        return Path.TrimEndingDirectorySeparator(FixPath(DownloadPath));
    }

    public void FixPaths(Func<string, string> FixPath)
    {
        DownloadPath = FixPath(DownloadPath);
        YtDlp = FixPath(YtDlp);
        Deno = FixPath(Deno);
    }

    public string NameOf()
    {
        return nameof(DownloadOptions);
    }

    public override Settings ToSettings()
    {
        var settings = base.ToSettings();
        settings.Download = true;
        settings.URL = URL;
        settings.YtDlp = YtDlp;
        settings.Deno = Deno;
        settings.DownloadPath = DownloadPath;
        settings.MaxComments = MaxComments;
        settings.MaxParents = MaxParents;
        settings.MaxReplies = MaxReplies;
        settings.MaxRepliesPerThread = MaxRepliesPerThread;
        settings.SortNew = SortNew;
        settings.SortTop = SortTop;
        settings.FilenameSanitization = FilenameSanitization;
        settings.OnlyDownloadComments = OnlyDownloadComments;
        settings.RestrictFilenames = RestrictFilenames;
        settings.TrimTitle = TrimTitle;
        settings.UpdateYtDlp = UpdateYtDlp;
        settings.WindowsFilenames = WindowsFilenames;
        settings.YtDlpOptions = YtDlpOptions;
        return settings;
    }

    public void MergeOptions(DownloadOptions options)
    {
        if (options == null)
            return;

        base.MergeOptions(options);
        if (string.IsNullOrEmpty(URL)) URL = options.URL;
        if (string.IsNullOrEmpty(YtDlp)) YtDlp = options.YtDlp;
        if (string.IsNullOrEmpty(Deno)) Deno = options.Deno;
        if (string.IsNullOrEmpty(DownloadPath)) DownloadPath = options.DownloadPath;
        MaxComments ??= options.MaxComments;
        MaxParents ??= options.MaxParents;
        MaxReplies ??= options.MaxReplies;
        MaxRepliesPerThread ??= options.MaxRepliesPerThread;
        SortNew |= options.SortNew;
        SortTop |= options.SortTop;
        FilenameSanitization |= options.FilenameSanitization;
        OnlyDownloadComments |= options.OnlyDownloadComments;
        RestrictFilenames |= options.RestrictFilenames;
        TrimTitle ??= options.TrimTitle;
        UpdateYtDlp |= options.UpdateYtDlp;
        WindowsFilenames |= options.WindowsFilenames;
        if (string.IsNullOrEmpty(YtDlpOptions)) YtDlpOptions = options.YtDlpOptions;
    }
}

internal abstract class SharedOptions :
    ISharedOptions,
    IUploaderOptions,
    IAuthorsOptions,
    ITextsOptions,
    IEncodingOptions,
    IHTMLColorsOptions,
    IConfigurationOptions,
    IVerbosityOptions,
    IGeneralOptions
#if DEBUG
    , IDebugOptions
#endif
{
    // Shared
    public bool ToHTML { get; set; }
    public bool ToHTMLAndText { get; set; }
    public bool DarkTheme { get; set; }
    public bool DeleteJsonFile { get; set; }
    public bool DisableThreading { get; set; }
    public bool HideCommentSeparators { get; set; }
    public bool HideHeader { get; set; }
    public bool HideLikes { get; set; }
    public bool HideReplies { get; set; }
    public bool HideTime { get; set; }
    public bool HideVideoDescription { get; set; }
    public int? IndentSize { get; set; }
    public bool ShowCommentLink { get; set; }
    public bool ShowCommentNavigationLinks { get; set; }
    public bool ShowCopyLinks { get; set; }
    public int? TextLineLength { get; set; }

    // Uploader
    public bool HighlightUploader { get; set; }
    public bool FilterUploader { get; set; }
    public bool HighlightAndFilterUploader { get; set; }

    // Authors
    public IEnumerable<string> HighlightAuthors { get; set; }
    public IEnumerable<string> FilterAuthors { get; set; }
    public IEnumerable<string> HighlightAndFilterAuthors { get; set; }
    public IEnumerable<string> HighlightAuthorsIgnoreCase { get; set; }
    public IEnumerable<string> FilterAuthorsIgnoreCase { get; set; }
    public IEnumerable<string> HighlightAndFilterAuthorsIgnoreCase { get; set; }

    // Texts
    public IEnumerable<string> HighlightTexts { get; set; }
    public IEnumerable<string> FilterTexts { get; set; }
    public IEnumerable<string> HighlightAndFilterTexts { get; set; }
    public IEnumerable<string> HighlightTextsIgnoreCase { get; set; }
    public IEnumerable<string> FilterTextsIgnoreCase { get; set; }
    public IEnumerable<string> HighlightAndFilterTextsIgnoreCase { get; set; }

    // Encoding
    public int? EncodingCodePage { get; set; }
    public bool UTF8 { get; set; }
    public bool ArabicISO { get; set; }
    public bool ArabicWindows { get; set; }
    public bool BalticISO4 { get; set; }
    public bool BalticISO13 { get; set; }
    public bool BalticWindows { get; set; }
    public bool CentralEuropean { get; set; }
    public bool ChineseSimplified { get; set; }
    public bool ChineseTraditional { get; set; }
    public bool CyrillicISO { get; set; }
    public bool CyrillicWindows { get; set; }
    public bool EasternEuropean { get; set; }
    public bool GreekISO { get; set; }
    public bool GreekWindows { get; set; }
    public bool HebrewISO { get; set; }
    public bool HebrewWindows { get; set; }
    public bool Japanese { get; set; }
    public bool Korean { get; set; }
    public bool SouthEuropean { get; set; }
    public bool Thai { get; set; }
    public bool TurkishISO { get; set; }
    public bool TurkishWindows { get; set; }
    public bool Vietnamese { get; set; }
    public bool WesternEuropeanISO1 { get; set; }
    public bool WesternEuropeanISO15 { get; set; }
    public bool WesternEuropeanWindows { get; set; }

    // HTML Colors
    public string TextColor { get; set; }
    public string BackgroundColor { get; set; }
    public string AuthorColor { get; set; }
    public string RepliedAuthorColor { get; set; }
    public string OtherAuthorColor { get; set; }
    public string SmallElementsColor { get; set; }
    public string FavoritedBackgroundColor { get; set; }
    public string HeaderBackgroundColor { get; set; }
    public string HighlightUploaderTextColor { get; set; }
    public string HighlightUploaderBackgroundColor { get; set; }
    public string HighlightUploaderBorderColor { get; set; }
    public string HighlightAuthorTextColor { get; set; }
    public string HighlightAuthorBackgroundColor { get; set; }
    public string HighlightAuthorBorderColor { get; set; }
    public string HighlightTextTextColor { get; set; }
    public string HighlightTextBackgroundColor { get; set; }
    public string HighlightTextBorderColor { get; set; }
    public string LinkColor { get; set; }
    public string CopyTextSuccessColor { get; set; }
    public string CopyTextFailureColor { get; set; }

    // Configuration
    public IEnumerable<string> ConfigLocations { get; set; }
    public bool IgnoreConfig { get; set; }
    public bool StopConfig { get; set; }

    // Verbosity
    public bool Verbose { get; set; }
    public bool Quiet { get; set; }
    public bool YtDlpQuiet { get; set; }
    public bool NoWriteComments { get; set; }

    // General
    public bool Beep { get; set; }
    public bool BeepOnErrors { get; set; }
    public bool PauseOnErrors { get; set; }
    public bool Help { get; set; }
    public bool Version { get; set; }

#if DEBUG
    // Debug
    public bool FilterAndHighlightCommentsWithEmoji { get; set; }
    public bool HighlightAllComments { get; set; }
#endif

    public virtual Settings ToSettings()
    {
        var settings = new Settings
        {
            // Shared
            ToHTML = ToHTML,
            ToHTMLAndText = ToHTMLAndText,
            DarkTheme = DarkTheme,
            DeleteJsonFile = DeleteJsonFile,
            DisableThreading = DisableThreading,
            HideCommentSeparators = HideCommentSeparators,
            HideHeader = HideHeader,
            HideLikes = HideLikes,
            HideReplies = HideReplies,
            HideTime = HideTime,
            HideVideoDescription = HideVideoDescription,
            IndentSize = IndentSize,
            ShowCommentLink = ShowCommentLink,
            ShowCommentNavigationLinks = ShowCommentNavigationLinks,
            ShowCopyLinks = ShowCopyLinks,
            TextLineLength = TextLineLength,

            // Encoding
            EncodingCodePage = EncodingCodePage,
            UTF8 = UTF8,
            ArabicISO = ArabicISO,
            ArabicWindows = ArabicWindows,
            BalticISO4 = BalticISO4,
            BalticISO13 = BalticISO13,
            BalticWindows = BalticWindows,
            CentralEuropean = CentralEuropean,
            ChineseSimplified = ChineseSimplified,
            ChineseTraditional = ChineseTraditional,
            CyrillicISO = CyrillicISO,
            CyrillicWindows = CyrillicWindows,
            EasternEuropean = EasternEuropean,
            GreekISO = GreekISO,
            GreekWindows = GreekWindows,
            HebrewISO = HebrewISO,
            HebrewWindows = HebrewWindows,
            Japanese = Japanese,
            Korean = Korean,
            SouthEuropean = SouthEuropean,
            Thai = Thai,
            TurkishISO = TurkishISO,
            TurkishWindows = TurkishWindows,
            Vietnamese = Vietnamese,
            WesternEuropeanISO1 = WesternEuropeanISO1,
            WesternEuropeanISO15 = WesternEuropeanISO15,
            WesternEuropeanWindows = WesternEuropeanWindows,

            // HTML Colors
            TextColor = ColorHelper.StringToColor(TextColor),
            BackgroundColor = ColorHelper.StringToColor(BackgroundColor),
            AuthorColor = ColorHelper.StringToColor(AuthorColor),
            RepliedAuthorColor = ColorHelper.StringToColor(RepliedAuthorColor),
            OtherAuthorColor = ColorHelper.StringToColor(OtherAuthorColor),
            SmallElementsColor = ColorHelper.StringToColor(SmallElementsColor),
            FavoritedBackgroundColor = ColorHelper.StringToColor(FavoritedBackgroundColor),
            HeaderBackgroundColor = ColorHelper.StringToColor(HeaderBackgroundColor),
            HighlightUploaderTextColor = ColorHelper.StringToColor(HighlightUploaderTextColor),
            HighlightUploaderBackgroundColor = ColorHelper.StringToColor(HighlightUploaderBackgroundColor),
            HighlightUploaderBorderColor = ColorHelper.StringToColor(HighlightUploaderBorderColor),
            HighlightAuthorTextColor = ColorHelper.StringToColor(HighlightAuthorTextColor),
            HighlightAuthorBackgroundColor = ColorHelper.StringToColor(HighlightAuthorBackgroundColor),
            HighlightAuthorBorderColor = ColorHelper.StringToColor(HighlightAuthorBorderColor),
            HighlightTextTextColor = ColorHelper.StringToColor(HighlightTextTextColor),
            HighlightTextBackgroundColor = ColorHelper.StringToColor(HighlightTextBackgroundColor),
            HighlightTextBorderColor = ColorHelper.StringToColor(HighlightTextBorderColor),
            LinkColor = ColorHelper.StringToColor(LinkColor),
            CopyTextSuccessColor = ColorHelper.StringToColor(CopyTextSuccessColor),
            CopyTextFailureColor = ColorHelper.StringToColor(CopyTextFailureColor),

            // Verbosity
            NoWriteComments = NoWriteComments,

#if DEBUG
            // Debug
            FilterAndHighlightCommentsWithEmoji = FilterAndHighlightCommentsWithEmoji,
            HighlightAllComments = HighlightAllComments
#endif
        };

        // Uploader
        if (HighlightAndFilterUploader || (HighlightUploader && FilterUploader))
            settings.SearchItems.Add(new SearchUploader(highlight: true, filter: true));
        else if (HighlightUploader)
            settings.SearchItems.Add(SearchUploader.HighlightUploader);
        else if (FilterUploader)
            settings.SearchItems.Add(SearchUploader.FilterUploader);

        // Authors
        if (HighlightAuthors.HasAny())
        {
            foreach (var author in HighlightAuthors)
                settings.SearchItems.Add(SearchAuthor.HighlightAuthor(author));
        }

        if (FilterAuthors.HasAny())
        {
            foreach (var author in FilterAuthors)
                settings.SearchItems.Add(SearchAuthor.FilterAuthor(author));
        }

        if (HighlightAndFilterAuthors.HasAny())
        {
            foreach (var author in HighlightAndFilterAuthors)
                settings.SearchItems.Add(new SearchAuthor(author, highlight: true, filter: true));
        }

        if (HighlightAuthorsIgnoreCase.HasAny())
        {
            foreach (var author in HighlightAuthorsIgnoreCase)
                settings.SearchItems.Add(SearchAuthor.HighlightAuthor(author, ignoreCase: true));
        }

        if (FilterAuthorsIgnoreCase.HasAny())
        {
            foreach (var author in FilterAuthorsIgnoreCase)
                settings.SearchItems.Add(SearchAuthor.FilterAuthor(author, ignoreCase: true));
        }

        if (HighlightAndFilterAuthorsIgnoreCase.HasAny())
        {
            foreach (var author in HighlightAndFilterAuthorsIgnoreCase)
                settings.SearchItems.Add(new SearchAuthor(author, highlight: true, filter: true, ignoreCase: true));
        }

        // Texts
        if (HighlightTexts.HasAny())
        {
            foreach (var text in HighlightTexts)
                settings.SearchItems.Add(SearchText.HighlightText(text));
        }

        if (FilterTexts.HasAny())
        {
            foreach (var text in FilterTexts)
                settings.SearchItems.Add(SearchText.FilterText(text));
        }

        if (HighlightAndFilterTexts.HasAny())
        {
            foreach (var text in HighlightAndFilterTexts)
                settings.SearchItems.Add(new SearchText(text, highlight: true, filter: true));
        }

        if (HighlightTextsIgnoreCase.HasAny())
        {
            foreach (var text in HighlightTextsIgnoreCase)
                settings.SearchItems.Add(SearchText.HighlightText(text, ignoreCase: true));
        }

        if (FilterTextsIgnoreCase.HasAny())
        {
            foreach (var text in FilterTextsIgnoreCase)
                settings.SearchItems.Add(SearchText.FilterText(text, ignoreCase: true));
        }

        if (HighlightAndFilterTextsIgnoreCase.HasAny())
        {
            foreach (var text in HighlightAndFilterTextsIgnoreCase)
                settings.SearchItems.Add(new SearchText(text, highlight: true, filter: true, ignoreCase: true));
        }

        return settings;
    }

    public virtual void MergeOptions(SharedOptions options)
    {
        if (options == null)
            return;

        // Shared
        ToHTML |= options.ToHTML;
        ToHTMLAndText |= options.ToHTMLAndText;
        DarkTheme |= options.DarkTheme;
        DeleteJsonFile |= options.DeleteJsonFile;
        DisableThreading |= options.DisableThreading;
        HideCommentSeparators |= options.HideCommentSeparators;
        HideHeader |= options.HideHeader;
        HideLikes |= options.HideLikes;
        HideReplies |= options.HideReplies;
        HideTime |= options.HideTime;
        HideVideoDescription |= options.HideVideoDescription;
        IndentSize ??= options.IndentSize;
        ShowCommentLink |= options.ShowCommentLink;
        ShowCommentNavigationLinks |= options.ShowCommentNavigationLinks;
        ShowCopyLinks |= options.ShowCopyLinks;
        TextLineLength ??= options.TextLineLength;

        // Uploader
        HighlightUploader |= options.HighlightUploader;
        FilterUploader |= options.FilterUploader;
        HighlightAndFilterUploader |= options.HighlightAndFilterUploader;

        // Authors
        if (HighlightAuthors.IsNullOrEmpty()) HighlightAuthors = options.HighlightAuthors;
        if (FilterAuthors.IsNullOrEmpty()) FilterAuthors = options.FilterAuthors;
        if (HighlightAndFilterAuthors.IsNullOrEmpty()) HighlightAndFilterAuthors = options.HighlightAndFilterAuthors;
        if (HighlightAuthorsIgnoreCase.IsNullOrEmpty()) HighlightAuthorsIgnoreCase = options.HighlightAuthorsIgnoreCase;
        if (FilterAuthorsIgnoreCase.IsNullOrEmpty()) FilterAuthorsIgnoreCase = options.FilterAuthorsIgnoreCase;
        if (HighlightAndFilterAuthorsIgnoreCase.IsNullOrEmpty()) HighlightAndFilterAuthorsIgnoreCase = options.HighlightAndFilterAuthorsIgnoreCase;

        // Texts
        if (HighlightTexts.IsNullOrEmpty()) HighlightTexts = options.HighlightTexts;
        if (FilterTexts.IsNullOrEmpty()) FilterTexts = options.FilterTexts;
        if (HighlightAndFilterTexts.IsNullOrEmpty()) HighlightAndFilterTexts = options.HighlightAndFilterTexts;
        if (HighlightTextsIgnoreCase.IsNullOrEmpty()) HighlightTextsIgnoreCase = options.HighlightTextsIgnoreCase;
        if (FilterTextsIgnoreCase.IsNullOrEmpty()) FilterTextsIgnoreCase = options.FilterTextsIgnoreCase;
        if (HighlightAndFilterTextsIgnoreCase.IsNullOrEmpty()) HighlightAndFilterTextsIgnoreCase = options.HighlightAndFilterTextsIgnoreCase;

        // Encoding
        EncodingCodePage ??= options.EncodingCodePage;
        UTF8 |= options.UTF8;
        ArabicISO |= options.ArabicISO;
        ArabicWindows |= options.ArabicWindows;
        BalticISO4 |= options.BalticISO4;
        BalticISO13 |= options.BalticISO13;
        BalticWindows |= options.BalticWindows;
        CentralEuropean |= options.CentralEuropean;
        ChineseSimplified |= options.ChineseSimplified;
        ChineseTraditional |= options.ChineseTraditional;
        CyrillicISO |= options.CyrillicISO;
        CyrillicWindows |= options.CyrillicWindows;
        EasternEuropean |= options.EasternEuropean;
        GreekISO |= options.GreekISO;
        GreekWindows |= options.GreekWindows;
        HebrewISO |= options.HebrewISO;
        HebrewWindows |= options.HebrewWindows;
        Japanese |= options.Japanese;
        Korean |= options.Korean;
        SouthEuropean |= options.SouthEuropean;
        Thai |= options.Thai;
        TurkishISO |= options.TurkishISO;
        TurkishWindows |= options.TurkishWindows;
        Vietnamese |= options.Vietnamese;
        WesternEuropeanISO1 |= options.WesternEuropeanISO1;
        WesternEuropeanISO15 |= options.WesternEuropeanISO15;
        WesternEuropeanWindows |= options.WesternEuropeanWindows;

        // HTML Colors
        if (string.IsNullOrEmpty(TextColor)) TextColor = options.TextColor;
        if (string.IsNullOrEmpty(BackgroundColor)) BackgroundColor = options.BackgroundColor;
        if (string.IsNullOrEmpty(AuthorColor)) AuthorColor = options.AuthorColor;
        if (string.IsNullOrEmpty(RepliedAuthorColor)) RepliedAuthorColor = options.RepliedAuthorColor;
        if (string.IsNullOrEmpty(OtherAuthorColor)) OtherAuthorColor = options.OtherAuthorColor;
        if (string.IsNullOrEmpty(SmallElementsColor)) SmallElementsColor = options.SmallElementsColor;
        if (string.IsNullOrEmpty(FavoritedBackgroundColor)) FavoritedBackgroundColor = options.FavoritedBackgroundColor;
        if (string.IsNullOrEmpty(HeaderBackgroundColor)) HeaderBackgroundColor = options.HeaderBackgroundColor;
        if (string.IsNullOrEmpty(HighlightUploaderTextColor)) HighlightUploaderTextColor = options.HighlightUploaderTextColor;
        if (string.IsNullOrEmpty(HighlightUploaderBackgroundColor)) HighlightUploaderBackgroundColor = options.HighlightUploaderBackgroundColor;
        if (string.IsNullOrEmpty(HighlightUploaderBorderColor)) HighlightUploaderBorderColor = options.HighlightUploaderBorderColor;
        if (string.IsNullOrEmpty(HighlightAuthorTextColor)) HighlightAuthorTextColor = options.HighlightAuthorTextColor;
        if (string.IsNullOrEmpty(HighlightAuthorBackgroundColor)) HighlightAuthorBackgroundColor = options.HighlightAuthorBackgroundColor;
        if (string.IsNullOrEmpty(HighlightAuthorBorderColor)) HighlightAuthorBorderColor = options.HighlightAuthorBorderColor;
        if (string.IsNullOrEmpty(HighlightTextTextColor)) HighlightTextTextColor = options.HighlightTextTextColor;
        if (string.IsNullOrEmpty(HighlightTextBackgroundColor)) HighlightTextBackgroundColor = options.HighlightTextBackgroundColor;
        if (string.IsNullOrEmpty(HighlightTextBorderColor)) HighlightTextBorderColor = options.HighlightTextBorderColor;
        if (string.IsNullOrEmpty(LinkColor)) LinkColor = options.LinkColor;
        if (string.IsNullOrEmpty(CopyTextSuccessColor)) CopyTextSuccessColor = options.CopyTextSuccessColor;
        if (string.IsNullOrEmpty(CopyTextFailureColor)) CopyTextFailureColor = options.CopyTextFailureColor;

        // Verbosity
        Verbose |= options.Verbose;
        Quiet |= options.Quiet;
        YtDlpQuiet |= options.YtDlpQuiet;

        // General
        Beep |= options.Beep;
        BeepOnErrors |= options.BeepOnErrors;
        PauseOnErrors |= options.PauseOnErrors;
        Help |= options.Help;
        Version |= options.Version;
    }

    public override string ToString()
    {
        return ToString(null);
    }

    public virtual string ToString(Parser parser)
    {
        return (parser ?? Parser.Default).FormatCommandLine(this);
    }

    public virtual string[] ToArgs(Parser parser)
    {
        return (parser ?? Parser.Default).FormatCommandLineArgs(this);
    }
}

#endregion

#region Help Options

internal class ConvertHelpOptions : IConvertOptions
{
    public string JsonFile { get; set; }
    public string URL { get; set; }
    public string YtDlp { get; set; }
    public string Deno { get; set; }
}

internal class DownloadHelpOptions : IDownloadOptions
{
    public string URL { get; set; }
    public string YtDlp { get; set; }
    public string Deno { get; set; }
    public string DownloadPath { get; set; }
    public bool FilenameSanitization { get; set; }
    public uint? MaxComments { get; set; }
    public uint? MaxParents { get; set; }
    public uint? MaxReplies { get; set; }
    public uint? MaxRepliesPerThread { get; set; }
    public bool OnlyDownloadComments { get; set; }
    public bool RestrictFilenames { get; set; }
    public bool SortNew { get; set; }
    public bool SortTop { get; set; }
    public uint? TrimTitle { get; set; }
    public bool UpdateYtDlp { get; set; }
    public bool WindowsFilenames { get; set; }
    public string YtDlpOptions { get; set; }
}

internal class SharedHelpOptions : ISharedOptions
{
    public bool ToHTML { get; set; }
    public bool ToHTMLAndText { get; set; }
    public bool DarkTheme { get; set; }
    public bool DeleteJsonFile { get; set; }
    public bool DisableThreading { get; set; }
    public bool HideCommentSeparators { get; set; }
    public bool HideHeader { get; set; }
    public bool HideLikes { get; set; }
    public bool HideReplies { get; set; }
    public bool HideTime { get; set; }
    public bool HideVideoDescription { get; set; }
    public int? IndentSize { get; set; }
    public bool ShowCommentLink { get; set; }
    public bool ShowCommentNavigationLinks { get; set; }
    public bool ShowCopyLinks { get; set; }
    public int? TextLineLength { get; set; }
}

internal class UploaderHelpOptions : IUploaderOptions
{
    public bool HighlightUploader { get; set; }
    public bool FilterUploader { get; set; }
    public bool HighlightAndFilterUploader { get; set; }
}

internal class AuthorsHelpOptions : IAuthorsOptions
{
    public IEnumerable<string> HighlightAuthors { get; set; }
    public IEnumerable<string> FilterAuthors { get; set; }
    public IEnumerable<string> HighlightAndFilterAuthors { get; set; }
    public IEnumerable<string> HighlightAuthorsIgnoreCase { get; set; }
    public IEnumerable<string> FilterAuthorsIgnoreCase { get; set; }
    public IEnumerable<string> HighlightAndFilterAuthorsIgnoreCase { get; set; }
}

internal class TextsHelpOptions : ITextsOptions
{
    public IEnumerable<string> HighlightTexts { get; set; }
    public IEnumerable<string> FilterTexts { get; set; }
    public IEnumerable<string> HighlightAndFilterTexts { get; set; }
    public IEnumerable<string> HighlightTextsIgnoreCase { get; set; }
    public IEnumerable<string> FilterTextsIgnoreCase { get; set; }
    public IEnumerable<string> HighlightAndFilterTextsIgnoreCase { get; set; }
}

internal class EncodingHelpOptions : IEncodingOptions
{
    public int? EncodingCodePage { get; set; }
    public bool UTF8 { get; set; }
    public bool ArabicISO { get; set; }
    public bool ArabicWindows { get; set; }
    public bool BalticISO4 { get; set; }
    public bool BalticISO13 { get; set; }
    public bool BalticWindows { get; set; }
    public bool CentralEuropean { get; set; }
    public bool ChineseSimplified { get; set; }
    public bool ChineseTraditional { get; set; }
    public bool CyrillicISO { get; set; }
    public bool CyrillicWindows { get; set; }
    public bool EasternEuropean { get; set; }
    public bool GreekISO { get; set; }
    public bool GreekWindows { get; set; }
    public bool HebrewISO { get; set; }
    public bool HebrewWindows { get; set; }
    public bool Japanese { get; set; }
    public bool Korean { get; set; }
    public bool SouthEuropean { get; set; }
    public bool Thai { get; set; }
    public bool TurkishISO { get; set; }
    public bool TurkishWindows { get; set; }
    public bool Vietnamese { get; set; }
    public bool WesternEuropeanISO1 { get; set; }
    public bool WesternEuropeanISO15 { get; set; }
    public bool WesternEuropeanWindows { get; set; }
}

internal class HTMLColorsHelpOptions : IHTMLColorsOptions
{
    public string TextColor { get; set; }
    public string BackgroundColor { get; set; }
    public string AuthorColor { get; set; }
    public string RepliedAuthorColor { get; set; }
    public string OtherAuthorColor { get; set; }
    public string SmallElementsColor { get; set; }
    public string FavoritedBackgroundColor { get; set; }
    public string HeaderBackgroundColor { get; set; }
    public string HighlightUploaderTextColor { get; set; }
    public string HighlightUploaderBackgroundColor { get; set; }
    public string HighlightUploaderBorderColor { get; set; }
    public string HighlightAuthorTextColor { get; set; }
    public string HighlightAuthorBackgroundColor { get; set; }
    public string HighlightAuthorBorderColor { get; set; }
    public string HighlightTextTextColor { get; set; }
    public string HighlightTextBackgroundColor { get; set; }
    public string HighlightTextBorderColor { get; set; }
    public string LinkColor { get; set; }
    public string CopyTextSuccessColor { get; set; }
    public string CopyTextFailureColor { get; set; }
}

internal class ConfigurationHelpOptions : IConfigurationOptions
{
    public IEnumerable<string> ConfigLocations { get; set; }
    public bool IgnoreConfig { get; set; }
    public bool StopConfig { get; set; }
}

internal class VerbosityHelpOptions : IVerbosityOptions
{
    public bool Verbose { get; set; }
    public bool Quiet { get; set; }
    public bool YtDlpQuiet { get; set; }
    public bool NoWriteComments { get; set; }
}

internal class GeneralHelpOptions : IGeneralOptions
{
    public bool Beep { get; set; }
    public bool BeepOnErrors { get; set; }
    public bool PauseOnErrors { get; set; }
    public bool Help { get; set; }
    public bool Version { get; set; }
}

#if DEBUG
internal class DebugHelpOptions : IDebugOptions
{
    public bool FilterAndHighlightCommentsWithEmoji { get; set; }
    public bool HighlightAllComments { get; set; }
}
#endif

#endregion

#region Examples

internal abstract class BaseExamples
{
    protected const string JSON_FILE = @"C:\Path\To\Comments File.json";
    protected const string URL = "https://www.youtube.com/watch?v=dtCwxFTMMDg";
    protected const string YT_DLP = @"C:\Path\To\yt-dlp.exe";
    protected const string DENO = @"C:\Path\To\deno.exe";
    protected const string DOWNLOAD_PATH = @"C:\Path\To\Downloads Folder";
    protected const string ALICE = "Alice";
    protected const string BOB = "Bob";
    protected const string PHRASE1 = "Alice and Bob";
    protected const string PHRASE2 = "are entangled";
    protected const string NEW_LINE = "\n   ";
}

internal class ConvertExamples : BaseExamples
{
    [Usage(ApplicationAlias = nameof(YouTubeCommentsToFile))]
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example(
                "1. Convert to text file",
                new ConvertOptions
                {
                    JsonFile = JSON_FILE
                }
            );

            yield return new Example(
                "2. Convert to HTML file",
                new ConvertOptions
                {
                    JsonFile = JSON_FILE,
                    ToHTML = true
                }
            );

            yield return new Example(
                "3. Convert to more robust HTML file. By specifying the URL, yt-dlp will download video information - title, uploader, upload date, description",
                new ConvertOptions
                {
                    JsonFile = JSON_FILE,
                    URL = URL,
                    YtDlp = YT_DLP,
                    Deno = DENO,
                    ToHTML = true,
                    ShowCommentNavigationLinks = true,
                    ShowCopyLinks = true,
                    HighlightUploader = true
                }
            );

            yield return new Example(
                "4. Highlight uploader, authors and texts. If a comment or reply matches any of the specified highlight items, the comment is highlighted",
                new ConvertOptions
                {
                    JsonFile = JSON_FILE,
                    HighlightUploader = true,
                    HighlightAuthors = [$"@{ALICE}", BOB],
                    HighlightTexts = [PHRASE1, PHRASE2]
                }
            );

            yield return new Example(
                "5. Filter by uploader, authors and texts. If a comment or reply matches any of the specified filter items, it is shown. Otherwise, it is removed",
                new ConvertOptions
                {
                    JsonFile = JSON_FILE,
                    FilterUploader = true,
                    FilterAuthors = [$"@{ALICE}", BOB],
                    FilterTexts = [PHRASE1, PHRASE2]
                }
            );
        }
    }
}

internal class DownloadExamples : BaseExamples
{
    [Usage(ApplicationAlias = nameof(YouTubeCommentsToFile))]
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example(
                "1. Download to text file",
                new DownloadOptions
                {
                    URL = URL,
                    YtDlp = YT_DLP,
                    Deno = DENO,
                    DownloadPath = DOWNLOAD_PATH
                }
            );

            yield return new Example(
                "2. Download to HTML file",
                new DownloadOptions
                {
                    URL = URL,
                    YtDlp = YT_DLP,
                    Deno = DENO,
                    DownloadPath = DOWNLOAD_PATH,
                    ToHTML = true
                }
            );

            yield return new Example(
                "3. Download to more robust HTML file",
                new DownloadOptions
                {
                    URL = URL,
                    YtDlp = YT_DLP,
                    Deno = DENO,
                    DownloadPath = DOWNLOAD_PATH,
                    ToHTML = true,
                    ShowCommentNavigationLinks = true,
                    ShowCopyLinks = true,
                    HighlightUploader = true
                }
            );

            yield return new Example(
                $"4. The '--yt-dlp' option can be omitted if yt-dlp is at the same directory as {nameof(YouTubeCommentsToFile)}",
                new DownloadOptions
                {
                    URL = URL,
                    DownloadPath = DOWNLOAD_PATH
                }
            );

            yield return new Example(
                "5. Download all comments, sort by top comments first",
                new DownloadOptions
                {
                    URL = URL,
                    YtDlp = YT_DLP,
                    Deno = DENO,
                    DownloadPath = DOWNLOAD_PATH,
                    SortTop = true
                }
            );

            yield return new Example(
                "6. Download up to 10,000 top-level comments and replies, at most 100 replies for each top-level comment, sort by new comments first (default)",
                new DownloadOptions
                {
                    URL = URL,
                    YtDlp = YT_DLP,
                    Deno = DENO,
                    DownloadPath = DOWNLOAD_PATH,
                    MaxComments = 10000,
                    MaxRepliesPerThread = 100
                }
            );

            yield return new Example(
                "7. Download all top-level comments, up to 1000 replies, at most 10 replies for each top-level comment, sort by new comments first",
                new DownloadOptions
                {
                    URL = URL,
                    YtDlp = YT_DLP,
                    Deno = DENO,
                    DownloadPath = DOWNLOAD_PATH,
                    MaxReplies = 1000,
                    MaxRepliesPerThread = 10,
                    SortNew = true
                }
            );

            yield return new Example(
                "8. Download up to 1000 top-level comments, no replies, sort by top comments first",
                new DownloadOptions
                {
                    URL = URL,
                    YtDlp = YT_DLP,
                    Deno = DENO,
                    DownloadPath = DOWNLOAD_PATH,
                    MaxParents = 1000,
                    MaxReplies = 0,
                    SortTop = true
                }
            );

            yield return new Example(
                "9. Download to text file, use configuration file for yt-dlp, use cookies file for restricted video",
                new DownloadOptions
                {
                    URL = URL,
                    YtDlp = YT_DLP,
                    Deno = DENO,
                    DownloadPath = DOWNLOAD_PATH,
                    YtDlpOptions = @"--config-locations ""C:\Path\To\yt-dlp.conf"" --cookies cookies.txt"
                }
            );
        }
    }
}

internal class UploaderExamples : BaseExamples
{
    [Usage(ApplicationAlias = nameof(YouTubeCommentsToFile))]
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example(
                "1. Highlight all comments written by the uploader of the video",
                new DownloadOptions
                {
                    HighlightUploader = true
                }
            );

            yield return new Example(
                "2. Show only conversations that the uploader of the video participated in",
                new DownloadOptions
                {
                    FilterUploader = true
                }
            );
        }
    }
}

internal class AuthorsExamples : BaseExamples
{
    [Usage(ApplicationAlias = nameof(YouTubeCommentsToFile))]
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example(
                $"1. Highlight all comments written by either @{ALICE} or @{BOB}.{NEW_LINE}Search for author names is case sensitive",
                new DownloadOptions
                {
                    HighlightAuthors = [$"@{ALICE}", BOB]
                }
            );

            yield return new Example(
                $"2. Highlight all comments written by either @{ALICE} or @{BOB}.{NEW_LINE}Search for author names is case insensitive",
                new DownloadOptions
                {
                    HighlightAuthorsIgnoreCase = [ALICE.ToLower(), $"@{BOB.ToLower()}"]
                }
            );

            yield return new Example(
                $"3. Show only conversations that either @{ALICE} or @{BOB} participated in.{NEW_LINE}Search for author names is case insensitive",
                new DownloadOptions
                {
                    FilterAuthorsIgnoreCase = [ALICE.ToLower(), BOB.ToLower()]
                }
            );
        }
    }
}

internal class TextsExamples : BaseExamples
{
    [Usage(ApplicationAlias = nameof(YouTubeCommentsToFile))]
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example(
                $@"1. Highlight all comments that contain any of the text phrases ""{PHRASE1}"" and ""{PHRASE2}"".{NEW_LINE}Search for text phrases is case sensitive",
                new DownloadOptions
                {
                    HighlightTexts = [PHRASE1, PHRASE2]
                }
            );

            yield return new Example(
                $@"2. Highlight all comments that contain any of the text phrases ""{PHRASE1}"" and ""{PHRASE2}"".{NEW_LINE}Search for text phrases is case insensitive",
                new DownloadOptions
                {
                    HighlightTextsIgnoreCase = [PHRASE1.ToLower(), PHRASE2.ToLower()]
                }
            );

            yield return new Example(
                $@"3. Show only conversations with comments that contain any of the text phrases ""{PHRASE1}"" and ""{PHRASE2}"".{NEW_LINE}Search for text phrases is case sensitive",
                new DownloadOptions
                {
                    FilterTexts = [PHRASE1, PHRASE2]
                }
            );
        }
    }
}

#endregion
