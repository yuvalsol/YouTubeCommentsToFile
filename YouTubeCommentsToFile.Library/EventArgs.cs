namespace YouTubeCommentsToFile.Library;

public sealed class StartDownloadCommentsEventArgs(string url, string ytDlpCommandLine) : EventArgs
{
    public string URL => url;
    public string YtDlpCommandLine => ytDlpCommandLine;
}

public sealed class DownloadCommentsProgressEventArgs(string line, bool isError = false) : EventArgs
{
    public string Line => line;
    public bool IsError => isError;
}

public sealed class FinishDownloadCommentsEventArgs(string url, string ytDlpCommandLine, string jsonFile, Exception error = null) : EventArgs
{
    public string URL => url;
    public string YtDlpCommandLine => ytDlpCommandLine;
    public string JsonFile => jsonFile;
    public Exception Error => error;
}

public sealed class StartDownloadVideoInfoEventArgs(VideoInfo videoInfo) : EventArgs
{
    public VideoInfo VideoInfo => videoInfo;
}

public sealed class FinishDownloadVideoInfoEventArgs(VideoInfo videoInfo, Exception error = null) : EventArgs
{
    public VideoInfo VideoInfo => videoInfo;
    public Exception Error => error;
}

public sealed class StartLoadJsonFileEventArgs(VideoInfo videoInfo, string jsonFile) : EventArgs
{
    public VideoInfo VideoInfo => videoInfo;
    public string JsonFile => jsonFile;
}

public sealed class FinishLoadJsonFileEventArgs(VideoInfo videoInfo, string jsonFile, Exception error = null) : EventArgs
{
    public VideoInfo VideoInfo => videoInfo;
    public string JsonFile => jsonFile;
    public Exception Error => error;
}

public sealed class StartProcessCommentsEventArgs(VideoInfo videoInfo) : EventArgs
{
    public VideoInfo VideoInfo => videoInfo;
}

public sealed class FinishProcessCommentsEventArgs(VideoInfo videoInfo, int commentsCount, int totalComments, Exception error = null) : EventArgs
{
    public VideoInfo VideoInfo => videoInfo;
    public int CommentsCount => commentsCount;
    public int TotalComments => totalComments;
    public Exception Error => error;
}

public sealed class StartWriteCommentsEventArgs(VideoInfo videoInfo, string commentsFile, bool writeToHTML) : EventArgs
{
    public VideoInfo VideoInfo => videoInfo;
    public string CommentsFile => commentsFile;
    public bool WriteToHTML => writeToHTML;
}

public sealed class WriteCommentErrorEventArgs(string comment, Exception error, int lostComments) : EventArgs
{
    public string Comment => comment;
    public Exception Error => error;
    public int LostComments => lostComments;
}

public sealed class FinishWriteCommentsEventArgs(VideoInfo videoInfo, string commentsFile, bool writeToHTML, Exception error = null) : EventArgs
{
    public VideoInfo VideoInfo => videoInfo;
    public string CommentsFile => commentsFile;
    public bool WriteToHTML => writeToHTML;
    public Exception Error => error;
}

public sealed class StartDeleteJsonFileEventArgs(string jsonFile) : EventArgs
{
    public string JsonFile => jsonFile;
}

public sealed class FinishDeleteJsonFileEventArgs(string jsonFile, Exception error = null) : EventArgs
{
    public string JsonFile => jsonFile;
    public Exception Error => error;
}

public sealed class FinishEventArgs(TimeSpan processTime, Exception error = null) : EventArgs
{
    public TimeSpan ProcessTime => processTime;
    public Exception Error => error;
}

public sealed class TracepointEventArgs(string message) : EventArgs
{
    public string Message => message;
}
