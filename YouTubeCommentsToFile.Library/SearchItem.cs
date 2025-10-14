namespace YouTubeCommentsToFile.Library;

public abstract class SearchItem(bool highlight, bool filter)
{
    public bool Highlight { get; protected set; } = highlight;
    public bool Filter { get; protected set; } = filter;

    internal abstract bool IsMatch(Comment comment);
}

public abstract class SearchValueItem(string value, bool highlight, bool filter, bool ignoreCase)
    : SearchItem(highlight, filter)
{
    protected string Value = value;
    public bool IgnoreCase { get; protected set; } = ignoreCase;
}

public class SearchUploader(bool highlight, bool filter)
    : SearchItem(highlight, filter)
{
    public static readonly SearchUploader FilterUploader = new(highlight: false, filter: true);
    public static readonly SearchUploader HighlightUploader = new(highlight: true, filter: false);

    public SearchUploader()
        : this(highlight: true, filter: false)
    { }

    internal override bool IsMatch(Comment comment)
    {
        return comment.AuthorIsUploader;
    }
}

public class SearchAuthor(string author, bool highlight, bool filter, bool ignoreCase = false)
    : SearchValueItem(author, highlight, filter, ignoreCase)
{
    public string Author => Value;

    public static SearchAuthor FilterAuthor(string author, bool ignoreCase = false) => new(author, highlight: false, filter: true, ignoreCase);
    public static SearchAuthor HighlightAuthor(string author, bool ignoreCase = false) => new(author, highlight: true, filter: false, ignoreCase);

    public SearchAuthor(string author, bool ignoreCase = false)
        : this(author, highlight: true, filter: false, ignoreCase)
    { }

    internal override bool IsMatch(Comment comment)
    {
        if (string.IsNullOrEmpty(Author))
            return false;

        string author = (Author.StartsWith('@') ? Author[1..] : $"@{Author}");

        // CompareOptions.IgnoreNonSpace: ignore diacritics (e == é)
        return
            string.Compare(comment.Author, Author, CultureInfo.InvariantCulture, (IgnoreCase ? CompareOptions.IgnoreCase : CompareOptions.None) | CompareOptions.IgnoreNonSpace) == 0 ||
            string.Compare(comment.Author, author, CultureInfo.InvariantCulture, (IgnoreCase ? CompareOptions.IgnoreCase : CompareOptions.None) | CompareOptions.IgnoreNonSpace) == 0;
    }
}

public class SearchText(string text, bool highlight, bool filter, bool ignoreCase = false)
    : SearchValueItem(text, highlight, filter, ignoreCase)
{
    public string Text => Value;

    public static SearchText FilterText(string text, bool ignoreCase = false) => new(text, highlight: false, filter: true, ignoreCase);
    public static SearchText HighlightText(string text, bool ignoreCase = false) => new(text, highlight: true, filter: false, ignoreCase);

    public SearchText(string text, bool ignoreCase = false)
        : this(text, highlight: true, filter: false, ignoreCase)
    { }

    internal override bool IsMatch(Comment comment)
    {
        if (string.IsNullOrEmpty(Text))
            return false;

        /*
         * can't use this code:
         * 
         * return CultureInfo.InvariantCulture.CompareInfo.IndexOf(comment.Text, Text,
         *     (IgnoreCase ? CompareOptions.IgnoreCase : CompareOptions.None) | CompareOptions.IgnoreNonSpace
         * ) != -1;
         * 
         * there is a bug in .NET 8.
         * when using just CompareOptions.IgnoreNonSpace, it also ignores case:
         * 
         * return CultureInfo.InvariantCulture.CompareInfo.IndexOf(comment.Text, Text,
         *     CompareOptions.IgnoreNonSpace
         * ) != -1;
         * 
         * settle for ignoring case without ignoring diacritics
         * 
         */

        return comment.Text.Contains(Text, (IgnoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
    }
}
