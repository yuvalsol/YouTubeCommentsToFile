namespace YouTubeCommentsToFile.Library;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class YtDlpFieldAttribute(string fieldName) : Attribute
{
    public string Field => fieldName;
}
