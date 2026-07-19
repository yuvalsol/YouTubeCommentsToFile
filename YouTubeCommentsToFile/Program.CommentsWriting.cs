using YouTubeCommentsToFile.Library;

namespace YouTubeCommentsToFile;

internal static partial class Program
{
    private static void WriteCommentsToFile(IOperationOptions operationOptions)
    {
        var writer = new CommentsWriter(operationOptions.ToSettings());

        bool download = false;

        writer.Start += (sender, e) =>
        {
            download = e.Download;
            WriteLine(GetVersion(), operationOptions);
            WriteLine(e.Download ? $"Download {e.URL}" : $"Convert {Path.GetFileName(e.JsonFile)}", operationOptions);
        };

        writer.StartDownloadComments += (sender, e) =>
        {
            WriteVerboseProperty("URL", [e.URL]);
            WriteVerboseProperty("yt-dlp", [e.YtDlpCommandLine]);
            WriteLine("Downloading comments...", operationOptions);
        };

        object lockObj = new();

        writer.DownloadCommentsProgressAsync += (sender, e) =>
        {
            lock (lockObj)
            {
                if (e.IsError)
                    WriteErrorLine(e.Line, operationOptions);
                else
                    WriteLine(e.Line, operationOptions);
            }
        };

        bool isPrintedJsonFile = false;

        writer.FinishDownloadComments += (sender, e) =>
        {
            lock (lockObj)
            {
                if (e.Error == null)
                {
                    WriteLine("Comments JSON file downloaded successfully.", operationOptions);
                    WriteVerboseProperty("JSON File", [e.JsonFile]);
                    isPrintedJsonFile = true;
                }
                else
                {
                    WriteErrorLine("Failed to download comments JSON file.", operationOptions);
                }
            }
        };

        writer.StartDownloadVideoInfo += (sender, e) =>
        {
            Write("Downloading video information...", operationOptions);
        };

        bool isPrintedVideoTitle = false;
        bool withoutDebuggingInfo =
#if RELEASE
            true;
#elif DEBUG
            false;
#endif

        writer.FinishDownloadVideoInfo += (sender, e) =>
        {
            if (e.Error == null)
            {
                WriteLine(" Done.", operationOptions);

                WriteVerboseProperty("Title", [e.VideoInfo.Title]);
                if (string.IsNullOrEmpty(e.VideoInfo.Uploader) == false)
                    WriteVerboseProperty("Uploader", [e.VideoInfo.Uploader]);
                else if (string.IsNullOrEmpty(e.VideoInfo.UploaderID) == false)
                    WriteVerboseProperty("Uploader", [e.VideoInfo.UploaderID]);
                isPrintedVideoTitle = true;
            }
            else
            {
                WriteLine(operationOptions);
                WriteWarningLine(e.Error.GetExceptionErrorMessage("Failed to download video information.", withoutDebuggingInfo), operationOptions);
            }
        };

        writer.StartLoadJsonFile += (sender, e) =>
        {
            if (isPrintedVideoTitle == false && download == false)
            {
                WriteVerboseProperty("Title", [e.VideoInfo.Title]);
                if (string.IsNullOrEmpty(e.VideoInfo.Uploader) == false)
                    WriteVerboseProperty("Uploader", [e.VideoInfo.Uploader]);
                else if (string.IsNullOrEmpty(e.VideoInfo.UploaderID) == false)
                    WriteVerboseProperty("Uploader", [e.VideoInfo.UploaderID]);
            }

            Write("Loading JSON file...", operationOptions);
        };

        writer.FinishLoadJsonFile += (sender, e) =>
        {
            if (e.Error == null)
            {
                WriteLine(" Done.", operationOptions);
                if (isPrintedJsonFile == false)
                    WriteVerboseProperty("JSON File", [e.JsonFile]);
            }
            else
            {
                WriteLine(operationOptions);
                WriteErrorLine("Failed to load JSON file.", operationOptions);
                if (isPrintedJsonFile == false)
                    WriteErrorLine($"JSON File: {e.JsonFile}", operationOptions);
            }
        };

        writer.StartProcessComments += (sender, e) =>
        {
            Write("Processing comments...", operationOptions);
        };

        writer.FinishProcessComments += (sender, e) =>
        {
            if (e.Error == null)
            {
                WriteLine(" Done.", operationOptions);

                if (e.CommentsCount == e.TotalComments)
                {
                    WriteVerboseProperty("Comments", [e.CommentsCount.ToString("N0")]);
                }
                else
                {
                    WriteVerboseProperty("Comments", [e.CommentsCount.ToString("N0")]);
                    WriteVerboseProperty("Discarded Comments", [(e.TotalComments - e.CommentsCount).ToString("N0")]);
                    WriteVerboseProperty("Total Comments", [e.TotalComments.ToString("N0")]);
                }
            }
            else
            {
                WriteLine(operationOptions);
                WriteErrorLine("Failed to process comments.", operationOptions);
            }
        };

        writer.StartWriteComments += (sender, e) =>
        {
            Write($"Writing to {(e.WriteToHTML ? "HTML" : "text")} file...", operationOptions);
        };

        List<StringBuilder> warningMessages = [];
        int lostComments = 0;

        writer.WriteCommentError += (sender, e) =>
        {
            warningMessages.Add(e.Error.GetExceptionErrorMessage($"{(string.IsNullOrEmpty(e.Id) ? null : $"COMMENT ID: {e.Id}{Environment.NewLine}")}{(string.IsNullOrEmpty(e.Parent) ? null : $"PARENT ID: {e.Parent}{Environment.NewLine}")}COMMENT:{Environment.NewLine}{e.Comment}", withoutDebuggingInfo));
            lostComments += e.LostComments;
        };

        writer.FinishWriteComments += (sender, e) =>
        {
            if (e.Error == null && lostComments == 0)
            {
                WriteLine(" Done.", operationOptions);
                WriteVerboseProperty($"{(e.WriteToHTML ? "HTML" : "Text")} File", [e.CommentsFile]);
            }
            else
            {
                WriteLine(operationOptions);

                if (e.Error != null)
                {
                    WriteErrorLine($"Failed to write {(e.WriteToHTML ? "HTML" : "text")} file.", operationOptions);
                    WriteErrorLine($"{(e.WriteToHTML ? "HTML" : "Text")} File: {e.CommentsFile}", operationOptions);
                }

                if (lostComments > 0)
                {
                    var seperator = new string('-', 17);
                    WriteWarningLine($"Lost Comments: {lostComments}", operationOptions);
                    WriteWarningLine(seperator, operationOptions);
                    WriteWarningLine(string.Join($"{Environment.NewLine}{seperator}{Environment.NewLine}", warningMessages), operationOptions);
                }
            }
        };

        writer.StartDeleteJsonFile += (sender, e) =>
        {
            Write("Deleting JSON file...", operationOptions);
        };

        writer.FinishDeleteJsonFile += (sender, e) =>
        {
            if (e.Error == null)
            {
                WriteLine(" Done.", operationOptions);
            }
            else
            {
                WriteLine(operationOptions);
                WriteErrorLine("Failed to delete JSON file.", operationOptions);
            }
        };

        writer.Finish += (sender, e) =>
        {
            if (e.Error == null)
            {
                if (lostComments == 0)
                    WriteLine("Finished successfully.", operationOptions);
                else
                    WriteLine("Finished with warnings.", operationOptions);

                string processTime = e.ProcessTime.ToString(e.ProcessTime.Days > 0 ? "d':'hh':'mm':'ss'.'fff" : e.ProcessTime.Hours > 0 ? "h':'mm':'ss'.'fff" : "m':'ss'.'fff");
                WriteVerboseProperty($"Process Time", [processTime]);
            }
            else
            {
                WriteErrorLine(e.Error.GetExceptionErrorMessage("Failed to write comments file.", withoutDebuggingInfo), operationOptions);
            }
        };

        void WriteVerboseProperty(string property, string[] values)
        {
            if (operationOptions.Verbose)
                WriteProperty(property, values);
        }

        void WriteProperty(string property, string[] values)
        {
            WriteMessages(GetPropertyMessages(property, values), operationOptions, singleLine: true);
        }

        writer.WriteCommentsToFile();
    }
}
