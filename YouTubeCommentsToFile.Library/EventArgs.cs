namespace YouTubeCommentsToFile.Library;

public sealed class StartEventArgs : EventArgs
{
}

public sealed class EncodingsEventArgs(Encoding encoding) : EventArgs
{
    public Encoding Encoding => encoding;
}

public sealed class StartConvertEventArgs(string jsonFile, string url, int count, int total) : EventArgs
{
    public string JsonFile => jsonFile;
    public string URL => url;
    public int Count => count;
    public int Total => total;
}

public sealed class StartDownloadEventArgs(string url, int count, int total) : EventArgs
{
    public string URL => url;
    public int Count => count;
    public int Total => total;
}

public sealed class StartDownloadVideoInfoEventArgs(string ytDlpCommandLine) : EventArgs
{
    public string YtDlpCommandLine => ytDlpCommandLine;
}

public sealed class FinishDownloadVideoInfoEventArgs(string ytDlpCommandLine, VideoInfo videoInfo, Exception error = null) : EventArgs
{
    public string YtDlpCommandLine => ytDlpCommandLine;
    public VideoInfo VideoInfo => videoInfo;
    public Exception Error => error;
}

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

public sealed class WriteCommentErrorEventArgs(string id, string parent, string comment, int lostComments, Exception error) : EventArgs
{
    public string Id => id;
    public string Parent => parent;
    public string Comment => comment;
    public int LostComments => lostComments;
    public Exception Error => error;
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

public sealed class FinishConvertEventArgs(TimeSpan processTime, int count, int total, Exception error = null) : EventArgs
{
    public TimeSpan ProcessTime => processTime;
    public int Count => count;
    public int Total => total;
    public Exception Error => error;
}

public sealed class FinishDownloadEventArgs(TimeSpan processTime, int count, int total, Exception error = null) : EventArgs
{
    public TimeSpan ProcessTime => processTime;
    public int Count => count;
    public int Total => total;
    public Exception Error => error;
}

public sealed class SleepingEventArgs(TimeSpan timeout) : EventArgs
{
    public TimeSpan Timeout => timeout;
}

public sealed class FinishEventArgs(TimeSpan processTime) : EventArgs
{
    public TimeSpan ProcessTime => processTime;
}
