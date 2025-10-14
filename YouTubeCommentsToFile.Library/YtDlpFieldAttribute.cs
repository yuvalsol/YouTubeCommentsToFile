namespace YouTubeCommentsToFile.Library;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class YtDlpFieldAttribute(string field) : Attribute
{
    public string Field => field;
}
