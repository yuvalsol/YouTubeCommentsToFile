namespace YouTubeCommentsToFile.Library;

internal class SpecialAuthor(string @class, Color color, Color backgroundColor, params string[] names)
{
    public string Class => @class;
    public Color Color => color;
    public Color BackgroundColor => backgroundColor;
    public string[] Names => names;

    public bool IsMatch(string author)
    {
        return Names.HasAny(name => author == name || author == $"@{name}");
    }

    public string ToCSS(string format)
    {
        return string.Format(format, Class, Color.ToRgb(), BackgroundColor.ToRgb());
    }
}

