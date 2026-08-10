using YouTubeCommentsToFile.Library;

namespace YouTubeCommentsToFile;

internal partial class Program
{
    private bool download;
    private readonly object lockObj = new();
    private bool isPrintedJsonFile;
    private bool isPrintedVideoTitle;
    private readonly List<StringBuilder> warningMessages = [];
    private int lostComments;
    private readonly bool withoutDebuggingInfo =
#if RELEASE
            true;
#elif DEBUG
        false;
#endif

    private void WriteCommentsToFile()
    {
        var writer = new CommentsWriter(operationOptions.ToSettings());

        writer.Start += OnStart;
        writer.StartConvert += OnStartConvert;
        writer.StartDownload += OnStartDownload;
        writer.StartDownloadVideoInfo += OnStartDownloadVideoInfo;
        writer.FinishDownloadVideoInfo += OnFinishDownloadVideoInfo;
        writer.StartDownloadComments += OnStartDownloadComments;
        if (operationOptions.YtDlpQuiet == false)
            writer.DownloadCommentsProgressAsync += OnDownloadCommentsProgressAsync;
        writer.FinishDownloadComments += OnFinishDownloadComments;
        writer.StartLoadJsonFile += OnStartLoadJsonFile;
        writer.FinishLoadJsonFile += OnFinishLoadJsonFile;
        writer.StartProcessComments += OnStartProcessComments;
        writer.FinishProcessComments += OnFinishProcessComments;
        writer.StartWriteComments += OnStartWriteComments;
        writer.WriteCommentError += OnWriteCommentError;
        writer.FinishWriteComments += OnFinishWriteComments;
        writer.StartDeleteJsonFile += OnStartDeleteJsonFile;
        writer.FinishDeleteJsonFile += OnFinishDeleteJsonFile;
        writer.FinishConvert += OnFinishConvert;
        writer.FinishDownload += OnFinishDownload;
        writer.Finish += OnFinish;

        writer.WriteCommentsToFile();
    }

    private void OnStart(object sender, StartEventArgs e)
    {
    }

    private void OnStartConvert(object sender, StartConvertEventArgs e)
    {
        download = false;
        WriteLine($"{(e.Total > 1 ? $"{e.Count}/{e.Total} " : null)}Convert {Path.GetFileName(e.JsonFile)}");
        if (string.IsNullOrEmpty(e.URL) == false)
            WriteLine($"URL: {e.URL}");
        WriteVerboseProperty("Encoding", e.EncodingName);
    }

    private void OnStartDownload(object sender, StartDownloadEventArgs e)
    {
        download = true;
        WriteLine($"{(e.Total > 1 ? $"{e.Count}/{e.Total} " : null)}Download {e.URL}");
        WriteVerboseProperty("Encoding", e.EncodingName);
    }

    private void OnStartDownloadVideoInfo(object sender, StartDownloadVideoInfoEventArgs e)
    {
        Write("Downloading video information...");
    }

    private void OnFinishDownloadVideoInfo(object sender, FinishDownloadVideoInfoEventArgs e)
    {
        if (e.Error == null)
        {
            WriteLine(" Done.");
            WriteVerboseProperty("Title", e.VideoInfo.Title);
            WriteVerboseProperty("Uploader", e.VideoInfo.Uploader);
            WriteVerboseProperty("Uploader ID", e.VideoInfo.UploaderID);
            WriteVerboseProperty("Upload Date", e.VideoInfo.GetUploadDate()?.ToString("d"));
            isPrintedVideoTitle = true;
        }
        else
        {
            WriteWarningLine(" Failed.");
            if (operationOptions.Verbose)
                WriteWarningLine(e.Error.GetExceptionErrorMessage(withoutDebuggingInfo: withoutDebuggingInfo));
            else
                WriteWarningLine(e.Error.Message);
            finishedWithErrors = true;
        }
    }

    private void OnStartDownloadComments(object sender, StartDownloadCommentsEventArgs e)
    {
        WriteVerboseProperty("URL", e.URL);
        WriteVerboseProperty("yt-dlp", e.YtDlpCommandLine);
        if (operationOptions.YtDlpQuiet)
            Write("Downloading comments...");
        else
            WriteLine("Downloading comments...");
    }

    private void OnDownloadCommentsProgressAsync(object sender, DownloadCommentsProgressEventArgs e)
    {
        lock (lockObj)
        {
            if (e.IsError)
            {
                if (string.IsNullOrEmpty(e.Line) == false && e.Line.StartsWith("WARNING"))
                    WriteLine(e.Line, ConsoleColor.DarkYellow);
                else
                    WriteErrorLine(e.Line);
            }
            else
            {
                WriteLine(e.Line);
            }
        }
    }

    private void OnFinishDownloadComments(object sender, FinishDownloadCommentsEventArgs e)
    {
        lock (lockObj)
        {
            if (e.Error == null)
            {
                if (operationOptions.YtDlpQuiet)
                    WriteLine(" Done.");
                WriteVerboseProperty("JSON File", e.JsonFile);
                isPrintedJsonFile = true;
            }
            else
            {
                if (operationOptions.YtDlpQuiet)
                    WriteErrorLine(" Failed.");
                else
                    WriteErrorLine("Failed to download comments JSON file.");
                finishedWithErrors = true;
            }
        }
    }

    private void OnStartLoadJsonFile(object sender, StartLoadJsonFileEventArgs e)
    {
        if (isPrintedVideoTitle == false && download == false)
        {
            WriteVerboseProperty("Title", e.VideoInfo.Title);
            WriteVerboseProperty("Uploader", e.VideoInfo.Uploader);
            WriteVerboseProperty("Uploader ID", e.VideoInfo.UploaderID);
            WriteVerboseProperty("Upload Date", e.VideoInfo.GetUploadDate()?.ToString("d"));
        }

        Write("Loading JSON file...");
    }

    private void OnFinishLoadJsonFile(object sender, FinishLoadJsonFileEventArgs e)
    {
        if (e.Error == null)
        {
            WriteLine(" Done.");
            if (isPrintedJsonFile == false)
                WriteVerboseProperty("JSON File", e.JsonFile);
        }
        else
        {
            WriteErrorLine(" Failed.");
            if (isPrintedJsonFile == false)
                WriteErrorLine($"JSON File: {e.JsonFile}");
            finishedWithErrors = true;
        }
    }

    private void OnStartProcessComments(object sender, StartProcessCommentsEventArgs e)
    {
        Write("Processing comments...");
    }

    private void OnFinishProcessComments(object sender, FinishProcessCommentsEventArgs e)
    {
        if (e.Error == null)
        {
            WriteLine(" Done.");

            if (e.CommentsCount == e.TotalComments)
            {
                WriteVerboseProperty("Comments", e.CommentsCount.ToString("N0"));
            }
            else
            {
                WriteVerboseProperty("Comments", e.CommentsCount.ToString("N0"));
                WriteVerboseProperty("Discarded Comments", (e.TotalComments - e.CommentsCount).ToString("N0"));
                WriteVerboseProperty("Total Comments", e.TotalComments.ToString("N0"));
            }
        }
        else
        {
            WriteErrorLine(" Failed.");
            finishedWithErrors = true;
        }
    }

    private void OnStartWriteComments(object sender, StartWriteCommentsEventArgs e)
    {
        Write($"Writing to {(e.WriteToHTML ? "HTML" : "text")} file...");
    }

    private void OnWriteCommentError(object sender, WriteCommentErrorEventArgs e)
    {
        warningMessages.Add(e.Error.GetExceptionErrorMessage($"{(string.IsNullOrEmpty(e.Id) ? null : $"COMMENT ID: {e.Id}{Environment.NewLine}")}{(string.IsNullOrEmpty(e.Parent) ? null : $"PARENT ID: {e.Parent}{Environment.NewLine}")}COMMENT:{Environment.NewLine}{e.Comment}", withoutDebuggingInfo));
        lostComments += e.LostComments;
        finishedWithErrors = true;
    }

    private void OnFinishWriteComments(object sender, FinishWriteCommentsEventArgs e)
    {
        if (e.Error == null && lostComments == 0)
        {
            WriteLine(" Done.");
            WriteVerboseProperty($"{(e.WriteToHTML ? "HTML" : "Text")} File", e.CommentsFile);
        }
        else
        {
            WriteLine();

            if (e.Error != null)
            {
                WriteErrorLine($"Failed to write {(e.WriteToHTML ? "HTML" : "text")} file.");
                WriteErrorLine($"{(e.WriteToHTML ? "HTML" : "Text")} File: {e.CommentsFile}");
            }

            if (lostComments > 0)
            {
                var seperator = new string('-', 17);
                WriteWarningLine($"Lost Comments: {lostComments}");
                WriteWarningLine(seperator);
                WriteWarningLine(string.Join($"{Environment.NewLine}{seperator}{Environment.NewLine}", warningMessages));
            }

            finishedWithErrors = true;
        }
    }

    private void OnStartDeleteJsonFile(object sender, StartDeleteJsonFileEventArgs e)
    {
        Write("Deleting JSON file...");
    }

    private void OnFinishDeleteJsonFile(object sender, FinishDeleteJsonFileEventArgs e)
    {
        if (e.Error == null)
        {
            WriteLine(" Done.");
        }
        else
        {
            WriteErrorLine(" Failed.");
            finishedWithErrors = true;
        }
    }

    private void OnFinishConvert(object sender, FinishConvertEventArgs e)
    {
        OnFinishOperation("convert", e.ProcessTime, e.Total, e.Error);
    }

    private void OnFinishDownload(object sender, FinishDownloadEventArgs e)
    {
        OnFinishOperation("download", e.ProcessTime, e.Total, e.Error);
    }

    private void OnFinishOperation(string operation, TimeSpan processTime, int total, Exception error)
    {
        if (error == null)
        {
            if (lostComments > 0)
                WriteLine($"Finished {operation}ing comments with warnings.");
        }
        else
        {
            WriteErrorLine(error.GetExceptionErrorMessage("Failed to write comments file.", withoutDebuggingInfo));
            finishedWithErrors = true;
        }

        WriteVerboseProcessTime($"{operation.PrettifyName()} Process Time", processTime);

        if (total > 1)
        {
            Console.WriteLine();
            ResetCommentsWritingFields();
        }
    }

    private void ResetCommentsWritingFields()
    {
        isPrintedJsonFile = false;
        isPrintedVideoTitle = false;
        warningMessages.Clear();
        lostComments = 0;
    }

    private void OnFinish(object sender, FinishEventArgs e)
    {
        WriteVerboseProcessTime("Total Process Time", e.ProcessTime);
    }

    private void WriteVerboseProcessTime(string property, TimeSpan processTime)
    {
        WriteVerboseProperty(
            property,
            processTime.ToString(processTime.Days > 0 ? "d':'hh':'mm':'ss'.'fff" : processTime.Hours > 0 ? "h':'mm':'ss'.'fff" : "m':'ss'.'fff")
        );
    }

    private void WriteVerboseProperty(string property, string value)
    {
        if (operationOptions.Verbose && string.IsNullOrEmpty(value) == false)
            WriteMessages(GetPropertyMessages(property, [value]), singleLine: true);
    }
}
