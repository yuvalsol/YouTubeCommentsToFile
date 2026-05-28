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
    protected virtual int Ups { get; set; }
    protected virtual int Downs { get; set; }
    protected virtual string AuthorURL { get; set; }
    protected virtual bool IsFavorited { get; set; }
    protected virtual string TimeText { get; set; }
    protected virtual bool IsPinned { get; set; }

    public virtual bool IsTopLevelComment => string.IsNullOrEmpty(Parent);
    public virtual bool IsReply => !IsTopLevelComment;

    #endregion

    #region Comment Writing Properties

    public int CommentNumber { get; set; }
    public int ReplyNumber { get; set; }
    public bool IsHighlight { get; set; }

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

    public virtual bool AuthorIsYouTube => Author == "@YouTube";
    protected virtual string MissingAuthor => "Missing Author";

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
            websiteInfo: WebsiteInfo.YouTube,
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
        int widthCh = textLineLength - indentCount;
        if (widthCh < textLineLengthRepliesDeep)
            widthCh = textLineLengthRepliesDeep;

        int lineCh = widthCh;
        if (writeToHTML == false && IsHighlight)
            lineCh -= 4; // "│ " + line + " │"

        string openHighlightBorderTop = (writeToHTML && IsHighlight ? $"<span class='highlight highlight-border-top width-{lineCh}'>" : null);
        string openHighlightBorderMiddle = (writeToHTML && IsHighlight ? $"<span class='highlight highlight-border-middle width-{lineCh}'>" : null);
        string openHighlightBorderBottom = (writeToHTML && IsHighlight ? $"<span class='highlight highlight-border-bottom width-{lineCh}'>" : null);
        string closeHighlightBorder = (writeToHTML && IsHighlight ? "</span>" : null);

        string id = (writeToHTML ? $"c{CommentNumber}{(IsReply ? $"r{ReplyNumber}" : null)}" : null);

        bool isFavorited = IsFavorited && string.IsNullOrEmpty(uploaderId) == false;

        string commentLink = null;
        if (settings.ShowCommentLink || writeToHTML)
        {
            if (websiteInfo.IsCommentURLQueryString)
                commentLink = $"&{websiteInfo.CommentURLParameter}={Id}";
            else if (websiteInfo.IsCommentURLPath)
                commentLink = $"/{Id}";
        }

        if (IsPinned)
        {
            if (writeToHTML)
                yield return $"<span class='pinned'>{HttpUtility.HtmlEncode($"Pinned{(string.IsNullOrEmpty(uploaderId) ? null : $" by {uploaderId}")}")}</span>";
            else
                yield return $"Pinned{(string.IsNullOrEmpty(uploaderId) ? null : $" by {uploaderId}")}";
        }

        if (writeToHTML == false && IsHighlight)
            yield return $"┌{new string('─', widthCh - 2)}┐";

        if (writeToHTML)
        {
            string anchorReplyId = (IsReply ? $" id='c{CommentNumber}r{ReplyNumber}'" : null);

            string authorPart;
            if (string.IsNullOrEmpty(Author))
            {
                if (string.IsNullOrEmpty(AuthorURL))
                    authorPart = $"<span{anchorReplyId} class='author'>{HttpUtility.HtmlEncode(MissingAuthor)}</span>";
                else
                    authorPart = $"<a{anchorReplyId} href='{HttpUtility.HtmlAttributeEncode(AuthorURL)}'><span class='author'>{HttpUtility.HtmlEncode(MissingAuthor)}</span></a>";
            }
            else
            {
                if (string.IsNullOrEmpty(AuthorURL))
                    authorPart = $"<span{anchorReplyId} class='{(AuthorIsYouTube ? "youtube" : (AuthorIsUploader ? "uploader" : "author"))}'>{HttpUtility.HtmlEncode(Author)}</span>";
                else
                    authorPart = $"<a{anchorReplyId} href='{HttpUtility.HtmlAttributeEncode(AuthorURL)}'><span class='{(AuthorIsYouTube ? "youtube" : (AuthorIsUploader ? "uploader" : "author"))}'>{HttpUtility.HtmlEncode(Author)}</span></a>";
            }

            string timePart = null;
            if (settings.HideTime == false && string.IsNullOrEmpty(TimeText) == false)
            {
                if (string.IsNullOrEmpty(settings.URL) || string.IsNullOrEmpty(commentLink))
                    timePart = $" <span class='time'>{HttpUtility.HtmlEncode(TimeText)}</span>";
                else
                    timePart = $" <a href='{HttpUtility.HtmlAttributeEncode($"{settings.URL}{commentLink}")}'><span class='time'>{HttpUtility.HtmlEncode(TimeText)}</span></a>";
            }

            string commentLinkPart = null;
            if (settings.ShowCommentLink && string.IsNullOrEmpty(commentLink) == false)
            {
                if (string.IsNullOrEmpty(settings.URL))
                    commentLinkPart = $" <span class='comment-link-text'>{commentLink}</span>";
                else
                    commentLinkPart = $" <a class='link' href='{HttpUtility.HtmlAttributeEncode($"{settings.URL}{commentLink}")}'><span class='comment-link'>Highlighted {(IsReply ? "Reply" : "Comment")}</span></a>";
            }

            string copyTextPart = null;
            if (settings.ShowCopyLinks)
                copyTextPart = $" <span class='copy-text' data-id='{id}' title='Copy {(IsReply ? "Reply" : "Comment")} To Clipboard'>Copy</span>";

            yield return $"{openHighlightBorderTop}{authorPart}{timePart}{commentLinkPart}{copyTextPart}{closeHighlightBorder}";
        }
        else
        {
            string line = $"{(string.IsNullOrEmpty(Author) ? MissingAuthor : Author)}{(settings.HideTime || string.IsNullOrEmpty(TimeText) ? null : $" {TimeText}")}";
            if (IsHighlight)
            {
                int diff = lineCh - line.GetTextLengthForTextFile();
                yield return $"{(IsReply ? '┤' : '│')} {line}{(diff > 0 ? new string(' ', diff) : null)} │";
            }
            else
            {
                yield return line;
            }

            if (settings.ShowCommentLink && string.IsNullOrEmpty(commentLink) == false)
            {
                line = $"{settings.URL}{commentLink}";
                if (IsHighlight)
                {
                    int diff = lineCh - line.GetTextLengthForTextFile();
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
            bool isLineRTL = writeToHTML && rtlBlocks.Any(b => b.start <= i && i <= b.end);
            int lineLength = (writeToHTML ? line.Length : line.GetTextLengthForTextFile());

            if (lineLength <= lineCh)
            {
                if (writeToHTML)
                {
                    if (IsHighlight && lineLength == 0)
                        line = " ";

                    yield return $"{(settings.HideLikes && i + 1 == lines.Length ? openHighlightBorderBottom : openHighlightBorderMiddle)}<span class='{(IsReply ? "reply" : "comment")}{(isLineRTL ? $" rtl width-{lineCh}" : null)}'{(settings.ShowCopyLinks ? $" data-id='{id}'" : null)}>{line.ToHtml(settings.URL, websiteInfo.HashtagURL, websiteInfo.LowerHashtag, checkForRepliedAuthor: i == 0, allAuthors)}</span>{closeHighlightBorder}";
                }
                else
                {
                    if (IsHighlight)
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
                    string[] innerLines = line.SplitToHtmlLines(lineCh, settings.URL, websiteInfo.HashtagURL, websiteInfo.LowerHashtag, checkForRepliedAuthor: i == 0, allAuthors);
                    for (int k = 0; k < innerLines.Length; k++)
                    {
                        string innerLine = (innerLines[k] ?? string.Empty).Trim();

                        if (IsHighlight && innerLine.Length == 0)
                            innerLine = " ";

                        yield return $"{(settings.HideLikes && i + 1 == lines.Length && k + 1 == innerLines.Length ? openHighlightBorderBottom : openHighlightBorderMiddle)}<span class='{(IsReply ? "reply" : "comment")}{(isLineRTL ? $" rtl width-{lineCh}" : null)}'{(settings.ShowCopyLinks ? $" data-id='{id}'" : null)}>{innerLine}</span>{closeHighlightBorder}";
                    }
                }
                else
                {
                    string[] innerLines = line.SplitToLines(lineCh);
                    for (int k = 0; k < innerLines.Length; k++)
                    {
                        string innerLine = (innerLines[k] ?? string.Empty).Trim();
                        if (IsHighlight)
                        {
                            int diff = lineCh - innerLine.GetTextLengthForTextFile();
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

        if (settings.HideLikes == false)
        {
            string line = $"{LikeCount} Like{(LikeCount == 1 ? string.Empty : "s")}";

            if (writeToHTML)
            {
                yield return $"{openHighlightBorderBottom}<span class='likes'>{HttpUtility.HtmlEncode(line)}</span>{closeHighlightBorder}";
            }
            else
            {
                if (IsHighlight)
                {
                    int diff = lineCh - line.GetTextLengthForTextFile();
                    yield return $"│ {line}{(diff > 0 ? new string(' ', diff) : null)} │";
                }
                else
                {
                    yield return line;
                }
            }
        }

        if (writeToHTML == false && IsHighlight)
            yield return $"{(Count > 0 && isFavorited == false ? '├' : '└')}{new string('─', widthCh - 2)}┘";

        if (isFavorited)
        {
            if (writeToHTML)
                yield return $"<span class='favorited'><span class='heart'>{HttpUtility.HtmlEncode("❤")}</span>{HttpUtility.HtmlEncode($" by {uploaderId}")}</span>";
            else
                yield return $"♥ by {uploaderId}";
        }
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
            int index = repliedAuthor.Index + repliedAuthor.Name.Length;
            string line1 = lines[0][..index].TrimEnd();
            string line2 = lines[0][index..].TrimStart();
            if (string.IsNullOrWhiteSpace(line2) == false)
                lines = [line1, line2, .. lines.Skip(1)];
        }

        return lines;
    }

    public virtual bool HasRTLLine(bool writeToHTML)
    {
        return GetLines(writeToHTML).HasRTLLine();
    }

    #endregion
}
