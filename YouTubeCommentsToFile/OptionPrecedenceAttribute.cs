namespace YouTubeCommentsToFile;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class OptionPrecedenceAttribute(int group, int ordinal) : Attribute
{
    public int Group => group;
    public int Ordinal => ordinal;

    public OptionPrecedenceAttribute(int ordinal) : this(0, ordinal) { }
}
