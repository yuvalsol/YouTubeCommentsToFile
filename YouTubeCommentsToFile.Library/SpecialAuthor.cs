namespace YouTubeCommentsToFile.Library;

internal record SpecialAuthor(string Class, Color Color, Color BackgroundColor, params string[] Names)
{
    public static IEnumerable<SpecialAuthor> SpecialAuthors =>
    [
        new("youtube", Color.White, Color.Red, "YouTube"),
        new("bilibili", Color.White, Color.FromArgb(6, 182, 242), "BiliBili", "哔哩哔哩"),
        new("reddit", Color.White, Color.FromArgb(255, 69, 0), "Reddit")
    ];

    public bool IsSpecialAuthor(Comment comment)
    {
        return Names.HasAny(name =>
            string.Compare(comment.Author, name, StringComparison.InvariantCultureIgnoreCase) == 0 ||
            string.Compare(comment.Author, $"@{name}", StringComparison.InvariantCultureIgnoreCase) == 0
        );
    }
}
