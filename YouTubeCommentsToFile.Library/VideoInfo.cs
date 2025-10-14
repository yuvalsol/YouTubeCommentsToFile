namespace YouTubeCommentsToFile.Library;

public class VideoInfo
{
    [YtDlpField("title")]
    public string Title { get; internal set; }
    [YtDlpField("uploader")]
    public string Uploader { get; internal set; }
    [YtDlpField("uploader_id")]
    public string UploaderID { get; internal set; }
    [YtDlpField("uploader_url")]
    public string UploaderURL { get; internal set; }
    [YtDlpField("description")]
    public string Description { get; internal set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        if (string.IsNullOrEmpty(Title) == false)
            sb.AppendLine(Title);

        if (string.IsNullOrEmpty(Uploader) == false)
            sb.AppendLine(Uploader);

        if (string.IsNullOrEmpty(UploaderID) == false)
            sb.AppendLine(UploaderID);

        if (string.IsNullOrEmpty(UploaderURL) == false)
            sb.AppendLine(UploaderURL);

        if (string.IsNullOrEmpty(Description) == false)
        {
            sb.AppendLine();
            sb.AppendLine(Description);
        }

        return sb.ToString();
    }
}
