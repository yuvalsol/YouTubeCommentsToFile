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
    [YtDlpField("tags")]
    public string Tags { get; internal set; }

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

    public string[] GetKeywords()
    {
        if (string.IsNullOrEmpty(Tags))
            return [];

        string tags = Tags;

        if (tags.StartsWith('[') && tags.EndsWith(']'))
            tags = tags[1..^1];

        string[] keywords = tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        for (int i = 0; i < keywords.Length; i++)
        {
            string keyword = keywords[i];
            if (keyword.StartsWith('\'') && keyword.EndsWith('\''))
                keywords[i] = keyword[1..^1];
        }

        return keywords;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Title:        {Title}");
        sb.AppendLine($"Uploader:     {Uploader}");
        sb.AppendLine($"Uploader ID:  {UploaderID}");
        sb.AppendLine($"Uploader URL: {UploaderURL}");
        sb.AppendLine($"Upload Date:  {GetUploadDate()?.ToString("d") ?? UploadDate}");
        sb.AppendLine($"Description:{Environment.NewLine}{Description}");
        sb.AppendLine($"Keywords: {string.Join(", ", GetKeywords())}");
        return sb.ToString();
    }
}
