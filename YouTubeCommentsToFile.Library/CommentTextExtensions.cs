namespace YouTubeCommentsToFile.Library;

internal static partial class CommentTextExtensions
{
    #region Clean Text

    // \u00A0 NBSP
    [GeneratedRegex(@"[\u00A0]")]
    private static partial Regex RegexNBSP();

    // \u0091 Private Use One
    // \u0092 Private Use Two
    [GeneratedRegex(@"[`´‘’‚‛\u0091\u0092]")]
    private static partial Regex RegexQuotation();

    // \u0093 Set Transmit State
    // \u0094 Cancel Character
    [GeneratedRegex(@"[“”„‟\u0093\u0094]")]
    private static partial Regex RegexDoubleQuotation();

    // \u0096 Start Of Guarded Area
    // \u0097 End Of Guarded Area
    [GeneratedRegex(@"[—–―‒\u0096\u0097]")]
    private static partial Regex RegexDash();

    [GeneratedRegex(@"[…]")]
    private static partial Regex RegexThreeDots();

    // \u200B Zero Width Space
    // \u2060 Word Joiner
    [GeneratedRegex(@"[\u200B\u2060]+")]
    private static partial Regex RegexUnwantedChars();

    [GeneratedRegex(@"^\s*\s(?=@)")]
    private static partial Regex RegexSpacesBeforeRepliedAuthor();

    public static string CleanText(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var sb = new StringBuilder(text);

        if (RegexNBSP().IsMatch(text))
        {
            RegexNBSP().Replace(sb, text, " ");
            text = sb.ToString();
        }

        if (RegexQuotation().IsMatch(text))
        {
            RegexQuotation().Replace(sb, text, "'");
            text = sb.ToString();
        }

        if (RegexDoubleQuotation().IsMatch(text))
        {
            RegexDoubleQuotation().Replace(sb, text, "\"");
            text = sb.ToString();
        }

        if (RegexDash().IsMatch(text))
        {
            RegexDash().Replace(sb, text, "-");
            text = sb.ToString();
        }

        if (RegexThreeDots().IsMatch(text))
        {
            RegexThreeDots().Replace(sb, text, "...");
            text = sb.ToString();
        }

        if (RegexUnwantedChars().IsMatch(text))
        {
            RegexUnwantedChars().Remove(sb, text);
            text = sb.ToString();
        }

        if (RegexSpacesBeforeRepliedAuthor().IsMatch(text))
        {
            RegexSpacesBeforeRepliedAuthor().Remove(sb, text);
            text = sb.ToString();
        }

        if (text.GetRepliedAuthor(out var repliedAuthor))
        {
            if (repliedAuthor.AtSigns != null)
                text = text.Remove(repliedAuthor.AtSigns.Index, repliedAuthor.AtSigns.Length);

            if (repliedAuthor.FalseStart != null)
                text = text.Remove(repliedAuthor.FalseStart.Index, repliedAuthor.FalseStart.Length);
        }

        var authorAtSigns =
            text.GetAuthors()
            .Where(author => author.AtSigns != null)
            .Select(author => author.AtSigns)
            .OrderByDescending(atSigns => atSigns.Index);

        foreach (var atSigns in authorAtSigns)
            text = text.Remove(atSigns.Index, atSigns.Length);

        return text;
    }

    #endregion

    #region To Html

    private class Placeholder
    {
        public bool IsRepliedAuthor;
        public bool IsOtherAuthor;
        public bool IsLink;
        public bool IsHashtag;
        public bool IsTimestamp;
        public bool IsHeart;
        public int FromIndex;
        public int ToIndex;
        public string Value;
        public string FromGuid = Guid.NewGuid().ToString();
        public string ToGuid = Guid.NewGuid().ToString();
    }

    private static readonly CultureInfo enUS = new("en-US");

    public static string ToHtml(this string text, string url, string hashtagURL, bool isSupportHashtagWithSpace, bool isToLowerCaseHashtag, bool checkForRepliedAuthor, List<string> allAuthors)
    {
        List<Placeholder> placeholders = text.GetHtmlPlaceholders(url, isSupportHashtagWithSpace, checkForRepliedAuthor, allAuthors);

        if (placeholders.IsNullOrEmpty())
            return HttpUtility.HtmlEncode(text);

        var sb = new StringBuilder(text);

        ApplyHtmlPlaceholders(sb, placeholders);

        return ReplaceHtmlPlaceholders(sb, url, hashtagURL, isToLowerCaseHashtag, placeholders);
    }

    public static string[] SplitToHtmlLines(this string text, int lineCh, string url, string hashtagURL, bool isSupportHashtagWithSpace, bool isToLowerCaseHashtag, bool checkForRepliedAuthor, List<string> allAuthors)
    {
        List<Placeholder> placeholders = text.GetHtmlPlaceholders(url, isSupportHashtagWithSpace, checkForRepliedAuthor, allAuthors);

        if (placeholders.IsNullOrEmpty())
            return [.. text.SplitToLines(lineCh).Select(HttpUtility.HtmlEncode)];

        var sb = text.SplitTextToLines(lineCh, placeholders);

        ApplyHtmlPlaceholders(sb, placeholders);

        return ReplaceHtmlPlaceholders(sb, url, hashtagURL, isToLowerCaseHashtag, placeholders).Split('\n');
    }

    private static List<Placeholder> GetHtmlPlaceholders(this string text, string url, bool isSupportHashtagWithSpace, bool checkForRepliedAuthor, List<string> allAuthors)
    {
        Author repliedAuthor = null;
        bool hasRepliedAuthor =
            checkForRepliedAuthor &&
            text.GetRepliedAuthor(out repliedAuthor);

        IEnumerable<Author> otherAuthors = null;
        if (allAuthors.HasAny())
        {
            otherAuthors =
                text.GetAuthors()
                .Where(a => allAuthors.BinarySearch(a.Name, StringComparer.OrdinalIgnoreCase) >= 0);
        }

        bool hasOtherAuthors = otherAuthors.HasAny();
        if (hasOtherAuthors && hasRepliedAuthor)
        {
            otherAuthors = otherAuthors.Except([repliedAuthor]);
            hasOtherAuthors = otherAuthors.HasAny();
        }

        bool hasLink = text.HasLink();

        bool hasHashtag = text.HasHashtag(isSupportHashtagWithSpace);

        bool hasTimestamp =
            string.IsNullOrEmpty(url) == false &&
            text.HasTimestamp();

        bool hasHeart = text.HasHeart();

        if ((hasRepliedAuthor || hasOtherAuthors || hasLink || hasHashtag || hasTimestamp || hasHeart) == false)
            return null;

        List<Placeholder> placeholders = [];

        if (hasRepliedAuthor)
        {
            placeholders.Add(new Placeholder()
            {
                IsRepliedAuthor = true,
                FromIndex = repliedAuthor.Index,
                ToIndex = repliedAuthor.Index + repliedAuthor.Length,
                Value = repliedAuthor.Name
            });
        }

        if (hasOtherAuthors)
        {
            placeholders.AddRange(
                otherAuthors.Select(otherAuthor => new Placeholder()
                {
                    IsOtherAuthor = true,
                    FromIndex = otherAuthor.Index,
                    ToIndex = otherAuthor.Index + otherAuthor.Length,
                    Value = otherAuthor.Name
                })
            );
        }

        if (hasLink)
        {
            placeholders.AddRange(
                text.GetLinks()
                .Select(m => new Placeholder()
                {
                    IsLink = true,
                    FromIndex = m.Index,
                    ToIndex = m.Index + m.Length,
                    Value = m.Value
                })
            );
        }

        if (hasHashtag)
        {
            placeholders.AddRange(
                text.GetHashtags(isSupportHashtagWithSpace)
                .Select(m => new Placeholder()
                {
                    IsHashtag = true,
                    FromIndex = m.Index,
                    ToIndex = m.Index + m.Length,
                    Value = m.Value
                })
            );
        }

        if (hasTimestamp)
        {
            placeholders.AddRange(
                text.GetTimestamps()
                .Select(m => new Placeholder()
                {
                    IsTimestamp = true,
                    FromIndex = m.Index,
                    ToIndex = m.Index + m.Length,
                    Value = m.Value
                })
            );
        }

        if (hasHeart)
        {
            placeholders.AddRange(
                text.GetHearts()
                .Select(m => new Placeholder()
                {
                    IsHeart = true,
                    FromIndex = m.Index,
                    ToIndex = m.Index + m.Length,
                    Value = m.Value
                })
            );
        }

        // sort descending
        placeholders.Sort((x, y) => y.FromIndex.CompareTo(x.FromIndex));

        // a link may contain a user name
        // www.host.com\@username
        if (hasLink && (hasRepliedAuthor || hasOtherAuthors))
        {
            var links = placeholders.Where(ph => ph.IsLink);

            var notAuthors = placeholders
                .Where(ph => ph.IsRepliedAuthor || ph.IsOtherAuthor)
                .Where(author => links.Any(link =>
                    /*
                     * [link.FromIndex,         link.ToIndex)
                     *   [author.FromIndex, author.ToIndex)
                     */
                    link.FromIndex <= author.FromIndex &&
                    author.ToIndex <= link.ToIndex
                ))
                .ToArray();

            if (notAuthors.HasAny())
            {
                placeholders.RemoveAll(ph => (ph.IsRepliedAuthor || ph.IsOtherAuthor) && notAuthors.Contains(ph));
                hasRepliedAuthor = placeholders.Any(ph => ph.IsRepliedAuthor);
                hasOtherAuthors = placeholders.Any(ph => ph.IsOtherAuthor);
            }
        }

        // some user names look like a link
        if (hasLink && (hasRepliedAuthor || hasOtherAuthors))
        {
            var authors = placeholders
                .Where(ph => ph.IsRepliedAuthor || ph.IsOtherAuthor)
                .ToArray();

            var notLinks = placeholders
                .Where(ph => ph.IsLink)
                .Where(link => authors.Any(author =>
                    /*
                     * [author.FromIndex, author.ToIndex)
                     *                                  [link.FromIndex, link.ToIndex)
                     * link.FromIndex >= author.ToIndex ||
                     *                              [author.FromIndex, author.ToIndex)
                     * [link.FromIndex, link.ToIndex)
                     * author.FromIndex >= link.ToIndex
                     */
                    link.FromIndex < author.ToIndex &&
                    author.FromIndex < link.ToIndex
                ))
                .ToArray();

            if (notLinks.HasAny())
            {
                placeholders.RemoveAll(ph => ph.IsLink && notLinks.Contains(ph));
                hasLink = placeholders.Any(ph => ph.IsLink);
            }
        }

        return placeholders;
    }

    private static void ApplyHtmlPlaceholders(StringBuilder sb, List<Placeholder> placeholders)
    {
        foreach (var item in placeholders)
            sb.Insert(item.ToIndex, item.ToGuid).Insert(item.FromIndex, item.FromGuid);
    }

    private static string ReplaceHtmlPlaceholders(StringBuilder sb, string url, string hashtagURL, bool isToLowerCaseHashtag, List<Placeholder> placeholders)
    {
        var encodedLine = HttpUtility.HtmlEncode(sb.ToString());

        sb.Clear();
        sb.Append(encodedLine);

        foreach (var item in placeholders)
        {
            int index = encodedLine.IndexOf(item.FromGuid);
            int length = encodedLine.IndexOf(item.ToGuid) + item.ToGuid.Length - index;

            string replacementPrefix = null;
            string replacementSuffix = null;

            if (item.IsRepliedAuthor)
            {
                replacementPrefix = @"<span class=""replied-author"">";
                replacementSuffix = "</span>";
            }
            else if (item.IsOtherAuthor)
            {
                replacementPrefix = @"<span class=""other-author"">";
                replacementSuffix = "</span>";
            }
            else if (item.IsLink)
            {
                string value = item.Value.Replace("\n", string.Empty);
                replacementPrefix = $@"<a href=""{HttpUtility.HtmlAttributeEncode(value)}"">";
                replacementSuffix = "</a>";
            }
            else if (item.IsHashtag)
            {
                string value = item.Value.Replace("\n", string.Empty);
                string hashtag = value[1..];
                if (isToLowerCaseHashtag)
                    hashtag = hashtag.ToLowerInvariant();
                replacementPrefix = $@"<a href=""{HttpUtility.HtmlAttributeEncode($"{hashtagURL}{hashtag}")}"">";
                replacementSuffix = "</a>";
            }
            else if (item.IsTimestamp)
            {
                string value = item.Value.Replace("\n", string.Empty);
                if (TimeSpan.TryParse($"{(value.Length <= 5 /* 00:00 */ ? "00:" /* hours */ : null)}{value}", enUS, out TimeSpan ts))
                {
                    replacementPrefix = $@"<a href=""{HttpUtility.HtmlAttributeEncode($"{url}&t={ts.TotalSeconds}s")}"">";
                    replacementSuffix = "</a>";
                }
            }
            else if (item.IsHeart)
            {
                replacementPrefix = @"<span class=""heart"">";
                replacementSuffix = "</span>";
            }

            if (string.IsNullOrEmpty(replacementPrefix) == false && string.IsNullOrEmpty(replacementSuffix) == false)
            {
                string replacement = null;

                if (item.Value.Contains('\n'))
                {
                    string[] htmlValues = [.. item.Value.Split('\n').Select(HttpUtility.HtmlEncode)];
                    htmlValues[0] = $"{replacementPrefix}{htmlValues[0]}";
                    htmlValues[^1] = $"{htmlValues[^1]}{replacementSuffix}";
                    replacement = string.Join('\n', htmlValues);
                }
                else
                {
                    replacement = $"{replacementPrefix}{HttpUtility.HtmlEncode(item.Value)}{replacementSuffix}";
                }

                sb.Remove(index, length);
                sb.Insert(index, replacement);
            }
        }

        return sb.ToString();
    }

    #endregion

    #region Authors

    /* Handle name:
     * Between 3–30 characters
     * Alphabet letters or numbers from one of 75 supported languages
     * underscores (_), hyphens (-), periods (.), Latin middle dots (·)
     */

    /* RegexAuthor:
     *
     * line start or at least one space: ^|\s+
     * <AtSigns>: precede with at signs, if any: @*
     * <Author>: author
     *     at sign: @
     *     3-30 chars that are not punctuation or emoji
     */
    [GeneratedRegex(@"(?<=^|\s+)(?<AtSigns>@*)(?<Author>@(?:.(?<![\s—–―‒~!@#$%^&*()=+[\]{};:'""\\|,<>/?`´‘’‚‛“”„‟\u202C]|\p{So}|\p{Cs}\p{Cs}(\p{Cf}\p{Cs}\p{Cs})*)){3,30})")]
    private static partial Regex RegexAuthor();

    /* RegexRepliedAuthor:
     *
     * first pattern: replied author with false start
     *     line start: ^
     *     precede with spaces, if any: \s*
     *     <FalseStart>: at least one at sign followed by 1-3 chars that are not punctuation or emoji followed by at least one space
     *         at least one at sign: @+
     *         <Start>: 1-3 chars that are not
     *             punctuation: [\s—–―‒~!@#$%^&*()=+[\]{};:'""\\|,<>/?`´‘’‚‛“”„‟\u202C]
     *             or
     *             emoji: \p{So}|\p{Cs}\p{Cs}(\p{Cf}\p{Cs}\p{Cs})*
     *         at least one space: \s+
     *     <AtSigns>: precede with at signs, if any: @*
     *     <RepliedAuthor>: replied author starts with 1-3 case-insensitive chars from <FalseStart>
     *         at sign: @
     *         case-insensitive <Start>: (?i:\k<Start>)
     *         3-30 chars that are not punctuation or emoji
     *
     * it is possible for replied author to be more than 30 chars, at most 3+30=33 chars
     *
     * "@U @Username": FalseStart="@U " RepliedAuthor="@Username"
     * "@us @Username": FalseStart="@us " RepliedAuthor="@Username"
     * "@@@Use @@@Username": FalseStart="@@@Use " AtSigns="@@" RepliedAuthor="@Username"
     * "@X @Username": no match. "Username" doesn't start with "X" or "x"
     *
     * second pattern: replied author without false start
     *     line start: ^
     *     precede with spaces, if any: \s*
     *     <AtSigns>: precede with at signs, if any: @*
     *     <RepliedAuthor>: replied author
     *         at sign: @
     *         3-30 chars that are not punctuation or emoji
     *
     * "@Username": RepliedAuthor="@Username"
     * "@@@Username": AtSigns="@@" RepliedAuthor="@Username"
     * "@X @Username": no match. "X" is only 1 char. the minimum is 3 chars
     * "@Xyz @Username": RepliedAuthor="@Xyz"
     */
    [GeneratedRegex(@"^\s*(?<FalseStart>@+(?<Start>(?:.(?<![\s—–―‒~!@#$%^&*()=+[\]{};:'""\\|,<>/?`´‘’‚‛“”„‟\u202C]|\p{So}|\p{Cs}\p{Cs}(\p{Cf}\p{Cs}\p{Cs})*)){1,3})\s+)(?<AtSigns>@*)(?<RepliedAuthor>@(?i:\k<Start>)(?:.(?<![\s—–―‒~!@#$%^&*()=+[\]{};:'""\\|,<>/?`´‘’‚‛“”„‟\u202C]|\p{So}|\p{Cs}\p{Cs}(\p{Cf}\p{Cs}\p{Cs})*)){3,30})|^\s*(?<AtSigns>@*)(?<RepliedAuthor>@(?:.(?<![\s—–―‒~!@#$%^&*()=+[\]{};:'""\\|,<>/?`´‘’‚‛“”„‟\u202C]|\p{So}|\p{Cs}\p{Cs}(\p{Cf}\p{Cs}\p{Cs})*)){3,30})")]
    private static partial Regex RegexRepliedAuthor();

    public static IEnumerable<Author> GetAuthors(this string text)
    {
        if (string.IsNullOrEmpty(text))
            yield break;

        var authors =
            RegexAuthor().Matches(text)
            .Select(match => new
            {
                gAuthor = match.Groups["Author"],
                gAtSigns = match.Groups["AtSigns"]
            })
            .Select(x => new Author(x.gAuthor.Value, x.gAuthor.Index, x.gAuthor.Length)
            {
                AtSigns = (x.gAtSigns.Success && x.gAtSigns.Length > 0 ? new AtSigns(x.gAtSigns.Index, x.gAtSigns.Length) : null)
            });

        foreach (var author in authors)
            yield return author;
    }

    public static bool GetRepliedAuthor(this string text, out Author repliedAuthor)
    {
        if (string.IsNullOrEmpty(text))
        {
            repliedAuthor = null;
            return false;
        }

        var match = RegexRepliedAuthor().Match(text);
        if (match.Success == false)
        {
            repliedAuthor = null;
            return false;
        }

        var gRepliedAuthor = match.Groups["RepliedAuthor"];
        var gAtSigns = match.Groups["AtSigns"];
        var gFalseStart = match.Groups["FalseStart"];

        repliedAuthor = new Author(gRepliedAuthor.Value, gRepliedAuthor.Index, gRepliedAuthor.Length)
        {
            AtSigns = (gAtSigns.Success && gAtSigns.Length > 0 ? new AtSigns(gAtSigns.Index, gAtSigns.Length) : null),
            FalseStart = (gFalseStart.Success && gFalseStart.Length > 0 ? new FalseStart(gFalseStart.Index, gFalseStart.Length) : null)
        };

        return true;
    }

    #endregion

    #region Emoji

    // https://stackoverflow.com/questions/14527292/c-sharp-regex-to-match-emoji

    [GeneratedRegex(@"\p{So}|\p{Cs}\p{Cs}(\p{Cf}\p{Cs}\p{Cs})*")]
    private static partial Regex RegexEmoji();

    public static bool HasEmoji(this string text)
    {
        return string.IsNullOrEmpty(text) == false && RegexEmoji().IsMatch(text);
    }

    public static MatchCollection GetEmojis(this string text)
    {
        return RegexEmoji().Matches(text);
    }

    #endregion

    #region Link

    // starts with https:// or www.
    // (?:https://|http://|ftp://|www\.)
    // [A-Za-z0-9-\\@:%_\+~#=,?&./]+

    // starts with xxx.xxx.xxx/
    // (?:[A-Za-z0-9-\\@:%_\+~#=,?&]+\.)+
    // [A-Za-z0-9-\\@:%_\+~#=,?&]+/
    // [A-Za-z0-9-\\@:%_\+~#=,?&./]+

    // ends with com,gov,net,org,tv
    // (?:[A-Za-z0-9-\\@:%_\+~#=,?&]+\.)+
    // (?:com|gov|net|org|tv)

    [GeneratedRegex(@"(?<Link>(?:https://|http://|ftp://|www\.)[A-Za-z0-9-\\@:%_\+~#=,?&./]+|(?:[A-Za-z0-9-\\@:%_\+~#=,?&]+\.)+[A-Za-z0-9-\\@:%_\+~#=,?&]+/[A-Za-z0-9-\\@:%_\+~#=,?&./]+|(?:[A-Za-z0-9-\\@:%_\+~#=,?&]+\.)+(?:com|gov|net|org|tv))", RegexOptions.IgnoreCase)]
    private static partial Regex RegexLink();

    public static bool HasLink(this string text)
    {
        return string.IsNullOrEmpty(text) == false && RegexLink().IsMatch(text);
    }

    public static MatchCollection GetLinks(this string text)
    {
        return RegexLink().Matches(text);
    }

    #endregion

    #region Hashtag

    /* RegexHashtag:
     *
     * #word1 word2# (BiliBili)
     * starts and ends with #
     * at least one space between
     * first and last letters are not a space
     * 
     * or
     * 
     * #word
     * starts with #
     * at least one letter (not digit or underscore)
     */
    [GeneratedRegex(@"(?<=^|\s)#\w[\w\s]*\s[\w\s]*\w#|(?<=^|\s)#\w*[\w-[\d_]]\w*(?=\s|$)")]
    private static partial Regex RegexHashtag();

    [GeneratedRegex(@"(?<=^|\s)#\w*[\w-[\d_]]\w*(?=\s|$)")]
    private static partial Regex RegexSingleWordHashtag();

    public static bool HasHashtag(this string text, bool isSupportHashtagWithSpace)
    {
        return
            string.IsNullOrEmpty(text) == false &&
            (isSupportHashtagWithSpace ? RegexHashtag() : RegexSingleWordHashtag()).IsMatch(text);
    }

    public static MatchCollection GetHashtags(this string text, bool isSupportHashtagWithSpace)
    {
        return (isSupportHashtagWithSpace ? RegexHashtag() : RegexSingleWordHashtag()).Matches(text);
    }

    #endregion

    #region Timestamp

    [GeneratedRegex(@"(?<!(?:Amos|Apostles|Chronicles|Colossians|Corinthians|Daniel|Deuteronomy|Ecclesiastes|Ephesians|Esther|Exodus|Ezekiel|Ezra|Galatians|Genesis|Habakkuk|Haggai|Hebrews|Hosea|Isaiah|James|Jeremiah|Job|Joel|John|Jonah|Joshua|Jude|Judges|Kings|Lamentations|Leviticus|Luke|Malachi|Mark|Matthew|Micah|Nahum|Nehemiah|Numbers|Obadiah|Peter|Philemon|Philippians|Proverbs|Psalms|Revelation|Romans|Ruth|Samuel|Solomon|Thessalonians|Timothy|Titus|Zechariah|Zephaniah)\s*)\b(\d{1,}:)?(?:[0-5])?\d:[0-5]\d(?!\d|\s*[ap]\.?m\.?)", RegexOptions.IgnoreCase)]
    private static partial Regex RegexTimestamp();

    public static bool HasTimestamp(this string text)
    {
        return string.IsNullOrEmpty(text) == false && RegexTimestamp().IsMatch(text);
    }

    public static MatchCollection GetTimestamps(this string text)
    {
        return RegexTimestamp().Matches(text);
    }

    #endregion

    #region Heart

    [GeneratedRegex(@"[♥❤❣]+")]
    private static partial Regex RegexHeart();

    public static bool HasHeart(this string text)
    {
        return string.IsNullOrEmpty(text) == false && RegexHeart().IsMatch(text);
    }

    public static MatchCollection GetHearts(this string text)
    {
        return RegexHeart().Matches(text);
    }

    #endregion

    #region Text For Text File

    [GeneratedRegex("\uD83C[\uDFFB-\uDFFF]")]
    private static partial Regex RegexEmojiModifierFitzpatrickType1To6();

    [GeneratedRegex("[\uFE0E\uFE0F]")]
    private static partial Regex RegexVariationSelector();

    [GeneratedRegex("[\u200D]")]
    private static partial Regex RegexZeroWidthJoiner();

    public static string CleanTextForTextFile(this string text)
    {
        return
            RegexZeroWidthJoiner().Replace(
                RegexVariationSelector().Replace(
                    RegexEmojiModifierFitzpatrickType1To6().Replace(text, string.Empty), string.Empty), string.Empty);
    }

    #endregion

    #region Split To Lines

    public static string[] SplitToLines(this string text, int lineCh)
    {
        return text.SplitTextToLines(lineCh).ToString().Split('\n');
    }

    [GeneratedRegex(@"[/?!&)。，、）？！]")]
    private static partial Regex RegexLineSplitChars();

    [GeneratedRegex(@"\n")]
    private static partial Regex RegexNewLine();

    private static StringBuilder SplitTextToLines(this string text, int lineCh, List<Placeholder> placeholders = null, bool skipLastMeasurements = false)
    {
        var body = new StringBuilder(text);

        int startIndex = 0;
        while (startIndex < body.Length)
        {
            int endIndex = startIndex + lineCh - 1;

            if (endIndex >= body.Length)
            {
                if (skipLastMeasurements == false)
                {
                    string line = body.ToString(startIndex, body.Length - startIndex);
                    int lineLength = line.MeasureText();

                    if (lineLength > line.Length)
                    {
                        const int STEP_CH = 10;
                        int lineChRecalculte = lineCh - STEP_CH;
                        for (; CommentsWriter.TEXT_LINE_MIN_LENGTH <= lineChRecalculte && lineChRecalculte > lineLength; lineChRecalculte -= STEP_CH) ;

                        for (; CommentsWriter.TEXT_LINE_MIN_LENGTH <= lineChRecalculte && lineChRecalculte <= lineLength; lineChRecalculte -= STEP_CH)
                        {
                            StringBuilder sb = line.SplitTextToLines(lineChRecalculte, skipLastMeasurements: true);
                            string lineRecalculte = sb.ToString();
                            if (lineRecalculte.Contains('\n') && lineRecalculte.Split('\n').All(l => l.MeasureText() <= lineCh))
                            {
                                foreach (var newLineIndex in RegexNewLine().Matches(lineRecalculte).Select(m => startIndex + m.Index).OrderDescending())
                                {
                                    body.Insert(newLineIndex, '\n');

                                    if (placeholders.HasAny())
                                    {
                                        // placeholders are sort by FromIndex desc
                                        // nlIndex < FromIndex < ToIndex
                                        // nlIndex = FromIndex < ToIndex - intersects
                                        // FromIndex < nlIndex < ToIndex - intersects
                                        // FromIndex < ToIndex <= nlIndex
                                        int nlIndex = newLineIndex;
                                        foreach (var ph in placeholders.TakeWhile(ph => nlIndex <= ph.FromIndex || nlIndex < ph.ToIndex))
                                        {
                                            if (nlIndex <= ph.FromIndex)
                                            {
                                                ph.FromIndex += 1;
                                                ph.ToIndex += 1;
                                            }
                                            else if (nlIndex < ph.ToIndex)
                                            {
                                                ph.ToIndex += 1;
                                                ph.Value = body.ToString(ph.FromIndex, ph.ToIndex - ph.FromIndex);
                                            }
                                        }
                                    }
                                }

                                break;
                            }
                        }
                    }
                }

                break;
            }

            if (endIndex >= body.Length)
                break;

            bool found = false;
            for (int i = endIndex; i >= startIndex; i--)
            {
                if (body[i] == ' ')
                {
                    if (body.ToString(startIndex, i - startIndex).MeasureText() <= lineCh)
                    {
                        body[i] = '\n';
                        startIndex = i + 1;
                        found = true;

                        if (placeholders.HasAny())
                        {
                            // placeholders are sort by FromIndex desc
                            // nlIndex < FromIndex < ToIndex
                            // nlIndex = FromIndex < ToIndex - intersects
                            // FromIndex < nlIndex < ToIndex - intersects
                            // FromIndex < ToIndex <= nlIndex
                            int nlIndex = i;
                            foreach (var ph in placeholders.Where(ph => ph.FromIndex <= nlIndex && nlIndex < ph.ToIndex))
                                ph.Value = body.ToString(ph.FromIndex, ph.ToIndex - ph.FromIndex);
                        }

                        break;
                    }
                }
            }

            if (found)
                continue;

            found = false;
            for (int i = endIndex; i >= startIndex; i--)
            {
                // break long url
                if (RegexLineSplitChars().IsMatch(body[i].ToString()))
                {
                    if (body.ToString(startIndex, i - startIndex + 1).MeasureText() <= lineCh)
                    {
                        body.Insert(i + 1, '\n');
                        startIndex = i + 2;
                        found = true;

                        if (placeholders.HasAny())
                        {
                            // placeholders are sort by FromIndex desc
                            // nlIndex < FromIndex < ToIndex
                            // nlIndex = FromIndex < ToIndex - intersects
                            // FromIndex < nlIndex < ToIndex - intersects
                            // FromIndex < ToIndex <= nlIndex
                            int nlIndex = i + 1;
                            foreach (var ph in placeholders.TakeWhile(ph => nlIndex <= ph.FromIndex || nlIndex < ph.ToIndex))
                            {
                                if (nlIndex <= ph.FromIndex)
                                {
                                    ph.FromIndex += 1;
                                    ph.ToIndex += 1;
                                }
                                else if (nlIndex < ph.ToIndex)
                                {
                                    ph.ToIndex += 1;
                                    ph.Value = body.ToString(ph.FromIndex, ph.ToIndex - ph.FromIndex);
                                }
                            }
                        }

                        break;
                    }
                }
            }

            if (found)
                continue;

            if (endIndex + 1 <= body.Length)
            {
                body.Insert(endIndex + 1, '\n');
                startIndex = endIndex + 2;

                if (placeholders.HasAny())
                {
                    // placeholders are sort by FromIndex desc
                    // nlIndex < FromIndex < ToIndex
                    // nlIndex = FromIndex < ToIndex - intersects
                    // FromIndex < nlIndex < ToIndex - intersects
                    // FromIndex < ToIndex <= nlIndex
                    int nlIndex = endIndex + 1;
                    foreach (var ph in placeholders.TakeWhile(ph => nlIndex <= ph.FromIndex || nlIndex < ph.ToIndex))
                    {
                        if (nlIndex <= ph.FromIndex)
                        {
                            ph.FromIndex += 1;
                            ph.ToIndex += 1;
                        }
                        else if (nlIndex < ph.ToIndex)
                        {
                            ph.ToIndex += 1;
                            ph.Value = body.ToString(ph.FromIndex, ph.ToIndex - ph.FromIndex);
                        }
                    }
                }
            }
        }

        if (body.Length > 0 && body[^1] == '\n')
        {
            body.Remove(body.Length - 1, 1);

            if (placeholders.HasAny())
            {
                // placeholders are sort by FromIndex desc
                // nlIndex < FromIndex < ToIndex
                // nlIndex = FromIndex < ToIndex - intersects
                // FromIndex < nlIndex < ToIndex - intersects
                // FromIndex < ToIndex <= nlIndex
                int nlIndex = body.Length; // = (body.Length + 1) - 1: +1 to componsate for body.Remove()
                foreach (var ph in placeholders.TakeWhile(ph => nlIndex <= ph.FromIndex || nlIndex < ph.ToIndex))
                {
                    if (nlIndex <= ph.FromIndex)
                    {
                        ph.FromIndex -= 1;
                        ph.ToIndex -= 1;
                    }
                    else if (nlIndex < ph.ToIndex)
                    {
                        ph.ToIndex -= 1;
                        ph.Value = body.ToString(ph.FromIndex, ph.ToIndex - ph.FromIndex);
                    }
                }
            }
        }

        return body;
    }

    [GeneratedRegex(@"\p{IsHangulJamo}|\p{IsCJKRadicalsSupplement}|\p{IsCJKSymbolsandPunctuation}|\p{IsHiragana}|\p{IsKatakana}|\p{IsEnclosedCJKLettersandMonths}|\p{IsCJKCompatibility}|\p{IsCJKUnifiedIdeographsExtensionA}|\p{IsCJKUnifiedIdeographs}|\p{IsHangulSyllables}|\p{IsCJKCompatibilityIdeographs}|\p{IsCJKCompatibilityForms}")]
    private static partial Regex RegexJapaneseChineseKorean();

    // full-width chars are counted twice
    public static int MeasureText(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        return
            text.Length +
            RegexJapaneseChineseKorean().Count(text) +
            RegexEmoji().Count(text);
    }

    #endregion

    #region RTL

    public static bool HasRTLLine(this string[] lines)
    {
        return lines.HasAny(IsLineRTL);
    }

    public static List<(int start, int end)> GetRTLBlocks(this string[] lines)
    {
        var rtlBlocks = lines
            .Select((line, i) => (line, i))
            .Where(x => IsLineRTL(x.line))
            .Select(x => x.i)
            .Select((val, index) => new { val, groupKey = val - index })
            .GroupBy(x => x.groupKey)
            .Select(g => (start: g.First().val, end: g.Last().val))
            .ToList();

        if (rtlBlocks.HasMoreThanOne() == false)
            return rtlBlocks;

        for (int i = 1; i < rtlBlocks.Count; i++)
        {
            // start0 <= end0 < start1 <= end1
            var (start0, end0) = rtlBlocks[i - 1];
            var (start1, end1) = rtlBlocks[i];
            int startIndex = end0 + 1;
            int endIndex = start1 - 1;
            int count = endIndex - startIndex + 1;
            if (lines.Skip(startIndex).Take(count).All(string.IsNullOrWhiteSpace))
            {
                rtlBlocks.RemoveRange(i - 1, 2);
                rtlBlocks.Insert(i - 1, (start0, end1));
                i--;
            }
        }

        return rtlBlocks;
    }

    [GeneratedRegex(@"\p{IsHebrew}|\p{IsArabic}|\p{IsArabicPresentationForms-A}|\p{IsArabicPresentationForms-B}|\p{IsSyriac}")]
    private static partial Regex RegexRTLLangs();

    [GeneratedRegex(@"[0-9]+")]
    private static partial Regex RegexDigits();

    [GeneratedRegex(@"[-—–―‒_~!@#$%^&*()=+[\]{};:'""\\|,.<>/?`´‘’‚‛“”„‟]+")]
    private static partial Regex RegexPunctuations();

    [GeneratedRegex(@"\s+")]
    private static partial Regex RegexSpace();

    private static bool IsLineRTL(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return false;

        if (RegexRTLLangs().IsMatch(line) == false)
            return false;

        line = RegexDigits().Replace(line, string.Empty);
        line = RegexPunctuations().Replace(line, string.Empty);
        line = RegexEmoji().Replace(line, string.Empty);
        line = RegexSpace().Replace(line, string.Empty);

        int countRTL = RegexRTLLangs().Count(line);
        line = RegexRTLLangs().Replace(line, string.Empty);
        int countLTR = line.Length;

        return 1.0 * countLTR / (countLTR + countRTL) < 0.5;
    }

    #endregion
}
