namespace YouTubeCommentsToFile.Library;

public class Settings
{
    public bool Download { get; set; }

    // Convert
    public string[] JsonFiles { get; set; }
    public string URL { get; set; }

    // Download
    public string[] URLs { get; set; }
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

    // Shared
    public string YtDlp { get; set; }
    public string Deno { get; set; }
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
    public List<SearchItem> SearchItems = [];

    // Encoding
    public int? EncodingCodePage { get; set; }
    [CodePage(65001)] public bool UTF8 { get; set; }
    [CodePage(28596)] public bool ArabicISO { get; set; }
    [CodePage(1256)] public bool ArabicWindows { get; set; }
    [CodePage(28594)] public bool BalticISO4 { get; set; }
    [CodePage(28603)] public bool BalticISO13 { get; set; }
    [CodePage(1257)] public bool BalticWindows { get; set; }
    [CodePage(1250)] public bool CentralEuropean { get; set; }
    [CodePage(936)] public bool ChineseSimplified { get; set; }
    [CodePage(950)] public bool ChineseTraditional { get; set; }
    [CodePage(28595)] public bool CyrillicISO { get; set; }
    [CodePage(1251)] public bool CyrillicWindows { get; set; }
    [CodePage(28592)] public bool EasternEuropean { get; set; }
    [CodePage(28597)] public bool GreekISO { get; set; }
    [CodePage(1253)] public bool GreekWindows { get; set; }
    [CodePage(28598)] public bool HebrewISO { get; set; }
    [CodePage(1255)] public bool HebrewWindows { get; set; }
    [CodePage(932)] public bool Japanese { get; set; }
    [CodePage(949)] public bool Korean { get; set; }
    [CodePage(28593)] public bool SouthEuropean { get; set; }
    [CodePage(874)] public bool Thai { get; set; }
    [CodePage(28599)] public bool TurkishISO { get; set; }
    [CodePage(1254)] public bool TurkishWindows { get; set; }
    [CodePage(1258)] public bool Vietnamese { get; set; }
    [CodePage(28591)] public bool WesternEuropeanISO1 { get; set; }
    [CodePage(28605)] public bool WesternEuropeanISO15 { get; set; }
    [CodePage(1252)] public bool WesternEuropeanWindows { get; set; }

    // HTML Colors
    public Color? TextColor { get; set; }
    public Color? BackgroundColor { get; set; }
    public Color? AuthorColor { get; set; }
    public Color? RepliedAuthorColor { get; set; }
    public Color? OtherAuthorColor { get; set; }
    public Color? SmallElementsColor { get; set; }
    public Color? FavoritedBackgroundColor { get; set; }
    public Color? HeaderBackgroundColor { get; set; }
    public Color? HighlightUploaderTextColor { get; set; }
    public Color? HighlightUploaderBackgroundColor { get; set; }
    public Color? HighlightUploaderBorderColor { get; set; }
    public Color? HighlightAuthorTextColor { get; set; }
    public Color? HighlightAuthorBackgroundColor { get; set; }
    public Color? HighlightAuthorBorderColor { get; set; }
    public Color? HighlightTextTextColor { get; set; }
    public Color? HighlightTextBackgroundColor { get; set; }
    public Color? HighlightTextBorderColor { get; set; }
    public Color? LinkColor { get; set; }
    public Color? CopyTextSuccessColor { get; set; }
    public Color? CopyTextFailureColor { get; set; }

    // General
    public uint? SleepInterval { get; set; }
    public uint? MinSleepInterval { get; set; }
    public uint? MaxSleepInterval { get; set; }

#if DEBUG
    // Debug
    public bool HighlightCommentsWithEmoji { get; set; }
    public bool FilterCommentsWithEmoji { get; set; }
    public bool HighlightAndFilterCommentsWithEmoji { get; set; }
    public bool HighlightAllComments { get; set; }
#endif
}