namespace YouTubeCommentsToFile.Library;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class CodePageAttribute(int codePage) : Attribute
{
    public int CodePage => codePage;
}
