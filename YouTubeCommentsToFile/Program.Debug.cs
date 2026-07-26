#if DEBUG
using YouTubeCommentsToFile.Library;

namespace YouTubeCommentsToFile;

internal partial class Program
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
        //    Verbose = true,
        //    BeepErrors = true
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
        //    Verbose = true,
        //    BeepErrors = true
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
        return new Program(args).Run();
    }

    private static int PrintArgs<TOptions>(TOptions options, bool singleLine = false)
        where TOptions : SharedOptions, IOperationOptions
    {
        return PrintArgs(options.ToArgs(GetParser()), singleLine);
    }

    private static int PrintArgs(string[] args, bool singleLine = false)
    {
        if (singleLine)
        {
            Console.WriteLine(string.Join(' ', args.RearrangeArgs().OrderByPrecedence()));
        }
        else
        {
            foreach (var arg in args.RearrangeArgs().OrderByPrecedence())
                Console.WriteLine(arg);
        }

        return 0;
    }

    private static int GetVideoInfo(string url, string ytDlp = null, int codePage = -1, bool updateYtDlp = false)
    {
        var videoInfo = CommentsWriter.GetVideoInfo(url, ytDlp, codePage, updateYtDlp);
        if (videoInfo != null)
        {
            Console.WriteLine(videoInfo.ToString());
            return 0;
        }
        else
        {
            Console.ForegroundColor = ERROR_COLOR;
            Console.WriteLine("Failed to get video info.");
            Console.ForegroundColor = FOREGROUND_COLOR;
            return -1;
        }
    }
}
#endif
