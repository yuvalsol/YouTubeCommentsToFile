namespace YouTubeCommentsToFile.Library;

public class Settings
{
    public bool Download { get; set; }

    // Convert
    public string JsonFile { get; set; }

    // Download
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
    public string URL { get; set; }
    public string YtDlp { get; set; }
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
    [CodePage(28596)] public bool ArabicISO { get; set; }
    [CodePage(1256)] public bool ArabicWindows { get; set; }
    [CodePage(28594)] public bool BalticISO4 { get; set; }
    [CodePage(28603)] public bool BalticISO13 { get; set; }
    [CodePage(1257)] public bool BalticWindows { get; set; }
    [CodePage(28604)] public bool Celtic { get; set; }
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
    [CodePage(28600)] public bool NorthEuropean { get; set; }
    [CodePage(28606)] public bool SouthEasternEuropean { get; set; }
    [CodePage(28593)] public bool SouthEuropean { get; set; }
    [CodePage(874)] public bool Thai { get; set; }
    [CodePage(28601)] public bool ThaiISO { get; set; }
    [CodePage(28599)] public bool TurkishISO { get; set; }
    [CodePage(1254)] public bool TurkishWindows { get; set; }
    [CodePage(1258)] public bool Vietnamese { get; set; }
    [CodePage(28591)] public bool WesternEuropeanISO1 { get; set; }
    [CodePage(28605)] public bool WesternEuropeanISO15 { get; set; }
    [CodePage(1252)] public bool WesternEuropeanWindows { get; set; }
}