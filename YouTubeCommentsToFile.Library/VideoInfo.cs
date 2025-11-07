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
    [YtDlpField("upload_date")]
    public string UploadDate { get; internal set; }
    [YtDlpField("description")]
    public string Description { get; internal set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Title:        {Title}");
        sb.AppendLine($"Uploader:     {Uploader}");
        sb.AppendLine($"Uploader ID:  {UploaderID}");
        sb.AppendLine($"Uploader URL: {UploaderURL}");
        sb.AppendLine($"Upload Date:  {UploadDate}");
        sb.AppendLine($"Description:{Environment.NewLine}{Description}");
        return sb.ToString();
    }
}
