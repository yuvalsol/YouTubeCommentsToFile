namespace YouTubeCommentsToFile;

internal static partial class ColorHelper
{
    [GeneratedRegex(@"^\s*(?<R>1?\d{1,2}|2(?:[0-4]\d|5[0-5]))\s*[, ]\s*(?<G>1?\d{1,2}|2(?:[0-4]\d|5[0-5]))\s*[, ]\s*(?<B>1?\d{1,2}|2(?:[0-4]\d|5[0-5]))\s*$")]
    private static partial Regex RegexColorInRGB();

    [GeneratedRegex(@"^(?:[a-fA-F0-9]{6}|[a-fA-F0-9]{8})$")]
    private static partial Regex RegexColorInHex();

    public static Color? StringToColor(string colorStr)
    {
        if (string.IsNullOrEmpty(colorStr))
            return null;

        if (RegexColorInRGB().IsMatch(colorStr))
        {
            var match = RegexColorInRGB().Match(colorStr);
            int r = Convert.ToInt32(match.Groups["R"].Value);
            int g = Convert.ToInt32(match.Groups["G"].Value);
            int b = Convert.ToInt32(match.Groups["B"].Value);
            return Color.FromArgb(r, g, b);
        }

        if (RegexColorInHex().IsMatch(colorStr))
            colorStr = $"#{colorStr}";

        try
        {
            var tc = TypeDescriptor.GetConverter(typeof(Color));
            return tc.ConvertFromString(colorStr) as Color?;
        }
        catch
        {
            return null;
        }
    }
}
