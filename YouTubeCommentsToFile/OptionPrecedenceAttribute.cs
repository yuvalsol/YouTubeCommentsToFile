namespace YouTubeCommentsToFile;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Property, AllowMultiple = false)]
internal sealed class OptionPrecedenceAttribute(int ordinal) : Attribute
{
    public int Ordinal => ordinal;
}
