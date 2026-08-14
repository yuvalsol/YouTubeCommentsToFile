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
        //    URLsValue = [""],
        //    YtDlp = @"",
        //    DownloadPath = @"",
        //    ToHTMLAndText = true,
        //    SortTop = true,
        //    HighlightUploader = true,
        //    ShowCommentNavigationLinks = true,
        //    DeleteJsonFile = false,
        //    Verbose = true,
        //    YtDlpQuiet = false,
        //    BeepOnErrors = true
        //});

        //return RunDebug(new ConvertOptions
        //{
        //    JsonFilesValue = [@""],
        //    URL = "",
        //    YtDlp = @"",
        //    ToHTMLAndText = true,
        //    HighlightUploader = true,
        //    ShowCommentNavigationLinks = true,
        //    DeleteJsonFile = false,
        //    Verbose = true,
        //    YtDlpQuiet = false,
        //    BeepOnErrors = true
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
        return RunDebug(options.ToArgs());
    }

    private static int RunDebug(string[] args)
    {
        return new Program(args).Run();
    }

    private static int PrintArgs<TOptions>(TOptions options, bool singleLine = false)
        where TOptions : SharedOptions, IOperationOptions
    {
        if (singleLine)
        {
            Console.WriteLine(options.ToString());
        }
        else
        {
            foreach (var arg in options.ToVerboseArgs())
                Console.WriteLine(arg);
        }

        return 0;
    }

    private static int PrintArgs(string[] args, bool singleLine = false)
    {
        if (singleLine)
        {
            Console.WriteLine(string.Join(' ', args));
        }
        else
        {
            foreach (var arg in args)
                Console.WriteLine(arg);
        }

        return 0;
    }

    private static int GetVideoInfo(string url, string ytDlp = null, Encoding encoding = null, bool updateYtDlp = false)
    {
        var videoInfo = CommentsWriter.GetVideoInfo(url, ytDlp, encoding, updateYtDlp);
        if (videoInfo != null)
        {
            PrintVideoInfoProperty(nameof(videoInfo.Title), videoInfo.Title);
            PrintVideoInfoProperty(nameof(videoInfo.Uploader), videoInfo.Uploader);
            PrintVideoInfoProperty(nameof(videoInfo.UploaderID), videoInfo.UploaderID);
            PrintVideoInfoProperty(nameof(videoInfo.UploaderURL), videoInfo.UploaderURL);
            PrintVideoInfoProperty(nameof(videoInfo.UploadDate), videoInfo.GetUploadDate()?.ToString("d") ?? videoInfo.UploadDate);
            PrintVideoInfoProperty(nameof(videoInfo.Description), videoInfo.Description, true);
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

    private static void PrintVideoInfoProperty(string property, string value, bool valueNewLine = false)
    {
        Console.ForegroundColor = PROPERTY_COLOR;
        Console.Write(property.ToWords());
        Console.ForegroundColor = FOREGROUND_COLOR;
        if (valueNewLine)
            Console.WriteLine(":");
        else
            Console.Write(": ");
        Console.WriteLine(value);
    }
}
#endif
