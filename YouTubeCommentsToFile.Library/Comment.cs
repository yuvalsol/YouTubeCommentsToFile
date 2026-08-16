namespace YouTubeCommentsToFile.Library;

internal abstract partial class Comment : ICollection<Comment>, IComparable<Comment>, IComparer<Comment>
{
    #region Comment Properties

    public virtual string Id { get; set; }
    public virtual string Parent { get; set; }
    public virtual string Text { get; set; }
    public virtual string Author { get; set; }
    public virtual bool AuthorIsUploader { get; set; }

    protected virtual int LikeCount { get; set; }
    protected virtual string AuthorURL { get; set; }
    protected virtual bool IsFavorited { get; set; }
    protected virtual string TimeText { get; set; }
    protected virtual bool IsPinned { get; set; }

    public virtual bool IsTopLevelComment => string.IsNullOrEmpty(Parent);
    public virtual bool IsReply => !IsTopLevelComment;

    protected virtual string MissingAuthor => "Missing Author";

    #endregion

    #region Comment Writing Properties

    public int CommentNumber { get; set; }
    public int ReplyNumber { get; set; }
    public bool IsHighlightUploader { get; set; }
    public bool IsHighlightAuthor { get; set; }
    public bool IsHighlightText { get; set; }
    public bool IsHighlight => IsHighlightUploader || IsHighlightAuthor || IsHighlightText;

    #endregion

    #region Replies

    private List<Comment> replies;
    public Comment ParentComment { get; private set; }

    public int Count => replies?.Count ?? 0;

    public bool IsReadOnly => false;

    public void Add(Comment reply)
    {
        replies ??= [];
        replies.Add(reply);
        reply.ParentComment = this;
    }

    public void Clear()
    {
        if (replies.IsNullOrEmpty())
            return;

        foreach (var reply in replies)
            reply.ParentComment = null;

        replies.Clear();
    }

    public bool Contains(Comment reply)
    {
        return replies?.Contains(reply) ?? false;
    }

    public void CopyTo(Comment[] array, int arrayIndex)
    {
        replies?.CopyTo(array, arrayIndex);
    }

    public bool Remove(Comment reply)
    {
        if (replies?.Remove(reply) ?? false)
        {
            reply.ParentComment = null;
            return true;
        }

        return false;
    }

    public int RemoveAll(Predicate<Comment> match)
    {
        if (replies.IsNullOrEmpty())
            return 0;

        foreach (var reply in replies.FindAll(match))
            reply.ParentComment = null;

        return replies.RemoveAll(match);
    }

    private static readonly List<Comment> EmptyReplies = [];

    public IEnumerator<Comment> GetEnumerator()
    {
        return ((IEnumerable<Comment>)(replies ?? EmptyReplies)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)(replies ?? EmptyReplies)).GetEnumerator();
    }

    #endregion

    #region Compare By Id

    public int CompareTo(Comment other)
    {
        return Compare(this, other);
    }

    public int Compare(Comment x, Comment y)
    {
        if (ReferenceEquals(x, y))
            return 0;

        if (x is null)
            return -1;

        if (y is null)
            return 1;

        return x.Id.CompareTo(y.Id);
    }

    #endregion

    #region To Lines, To String, To Error

    public virtual string ToError()
    {
        try
        {
            return ToString();
        }
        catch
        {
            return $"{(string.IsNullOrEmpty(Author) ? MissingAuthor : Author)}{(string.IsNullOrEmpty(TimeText) ? null : $" {TimeText}")}{Environment.NewLine}{Text}";
        }
    }

    public override string ToString()
    {
        return string.Join(Environment.NewLine, ToLines(
            settings: new(),
            websiteInfo: new YouTube(),
            uploaderId: null,
            indentCount: 0,
            textLineLength: CommentsWriter.TEXT_LINE_MAX_LENGTH,
            textLineLengthRepliesDeep: CommentsWriter.TEXT_LINE_MIN_LENGTH / 2,
            allAuthors: null,
            writeToHTML: false
        ));
    }

    public virtual IEnumerable<string> ToLines(
        Settings settings,
        WebsiteInfo websiteInfo,
        string uploaderId,
        int indentCount,
        int textLineLength,
        int textLineLengthRepliesDeep,
        List<string> allAuthors,
        bool writeToHTML)
    {
        bool isHTMLHighlight = writeToHTML && IsHighlight;
        bool isTextHighlight = writeToHTML == false && IsHighlight;

        int widthCh = textLineLength - indentCount;
        if (widthCh < textLineLengthRepliesDeep)
            widthCh = textLineLengthRepliesDeep;

        int lineCh = widthCh;
        if (isTextHighlight)
            lineCh -= 4; // "│ " + line + " │"

        string highlightClass = (isHTMLHighlight ? (IsHighlightUploader ? "highlight-uploader" : (IsHighlightAuthor ? "highlight-author" : "highlight-text")) : null);
        string openHighlightBorderTop = (isHTMLHighlight ? $"<span class='{highlightClass} highlight-border-top width-{lineCh}'>" : null);
        string openHighlightBorderMiddle = (isHTMLHighlight ? $"<span class='{highlightClass} highlight-border-middle width-{lineCh}'>" : null);
        string openHighlightBorderBottom = (isHTMLHighlight ? $"<span class='{highlightClass} highlight-border-bottom width-{lineCh}'>" : null);
        string closeHighlightBorder = (isHTMLHighlight ? "</span>" : null);

        string id = (writeToHTML ? $"c{CommentNumber}{(IsReply ? $"r{ReplyNumber}" : null)}" : null);

        string commentURLPart = null;
        if (ShowCommentLink(settings) || writeToHTML)
        {
            if (websiteInfo.IsCommentURLQueryString)
                commentURLPart = $"&{websiteInfo.CommentURLParameter}={Id}";
            else if (websiteInfo.IsCommentURLPath)
                commentURLPart = $"/{Id}";
        }

        if (ShowPinned())
        {
            string line = $"Pinned{(string.IsNullOrEmpty(uploaderId) ? null : $" by {uploaderId}")}";

            if (writeToHTML)
                yield return $"<span class='pinned'>{HttpUtility.HtmlEncode(line)}</span>";
            else
                yield return line;
        }

        if (isTextHighlight)
            yield return $"┌{new string('─', widthCh - 2)}┐";

        if (writeToHTML)
        {
            string anchorReplyId = (IsReply ? $" id='c{CommentNumber}r{ReplyNumber}'" : null);

            string author;
            if (string.IsNullOrEmpty(Author))
            {
                if (string.IsNullOrEmpty(AuthorURL))
                    author = $"<span{anchorReplyId} class='author'>{HttpUtility.HtmlEncode(MissingAuthor)}</span>";
                else
                    author = $"<a{anchorReplyId} href='{HttpUtility.HtmlAttributeEncode(AuthorURL)}'><span class='author'>{HttpUtility.HtmlEncode(MissingAuthor)}</span></a>";
            }
            else
            {
                string authorClass = SpecialAuthor.SpecialAuthors.FirstOrDefault(sa => sa.IsSpecialAuthor(this))?.Class ?? (AuthorIsUploader ? "uploader" : "author");

                if (string.IsNullOrEmpty(AuthorURL))
                    author = $"<span{anchorReplyId} class='{authorClass}'>{HttpUtility.HtmlEncode(Author)}</span>";
                else
                    author = $"<a{anchorReplyId} href='{HttpUtility.HtmlAttributeEncode(AuthorURL)}'><span class='{authorClass}'>{HttpUtility.HtmlEncode(Author)}</span></a>";
            }

            string time = null;
            if (ShowTime(settings))
            {
                if (string.IsNullOrEmpty(settings.URL) || string.IsNullOrEmpty(commentURLPart))
                    time = $" <span class='time'>{HttpUtility.HtmlEncode(TimeText)}</span>";
                else
                    time = $" <a href='{HttpUtility.HtmlAttributeEncode($"{settings.URL}{commentURLPart}")}'><span class='time'>{HttpUtility.HtmlEncode(TimeText)}</span></a>";
            }

            string commentLink = null;
            if (ShowCommentLink(settings) && string.IsNullOrEmpty(commentURLPart) == false)
            {
                if (string.IsNullOrEmpty(settings.URL))
                    commentLink = $" <span class='comment-link-text'>{commentURLPart}</span>";
                else
                    commentLink = $" <a class='link' href='{HttpUtility.HtmlAttributeEncode($"{settings.URL}{commentURLPart}")}'><span class='comment-link'>Highlighted {(IsReply ? "Reply" : "Comment")}</span></a>";
            }

            string copyText = null;
            if (settings.ShowCopyLinks)
                copyText = $" <span class='copy-text' data-id='{id}' title='Copy {(IsReply ? "Reply" : "Comment")} To Clipboard'>Copy</span>";

            yield return $"{openHighlightBorderTop}{author}{time}{commentLink}{copyText}{closeHighlightBorder}";
        }
        else
        {
            string line = $"{(string.IsNullOrEmpty(Author) ? MissingAuthor : Author)}{(ShowTime(settings) ? $" {TimeText}" : null)}";

            if (isTextHighlight)
            {
                int diff = lineCh - line.MeasureText();
                yield return $"{(IsReply ? '┤' : '│')} {line}{(diff > 0 ? new string(' ', diff) : null)} │";
            }
            else
            {
                yield return line;
            }

            if (ShowCommentLink(settings) && string.IsNullOrEmpty(commentURLPart) == false)
            {
                line = $"{settings.URL}{commentURLPart}";

                if (isTextHighlight)
                {
                    int diff = lineCh - line.MeasureText();
                    yield return $"│ {line}{(diff > 0 ? new string(' ', diff) : null)} │";
                }
                else
                {
                    yield return line;
                }
            }
        }

        string[] lines = GetLines(writeToHTML);

        var rtlBlocks = (writeToHTML ? lines.GetRTLBlocks() : null);

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            int lineLength = line.MeasureText();
            bool isLineRTL = writeToHTML && rtlBlocks.Any(b => b.start <= i && i <= b.end);

            if (lineLength <= lineCh)
            {
                if (writeToHTML)
                {
                    if (isHTMLHighlight && lineLength == 0)
                        line = " ";

                    yield return $"{(ShowLikes(settings) == false && i + 1 == lines.Length ? openHighlightBorderBottom : openHighlightBorderMiddle)}<span class='{(IsReply ? "reply" : "comment")}{(isLineRTL ? $" rtl width-{lineCh}" : null)}'{(settings.ShowCopyLinks ? $" data-id='{id}'" : null)}>{line.ToHtml(settings.URL, websiteInfo.HashtagURL, websiteInfo.IsSupportHashtagWithSpace, websiteInfo.IsToLowerCaseHashtag, checkForRepliedAuthor: i == 0, allAuthors)}</span>{closeHighlightBorder}";
                }
                else
                {
                    if (isTextHighlight)
                    {
                        int diff = lineCh - lineLength;
                        yield return $"│ {line}{(diff > 0 ? new string(' ', diff) : null)} │";
                    }
                    else
                    {
                        yield return line;
                    }
                }
            }
            else
            {
                if (writeToHTML)
                {
                    string[] innerLines = line.SplitToHtmlLines(lineCh, settings.URL, websiteInfo.HashtagURL, websiteInfo.IsSupportHashtagWithSpace, websiteInfo.IsToLowerCaseHashtag, checkForRepliedAuthor: i == 0, allAuthors);
                    for (int k = 0; k < innerLines.Length; k++)
                    {
                        string innerLine = (innerLines[k] ?? string.Empty).Trim();

                        if (isHTMLHighlight && innerLine.Length == 0)
                            innerLine = " ";

                        yield return $"{(ShowLikes(settings) == false && i + 1 == lines.Length && k + 1 == innerLines.Length ? openHighlightBorderBottom : openHighlightBorderMiddle)}<span class='{(IsReply ? "reply" : "comment")}{(isLineRTL ? $" rtl width-{lineCh}" : null)}'{(settings.ShowCopyLinks ? $" data-id='{id}'" : null)}>{innerLine}</span>{closeHighlightBorder}";
                    }
                }
                else
                {
                    string[] innerLines = line.SplitToLines(lineCh);
                    for (int k = 0; k < innerLines.Length; k++)
                    {
                        string innerLine = (innerLines[k] ?? string.Empty).Trim();
                        if (isTextHighlight)
                        {
                            int diff = lineCh - innerLine.MeasureText();
                            yield return $"│ {innerLine}{(diff > 0 ? new string(' ', diff) : null)} │";
                        }
                        else
                        {
                            yield return innerLine;
                        }
                    }
                }
            }
        }

        if (ShowLikes(settings))
        {
            string line = $"{LikeCount} Like{(LikeCount == 1 ? string.Empty : "s")}";

            if (writeToHTML)
            {
                yield return $"{openHighlightBorderBottom}<span class='likes'>{HttpUtility.HtmlEncode(line)}</span>{closeHighlightBorder}";
            }
            else
            {
                if (isTextHighlight)
                {
                    int diff = lineCh - line.MeasureText();
                    yield return $"│ {line}{(diff > 0 ? new string(' ', diff) : null)} │";
                }
                else
                {
                    yield return line;
                }
            }
        }

        if (isTextHighlight)
            yield return $"{(Count > 0 && ShowFavorited(uploaderId) == false ? '├' : '└')}{new string('─', widthCh - 2)}┘";

        if (ShowFavorited(uploaderId))
        {
            if (writeToHTML)
                yield return $"<span class='favorited'><span class='heart'>{HttpUtility.HtmlEncode("❤")}</span>{HttpUtility.HtmlEncode($" by {uploaderId}")}</span>";
            else
                yield return $"♥ by {uploaderId}";
        }
    }

    public virtual bool HasRTLLine(bool writeToHTML)
    {
        return GetLines(writeToHTML).HasRTLLine();
    }

    protected virtual string[] GetLines(bool writeToHTML)
    {
        var lines =
            (writeToHTML ? Text : Text.CleanTextForTextFile())
            .Split([Environment.NewLine, "\n"], StringSplitOptions.None)
            .Select(line => (line ?? string.Empty).Trim())
            .ToArray();

        if (lines.HasAny() && lines[0].GetRepliedAuthor(out var repliedAuthor))
        {
            int index = repliedAuthor.Index + repliedAuthor.Length;
            string line1 = lines[0][..index].TrimEnd();
            string line2 = lines[0][index..].TrimStart();
            if (string.IsNullOrWhiteSpace(line2) == false)
                lines = [line1, line2, .. lines.Skip(1)];
        }

        return lines;
    }

    protected virtual bool ShowTime(Settings settings)
    {
        return
            settings.HideTime == false &&
            string.IsNullOrEmpty(TimeText) == false;
    }

    protected virtual bool ShowCommentLink(Settings settings)
    {
        return settings.ShowCommentLink;
    }

    protected virtual bool ShowPinned()
    {
        return IsPinned;
    }

    protected virtual bool ShowLikes(Settings settings)
    {
        return settings.HideLikes == false;
    }

    protected virtual bool ShowFavorited(string uploaderId)
    {
        return
            IsFavorited &&
            string.IsNullOrEmpty(uploaderId) == false;
    }

    #endregion
}
