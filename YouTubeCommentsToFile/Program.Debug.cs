#if DEBUG
using YouTubeCommentsToFile.Library;

namespace YouTubeCommentsToFile;

internal static partial class Program
{
    private static int Debug()
    {
        return Version();
        //return Help();

        //return RunDebug(new DownloadOptions
        //{
        //    URL = "",
        //    YtDlp = @"",
        //    DownloadPath = @"",
        //    ToHTML = true,
        //    SortTop = true,
        //    HighlightUploader = true,
        //    ShowCommentNavigationLinks = true,
        //    DeleteJsonFile = false,
        //    Verbose = true
        //});

        //return RunDebug(new ConvertOptions
        //{
        //    JsonFile = @"",
        //    URL = "",
        //    YtDlp = @"",
        //    ToHTML = true,
        //    HighlightUploader = true,
        //    ShowCommentNavigationLinks = true,
        //    DeleteJsonFile = false,
        //    Verbose = true
        //});

        //return GetVideoInfo("", @"");
    }

    private static int Version()
    {
        return RunDebug(new ConvertOptions() { Version = true });
    }

    private static int Help()
    {
        return RunDebug(new ConvertOptions() { Help = true });
    }

    private static int RunDebug<TOptions>(TOptions options)
        where TOptions : SharedOptions, IOperationOptions
    {
        return RunDebug(options.ToArgs(GetParser()));
    }

    private static int RunDebug(string[] args)
    {
        return Run(args);
    }

    private static int GetVideoInfo(string url, string ytDlp = null, int codePage = -1, bool updateYtDlp = false)
    {
        var videoInfo = CommentsWriter.GetVideoInfo(url, ytDlp, codePage, updateYtDlp);
        if (videoInfo != null)
        {
            WriteLine(videoInfo.ToString());
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
