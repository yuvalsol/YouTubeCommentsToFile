#if DEBUG
using YouTubeCommentsToFile.Library;

namespace YouTubeCommentsToFile;

internal static partial class Program
{
    private static int Debug()
    {
        //return RunDebug([
        //    ""
        //    ,""
        //], verbose: false);

        //return RunDebug(new DownloadOptions
        //{
        //    URL = "",
        //    YtDlp = @"",
        //    DownloadPath = @"",
        //    ToHTML = true,
        //    SortTop = true,
        //    HighlightUploader = true,
        //    ShowCommentNavigationLinks = true,
        //    DeleteJsonFile = false
        //}, verbose: false);

        //return RunDebug(new ConvertOptions
        //{
        //    JsonFile = @"",
        //    URL = "",
        //    YtDlp = @"",
        //    ToHTML = true,
        //    HighlightUploader = true,
        //    ShowCommentNavigationLinks = true,
        //    DeleteJsonFile = false
        //}, verbose: false);

        //return GetVideoInfo("", @"");

        //WriteHelp();
        return 0;
    }

    private static int RunDebug<T>(T options, bool verbose = false)
        where T : SharedOptions
    {
        return RunDebug(options.ToArgs(GetParser()), verbose);
    }

    private static int RunDebug(string[] args, bool verbose = false)
    {
        int returnCode = Run(args, verbose);
        Console.WriteLine();
        return returnCode;
    }

    private static int GetVideoInfo(string url, string ytDlp = null, int codePage = -1, bool updateYtDlp = false)
    {
        var videoInfo = CommentsWriter.GetVideoInfo(url, ytDlp, codePage, updateYtDlp);
        if (videoInfo != null)
        {
            Console.WriteLine(videoInfo);
            return 0;
        }
        else
        {
            WriteErrorLine("Failed to get video info.");
            return -1;
        }
    }
}
#endif
