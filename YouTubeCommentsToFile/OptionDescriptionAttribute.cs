namespace YouTubeCommentsToFile;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
internal sealed class OptionDescriptionAttribute(string description) : Attribute
{
    public string Description => description;
}
