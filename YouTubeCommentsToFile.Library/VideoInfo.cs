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
    [YtDlpField("language")]
    public string Language { get; internal set; }
    [YtDlpField("description")]
    public string Description { get; internal set; }

    public DateTime? GetUploadDate()
    {
        if (string.IsNullOrEmpty(UploadDate))
            return null;

        try
        {
            if (int.TryParse(UploadDate[..4], out int year) == false)
                return null;

            if (int.TryParse(UploadDate[4..6], out int month) == false)
                return null;

            if (int.TryParse(UploadDate[6..], out int day) == false)
                return null;

            // Video upload date in UTC (YYYYMMDD)
            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        }
        catch
        {
            return null;
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Title:        {Title}");
        sb.AppendLine($"Uploader:     {Uploader}");
        sb.AppendLine($"Uploader ID:  {UploaderID}");
        sb.AppendLine($"Uploader URL: {UploaderURL}");
        sb.AppendLine($"Upload Date:  {GetUploadDate()?.ToString("d") ?? UploadDate}");
        sb.AppendLine($"Language:     {Language}");
        sb.AppendLine($"Description:{Environment.NewLine}{Description}");
        return sb.ToString();
    }
}
