using CommandLine;
using CommandLine.Text;
using YouTubeCommentsToFile.Library;

namespace YouTubeCommentsToFile;

#region Interfaces

internal interface IConvertOptions
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

internal interface IDownloadOptions
{
    [Option("url", HelpText = "Video URL to download its comments.")]
    [OptionPrecedence(2)]
    string URL { get; set; }

    [Option("yt-dlp", HelpText = $"Path to yt-dlp. If not set, search for yt-dlp in {nameof(YouTubeCommentsToFile)} directory. If not found, check if yt-dlp is accessible in the current environment.")]
    [OptionPrecedence(3)]
    string YtDlp { get; set; }

    [Option("deno", HelpText = $"Path to Deno. If not set, search for Deno in {nameof(YouTubeCommentsToFile)} directory. If not found, check if Deno is accessible in the current environment. If Deno is at the same location as yt-dlp, there is no need to specify a path to Deno.")]
    [OptionPrecedence(4)]
    string Deno { get; set; }

    [Option("download-path", HelpText = $"Path to download and to convert comments. If not set, download path is set to {nameof(YouTubeCommentsToFile)} directory.")]
    [OptionPrecedence(5)]
    string DownloadPath { get; set; }

    [Option("max-comments", HelpText = "Maximum number of top-level comments and replies to extract. If not set, extracts all.")]
    [OptionPrecedence(11, 1)]
    uint? MaxComments { get; set; }

    [Option("max-parents", HelpText = "Maximum number of top-level comments to extract. If not set, extracts all.")]
    [OptionPrecedence(11, 2)]
    uint? MaxParents { get; set; }

    [Option("max-replies", HelpText = "Maximum number of replies to extract. If not set, extracts all.")]
    [OptionPrecedence(11, 3)]
    uint? MaxReplies { get; set; }

    [Option("max-replies-per-thread", HelpText = "Maximum number of replies per top-level comment to extract. If not set, extracts all.")]
    [OptionPrecedence(11, 4)]
    uint? MaxRepliesPerThread { get; set; }

    [Option("sort-new", HelpText = "Sort comments by new comments first. This is the default sorting of yt-dlp.")]
    [OptionPrecedence(12, 1)]
    bool SortNew { get; set; }

    [Option("sort-top", HelpText = "Sort comments by top comments first.")]
    [OptionPrecedence(12, 2)]
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

internal interface ISharedOptions
{
    [Option("to-html", HelpText = "Whether to write comments to HTML file. When both '--to-html' and '--to-html-and-text' are disabled, writes only to text file.")]
    [OptionPrecedence(21, 1)]
    bool ToHTML { get; set; }

    [Option("to-html-and-text", HelpText = "Whether to write comments to HTML file and to text file.")]
    [OptionPrecedence(21, 2)]
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

internal interface IEncodingOptions
{
    [Option("encoding-code-page", HelpText = "The code page of the video page encoding.")]
    [OptionPrecedence(31, 1)]
    int? EncodingCodePage { get; set; }

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

internal interface IUploaderOptions
{
    [Option('u', "uh", HelpText = "Highlight uploader. Search for comments and replies written by the uploader of the video. If found, highlight the comment or reply.")]
    [OptionPrecedence(41, 1)]
    bool HighlightUploader { get; set; }

    [Option("uf", HelpText = "Filter uploader. Search for comments and replies written by the uploader of the video. If found, keep the conversation (comment and its replies). Otherwise, remove the conversation, if it didn't match any other filter items.")]
    [OptionPrecedence(41, 2)]
    bool FilterUploader { get; set; }

    [Option("uhf", HelpText = "Highlight and filter uploader.")]
    bool HighlightAndFilterUploader { get; set; }
}

internal interface IAuthorsOptions
{
    [Option("ah", HelpText = "Highlight authors. Search for comments and replies written by any of the specified authors. If found, highlight the comment or reply.")]
    [OptionPrecedence(51, 1)]
    IEnumerable<string> HighlightAuthors { get; set; }

    [Option("af", HelpText = "Filter authors. Search for comments and replies written by any of the specified authors. If found, keep the conversation (comment and its replies). Otherwise, remove the conversation, if it didn't match any other filter items.")]
    [OptionPrecedence(51, 2)]
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

internal interface ITextsOptions
{
    [Option("th", HelpText = "Highlight texts. Search for any of the specified texts in the comment or reply. If found, highlight the comment or reply.")]
    [OptionPrecedence(61, 1)]
    IEnumerable<string> HighlightTexts { get; set; }

    [Option("tf", HelpText = "Filter texts. Search for any of the specified texts in the comment or reply. If found, keep the conversation (comment and its replies). Otherwise, remove the conversation, if it didn't match any other filter items.")]
    [OptionPrecedence(61, 2)]
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

#endregion

#region Options

[Verb("convert", isDefault: true, HelpText = "Converts comments JSON file to text file or HTML file.")]
internal class ConvertOptions : SharedOptions, IConvertOptions
{
    public string JsonFile { get; set; }
    public string URL { get; set; }
    public string YtDlp { get; set; }
    public string Deno { get; set; }

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
}

[Verb("download", HelpText = "Downloads comments to text file or HTML file.")]
internal class DownloadOptions : SharedOptions, IDownloadOptions
{
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
}

internal abstract class SharedOptions : ISharedOptions, IEncodingOptions, IUploaderOptions, IAuthorsOptions, ITextsOptions
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
    public int? EncodingCodePage { get; set; }
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
    public bool HighlightUploader { get; set; }
    public bool FilterUploader { get; set; }
    public bool HighlightAndFilterUploader { get; set; }
    public IEnumerable<string> HighlightAuthors { get; set; }
    public IEnumerable<string> FilterAuthors { get; set; }
    public IEnumerable<string> HighlightAndFilterAuthors { get; set; }
    public IEnumerable<string> HighlightAuthorsIgnoreCase { get; set; }
    public IEnumerable<string> FilterAuthorsIgnoreCase { get; set; }
    public IEnumerable<string> HighlightAndFilterAuthorsIgnoreCase { get; set; }
    public IEnumerable<string> HighlightTexts { get; set; }
    public IEnumerable<string> FilterTexts { get; set; }
    public IEnumerable<string> HighlightAndFilterTexts { get; set; }
    public IEnumerable<string> HighlightTextsIgnoreCase { get; set; }
    public IEnumerable<string> FilterTextsIgnoreCase { get; set; }
    public IEnumerable<string> HighlightAndFilterTextsIgnoreCase { get; set; }

    public virtual Settings ToSettings()
    {
        var settings = new Settings
        {
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
            EncodingCodePage = EncodingCodePage,
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
            WesternEuropeanWindows = WesternEuropeanWindows
        };

        if (HighlightAndFilterUploader || (HighlightUploader && FilterUploader))
            settings.SearchItems.Add(new SearchUploader(highlight: true, filter: true));
        else if (HighlightUploader)
            settings.SearchItems.Add(SearchUploader.HighlightUploader);
        else if (FilterUploader)
            settings.SearchItems.Add(SearchUploader.FilterUploader);

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

    public override string ToString()
    {
        return $"{nameof(YouTubeCommentsToFile)} {Parser.Default.FormatCommandLine(this)}";
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

internal class EncodingHelpOptions : IEncodingOptions
{
    public int? EncodingCodePage { get; set; }
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

#endregion

#region Usage

internal class ConvertUsage
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
                    JsonFile = @"C:\Path\To\Comments File.json"
                }
            );

            yield return new Example(
                "2. Convert to HTML file",
                new ConvertOptions
                {
                    JsonFile = @"C:\Path\To\Comments File.json",
                    ToHTML = true
                }
            );

            yield return new Example(
                "3. Convert to more robust HTML file. By specifying the URL, yt-dlp will download video information - title, uploader, upload date, description",
                new ConvertOptions
                {
                    JsonFile = @"C:\Path\To\Comments File.json",
                    URL = "https://www.youtube.com/watch?v=dtCwxFTMMDg",
                    YtDlp = @"C:\Path\To\yt-dlp.exe",
                    Deno = @"C:\Path\To\deno.exe",
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
                    JsonFile = @"C:\Path\To\Comments File.json",
                    HighlightUploader = true,
                    HighlightAuthors = ["@Alice", "Bob"],
                    HighlightTexts = ["Alice and Bob", "are entangled"]
                }
            );

            yield return new Example(
                "5. Filter by uploader, authors and texts. If a comment or reply matches any of the specified filter items, it is shown. Otherwise, it is removed",
                new ConvertOptions
                {
                    JsonFile = @"C:\Path\To\Comments File.json",
                    FilterUploader = true,
                    FilterAuthors = ["@Alice", "Bob"],
                    FilterTexts = ["Alice and Bob", "are entangled"]
                }
            );
        }
    }
}

internal class DownloadUsage
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
                    URL = "https://www.youtube.com/watch?v=dtCwxFTMMDg",
                    YtDlp = @"C:\Path\To\yt-dlp.exe",
                    Deno = @"C:\Path\To\deno.exe",
                    DownloadPath = @"C:\Path\To\Downloads Folder"
                }
            );

            yield return new Example(
                "2. Download to HTML file",
                new DownloadOptions
                {
                    URL = "https://www.youtube.com/watch?v=dtCwxFTMMDg",
                    YtDlp = @"C:\Path\To\yt-dlp.exe",
                    Deno = @"C:\Path\To\deno.exe",
                    DownloadPath = @"C:\Path\To\Downloads Folder",
                    ToHTML = true
                }
            );

            yield return new Example(
                "3. Download to more robust HTML file",
                new DownloadOptions
                {
                    URL = "https://www.youtube.com/watch?v=dtCwxFTMMDg",
                    YtDlp = @"C:\Path\To\yt-dlp.exe",
                    Deno = @"C:\Path\To\deno.exe",
                    DownloadPath = @"C:\Path\To\Downloads Folder",
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
                    URL = "https://www.youtube.com/watch?v=dtCwxFTMMDg",
                    DownloadPath = @"C:\Path\To\Downloads Folder"
                }
            );

            yield return new Example(
                "5. Download all comments, sort by top comments first",
                new DownloadOptions
                {
                    URL = "https://www.youtube.com/watch?v=dtCwxFTMMDg",
                    YtDlp = @"C:\Path\To\yt-dlp.exe",
                    Deno = @"C:\Path\To\deno.exe",
                    DownloadPath = @"C:\Path\To\Downloads Folder",
                    SortTop = true
                }
            );

            yield return new Example(
                "6. Download up to 10,000 top-level comments and replies, at most 100 replies for each top-level comment, sort by new comments first (default)",
                new DownloadOptions
                {
                    URL = "https://www.youtube.com/watch?v=dtCwxFTMMDg",
                    YtDlp = @"C:\Path\To\yt-dlp.exe",
                    Deno = @"C:\Path\To\deno.exe",
                    DownloadPath = @"C:\Path\To\Downloads Folder",
                    MaxComments = 10000,
                    MaxRepliesPerThread = 100
                }
            );

            yield return new Example(
                "7. Download all top-level comments, up to 1000 replies, at most 10 replies for each top-level comment, sort by new comments first",
                new DownloadOptions
                {
                    URL = "https://www.youtube.com/watch?v=dtCwxFTMMDg",
                    YtDlp = @"C:\Path\To\yt-dlp.exe",
                    Deno = @"C:\Path\To\deno.exe",
                    DownloadPath = @"C:\Path\To\Downloads Folder",
                    MaxReplies = 1000,
                    MaxRepliesPerThread = 10,
                    SortNew = true
                }
            );

            yield return new Example(
                "8. Download up to 1000 top-level comments, no replies, sort by top comments first",
                new DownloadOptions
                {
                    URL = "https://www.youtube.com/watch?v=dtCwxFTMMDg",
                    YtDlp = @"C:\Path\To\yt-dlp.exe",
                    Deno = @"C:\Path\To\deno.exe",
                    DownloadPath = @"C:\Path\To\Downloads Folder",
                    MaxParents = 1000,
                    MaxReplies = 0,
                    SortTop = true
                }
            );

            yield return new Example(
                "9. Download to text file, use configuration file for yt-dlp, use cookies file for restricted video",
                new DownloadOptions
                {
                    URL = "https://www.youtube.com/watch?v=dtCwxFTMMDg",
                    YtDlp = @"C:\Path\To\yt-dlp.exe",
                    Deno = @"C:\Path\To\deno.exe",
                    DownloadPath = @"C:\Path\To\Downloads Folder",
                    YtDlpOptions = @"--config-locations ""C:\Path\To\yt-dlp.conf"" --cookies cookies.txt"
                }
            );
        }
    }
}

#endregion
