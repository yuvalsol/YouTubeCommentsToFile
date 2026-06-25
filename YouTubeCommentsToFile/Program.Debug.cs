#if DEBUG
using CommandLine;
using YouTubeCommentsToFile.Library;

namespace YouTubeCommentsToFile;

internal partial class Program
{
    private static void Debug()
    {
        //WriteHelp();

        //RunDebug(new DownloadOptions
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

        //RunDebug(new ConvertOptions
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

    private static void RunDebug<T>(T options)
    {
        Run(GetParser().FormatCommandLineArgs(options));
        Console.WriteLine();
    }

    private static void GetVideoInfo(string url, string ytDlp = null, int codePage = -1, bool updateYtDlp = false)
    {
        var videoInfo = CommentsWriter.GetVideoInfo(url, ytDlp, codePage, updateYtDlp);
        if (videoInfo != null)
            Console.WriteLine(videoInfo);
        else
            WriteErrorLine("Failed to get video info.");
    }
}
#endif
