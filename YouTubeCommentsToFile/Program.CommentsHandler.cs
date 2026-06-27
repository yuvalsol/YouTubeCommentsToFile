using YouTubeCommentsToFile.Library;

namespace YouTubeCommentsToFile;

internal static partial class Program
{
    private class CommentsHandler(Settings settings)
    {
        private bool isDownloadComments;
        private readonly object lockObj = new();
        private bool isPrintedJsonFile;
        private bool pauseOnErrors;
        private bool isPrintedVideoTitle;
        private int lostComments;
        private readonly List<StringBuilder> warningMessages = [];
        private readonly bool withoutDebuggingInfo =
#if RELEASE
            true;
#elif DEBUG
            false;
#endif

        public void WriteCommentsToFile()
        {
            var writer = new CommentsWriter(settings);

            writer.Start += Start;
            writer.StartDownloadComments += StartDownloadComments;
            writer.DownloadCommentsProgressAsync += DownloadCommentsProgressAsync;
            writer.FinishDownloadComments += FinishDownloadComments;
            writer.StartDownloadVideoInfo += StartDownloadVideoInfo;
            writer.FinishDownloadVideoInfo += FinishDownloadVideoInfo;
            writer.StartLoadJsonFile += StartLoadJsonFile;
            writer.FinishLoadJsonFile += FinishLoadJsonFile;
            writer.StartProcessComments += StartProcessComments;
            writer.FinishProcessComments += FinishProcessComments;
            writer.StartWriteComments += StartWriteComments;
            writer.WriteCommentError += WriteCommentError;
            writer.FinishWriteComments += FinishWriteComments;
            writer.StartDeleteJsonFile += StartDeleteJsonFile;
            writer.FinishDeleteJsonFile += FinishDeleteJsonFile;
            writer.Finish += Finish;
            writer.Verbose += Verbose;

            writer.WriteCommentsToFile();

            if (pauseOnErrors)
                PressAnyKey();
        }

        private void Start(object sender, EventArgs e)
        {
            Console.WriteLine(GetVersion());
        }

        private void StartDownloadComments(object sender, StartDownloadCommentsEventArgs e)
        {
            Console.WriteLine(e.URL);
            Console.WriteLine("Downloading comments...");
            Console.WriteLine("yt-dlp command line:");
            Console.WriteLine($"yt-dlp {e.YtDlpCommandLine}");
            isDownloadComments = true;
        }

        private void DownloadCommentsProgressAsync(object sender, DownloadCommentsProgressEventArgs e)
        {
            lock (lockObj)
            {
                if (e.IsError)
                    WriteErrorLine(e.Line);
                else
                    Console.WriteLine(e.Line);
            }
        }

        private void FinishDownloadComments(object sender, FinishDownloadCommentsEventArgs e)
        {
            lock (lockObj)
            {
                if (e.Error == null)
                {
                    Console.WriteLine("Comments JSON file downloaded successfully.");
                    Console.WriteLine($"JSON file: {e.JsonFile}");
                    isPrintedJsonFile = true;
                }
                else
                {
                    WriteErrorLine("Failed to download comments JSON file.");
                    pauseOnErrors = true;
                }
            }
        }

        private void StartDownloadVideoInfo(object sender, StartDownloadVideoInfoEventArgs e)
        {
            Console.Write("Downloading video information...");
        }

        private void FinishDownloadVideoInfo(object sender, FinishDownloadVideoInfoEventArgs e)
        {
            if (e.Error == null)
            {
                Console.WriteLine(" Done.");
                Console.WriteLine($"Title: {e.VideoInfo.Title}");
                if (string.IsNullOrEmpty(e.VideoInfo.Uploader) == false)
                    Console.WriteLine($"Uploader: {e.VideoInfo.Uploader}");
                else if (string.IsNullOrEmpty(e.VideoInfo.UploaderID) == false)
                    Console.WriteLine($"Uploader: {e.VideoInfo.UploaderID}");
                isPrintedVideoTitle = true;
            }
            else
            {
                Console.WriteLine();
                WriteWarningLine(e.Error.GetExceptionErrorMessage("Failed to download video information.", withoutDebuggingInfo).ToString());
                pauseOnErrors = true;
            }
        }

        private void StartLoadJsonFile(object sender, StartLoadJsonFileEventArgs e)
        {
            if (isPrintedVideoTitle == false && isDownloadComments == false)
            {
                Console.WriteLine($"Title: {e.VideoInfo.Title}");
                if (string.IsNullOrEmpty(e.VideoInfo.Uploader) == false)
                    Console.WriteLine($"Uploader: {e.VideoInfo.Uploader}");
                else if (string.IsNullOrEmpty(e.VideoInfo.UploaderID) == false)
                    Console.WriteLine($"Uploader: {e.VideoInfo.UploaderID}");
            }

            Console.Write("Loading JSON file...");
        }

        private void FinishLoadJsonFile(object sender, FinishLoadJsonFileEventArgs e)
        {
            if (e.Error == null)
            {
                Console.WriteLine(" Done.");
                if (isPrintedJsonFile == false)
                    Console.WriteLine($"JSON file: {e.JsonFile}");
            }
            else
            {
                Console.WriteLine();
                WriteErrorLine("Failed to load JSON file.");
                if (isPrintedJsonFile == false)
                    WriteErrorLine($"JSON file: {e.JsonFile}");
                pauseOnErrors = true;
            }
        }

        private void StartProcessComments(object sender, StartProcessCommentsEventArgs e)
        {
            Console.Write("Processing comments...");
        }

        private void FinishProcessComments(object sender, FinishProcessCommentsEventArgs e)
        {
            if (e.Error == null)
            {
                Console.WriteLine(" Done.");

                if (e.CommentsCount == e.TotalComments)
                {
                    Console.WriteLine($"Comments: {e.CommentsCount:N0}");
                }
                else
                {
                    Console.WriteLine($"Comments: {e.CommentsCount:N0}");
                    Console.WriteLine($"Discarded Comments: {e.TotalComments - e.CommentsCount:N0}");
                    Console.WriteLine($"Total Comments: {e.TotalComments:N0}");
                }
            }
            else
            {
                Console.WriteLine();
                WriteErrorLine("Failed to process comments.");
                pauseOnErrors = true;
            }
        }

        private void StartWriteComments(object sender, StartWriteCommentsEventArgs e)
        {
            Console.Write($"Writing to {(e.WriteToHTML ? "HTML" : "text")} file...");
        }

        private void WriteCommentError(object sender, WriteCommentErrorEventArgs e)
        {
            warningMessages.Add(e.Error.GetExceptionErrorMessage($"{(string.IsNullOrEmpty(e.Id) ? null : $"COMMENT ID: {e.Id}{Environment.NewLine}")}{(string.IsNullOrEmpty(e.Parent) ? null : $"PARENT ID: {e.Parent}{Environment.NewLine}")}COMMENT:{Environment.NewLine}{e.Comment}", withoutDebuggingInfo));
            lostComments += e.LostComments;
            pauseOnErrors = true;
        }

        private void FinishWriteComments(object sender, FinishWriteCommentsEventArgs e)
        {
            if (e.Error == null && lostComments == 0)
            {
                Console.WriteLine(" Done.");
                Console.WriteLine($"{(e.WriteToHTML ? "HTML" : "Text")} file: {e.CommentsFile}");
            }
            else
            {
                Console.WriteLine();

                if (e.Error != null)
                {
                    WriteErrorLine($"Failed to write {(e.WriteToHTML ? "HTML" : "text")} file.");
                    WriteErrorLine($"{(e.WriteToHTML ? "HTML" : "Text")} file: {e.CommentsFile}");
                }

                if (lostComments > 0)
                {
                    WriteWarningLine($"Lost comments: {lostComments}");
                    WriteWarningLine("-----------------");
                    WriteWarningLine(string.Join($"{Environment.NewLine}-----------------{Environment.NewLine}", warningMessages));
                }

                pauseOnErrors = true;
            }
        }

        private void StartDeleteJsonFile(object sender, StartDeleteJsonFileEventArgs e)
        {
            Console.Write("Deleting JSON file...");
        }

        private void FinishDeleteJsonFile(object sender, FinishDeleteJsonFileEventArgs e)
        {
            if (e.Error == null)
            {
                Console.WriteLine(" Done.");
            }
            else
            {
                Console.WriteLine();
                WriteErrorLine("Failed to delete JSON file.");
                pauseOnErrors = true;
            }
        }

        private void Finish(object sender, FinishEventArgs e)
        {
            if (e.Error == null)
            {
                if (lostComments == 0)
                    Console.WriteLine("Finished successfully.");
                else
                    Console.WriteLine("Finished with warnings.");

                string processTime = e.ProcessTime.ToString(e.ProcessTime.Days > 0 ? "d':'hh':'mm':'ss'.'fff" : e.ProcessTime.Hours > 0 ? "h':'mm':'ss'.'fff" : "m':'ss'.'fff");
                Console.WriteLine($"Process Time: {processTime}");
            }
            else
            {
                WriteErrorLine(e.Error.GetExceptionErrorMessage("Failed to write comments file.", withoutDebuggingInfo).ToString());
                pauseOnErrors = true;
            }
        }

        private void Verbose(object sender, VerboseEventArgs e)
        {
            if (e.WriteMessagesInLines)
                WriteVerboseLines(e.Messages);
            else
                WriteVerbose(e.Messages);
        }
    }
}
