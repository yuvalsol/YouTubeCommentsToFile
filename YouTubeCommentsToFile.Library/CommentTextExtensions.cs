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

    public static string ToHtml(this string text, string url, string hashtagURL, bool lowerHashtag, bool checkForRepliedAuthor, List<string> allAuthors)
    {
        List<Placeholder> placeholders = text.GetHtmlPlaceholders(url, checkForRepliedAuthor, allAuthors);

        if (placeholders.IsNullOrEmpty())
            return HttpUtility.HtmlEncode(text);

        var sb = new StringBuilder(text);

        ApplyHtmlPlaceholders(sb, placeholders);

        return ReplaceHtmlPlaceholders(sb, url, hashtagURL, lowerHashtag, placeholders);
    }

    public static string[] SplitToHtmlLines(this string text, int lineCh, string url, string hashtagURL, bool lowerHashtag, bool checkForRepliedAuthor, List<string> allAuthors)
    {
        List<Placeholder> placeholders = text.GetHtmlPlaceholders(url, checkForRepliedAuthor, allAuthors);

        if (placeholders.IsNullOrEmpty())
            return [.. text.SplitToLines(lineCh).Select(HttpUtility.HtmlEncode)];

        var sb = text.SplitTextToLines(lineCh, placeholders);

        ApplyHtmlPlaceholders(sb, placeholders);

        return ReplaceHtmlPlaceholders(sb, url, hashtagURL, lowerHashtag, placeholders).Split('\n');
    }

    private static List<Placeholder> GetHtmlPlaceholders(this string text, string url, bool checkForRepliedAuthor, List<string> allAuthors)
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
        if (hasOtherAuthors)
        {
            if (hasRepliedAuthor)
            {
                otherAuthors = otherAuthors.Except([repliedAuthor]);
                hasOtherAuthors = otherAuthors.HasAny();
            }
            else if (checkForRepliedAuthor == false)
            {
                if (text.GetRepliedAuthor(out repliedAuthor))
                {
                    otherAuthors = otherAuthors.Except([repliedAuthor]);
                    hasOtherAuthors = otherAuthors.HasAny();
                }
            }
        }

        bool hasLink = text.HasLink();

        bool hasHashtag = text.HasHashtag();

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
                ToIndex = repliedAuthor.Index + repliedAuthor.Name.Length,
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
                    ToIndex = otherAuthor.Index + otherAuthor.Name.Length,
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
                text.GetHashtags()
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

        // some user names look like link
        if (hasLink && (hasRepliedAuthor || hasOtherAuthors))
        {
            var authors = placeholders
                .Where(item => item.IsRepliedAuthor || item.IsOtherAuthor)
                .ToArray();

            var notLinks = placeholders
                .Where(item => item.IsLink)
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
                placeholders.RemoveAll(item => item.IsLink && notLinks.Contains(item));
        }

        return placeholders;
    }

    private static void ApplyHtmlPlaceholders(StringBuilder sb, List<Placeholder> placeholders)
    {
        foreach (var item in placeholders)
            sb.Insert(item.ToIndex, item.ToGuid).Insert(item.FromIndex, item.FromGuid);
    }

    private static string ReplaceHtmlPlaceholders(StringBuilder sb, string url, string hashtagURL, bool lowerHashtag, List<Placeholder> placeholders)
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
                replacementPrefix = "<span class='replied-author'>";
                replacementSuffix = "</span>";
            }
            else if (item.IsOtherAuthor)
            {
                replacementPrefix = "<span class='other-author'>";
                replacementSuffix = "</span>";
            }
            else if (item.IsLink)
            {
                string value = item.Value.Replace("\n", string.Empty);
                replacementPrefix = $"<a href='{HttpUtility.HtmlAttributeEncode(value)}'>";
                replacementSuffix = "</a>";
            }
            else if (item.IsHashtag)
            {
                string value = item.Value.Replace("\n", string.Empty);
                string hashtag = value[1..];
                if (lowerHashtag)
                    hashtag = hashtag.ToLowerInvariant();
                replacementPrefix = $"<a href='{HttpUtility.HtmlAttributeEncode($"{hashtagURL}/{hashtag}")}'>";
                replacementSuffix = "</a>";
            }
            else if (item.IsTimestamp)
            {
                string value = item.Value.Replace("\n", string.Empty);
                if (TimeSpan.TryParse($"{(value.Length <= 5 /* 00:00 */ ? "00:" /* hours */ : null)}{value}", enUS, out TimeSpan ts))
                {
                    replacementPrefix = $"<a href='{HttpUtility.HtmlAttributeEncode($"{url}&t={ts.TotalSeconds}s")}'>";
                    replacementSuffix = "</a>";
                }
            }
            else if (item.IsHeart)
            {
                replacementPrefix = "<span class='heart'>";
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

    // Handle name:
    // Between 3–30 characters
    // Alphabet letters or numbers from one of 75 supported languages
    // underscores (_), hyphens (-), periods (.), Latin middle dots (·)

    // \u202C POP DIRECTIONAL FORMATTING

    [GeneratedRegex(@"(@\s*)?(?<author>@[^\s—–―‒~!@#$%^&*()=+[\]{};:'""\\|,<>/?`´‘’‚‛“”„‟\u202C]+)")]
    private static partial Regex RegexAuthor();

    [GeneratedRegex(@"^\s*(@\s*)?(?<author>@[^\s—–―‒~!@#$%^&*()=+[\]{};:'""\\|,<>/?`´‘’‚‛“”„‟\u202C]+)")]
    private static partial Regex RegexRepliedAuthor();

    public static IEnumerable<Author> GetAuthors(this string text)
    {
        if (string.IsNullOrEmpty(text))
            yield break;

        foreach (Match match in RegexAuthor().Matches(text))
        {
            var group = match.Groups["author"];
            string author = group.Value;

            if (author.HasEmoji())
                author = author[..RegexEmoji().Match(author).Index].TrimEnd();

            // Between 3–30 characters
            if (4 <= author.Length && author.Length <= 31)
                yield return new Author(author, group.Index);
        }
    }

    public static bool GetRepliedAuthor(this string text, out Author repliedAuthor)
    {
        if (string.IsNullOrEmpty(text) == false && RegexRepliedAuthor().IsMatch(text))
        {
            var group = RegexRepliedAuthor().Match(text).Groups["author"];
            string author = group.Value;

            if (author.HasEmoji())
                author = author[..RegexEmoji().Match(author).Index].TrimEnd();

            // Between 3–30 characters
            if (4 <= author.Length && author.Length <= 31)
            {
                repliedAuthor = new Author(author, group.Index);
                return true;
            }
        }

        repliedAuthor = null;
        return false;
    }

    #endregion

    #region Emoji

    // https://stackoverflow.com/questions/14527292/c-sharp-regex-to-match-emoji

    [GeneratedRegex(@"\p{So}|\p{Cs}\p{Cs}(\p{Cf}\p{Cs}\p{Cs})*")]
    private static partial Regex RegexEmoji();

    public static bool HasEmoji(this string text)
    {
        return !string.IsNullOrEmpty(text) && RegexEmoji().IsMatch(text);
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
        return !string.IsNullOrEmpty(text) && RegexLink().IsMatch(text);
    }

    public static MatchCollection GetLinks(this string text)
    {
        return RegexLink().Matches(text);
    }

    #endregion

    #region Hashtag

    [GeneratedRegex(@"(?<=^|\s)#[A-Za-z0-9_]*[A-Za-z][A-Za-z0-9_]*(?=\s|$)")]
    private static partial Regex RegexHashtag();

    public static bool HasHashtag(this string text)
    {
        return !string.IsNullOrEmpty(text) && RegexHashtag().IsMatch(text);
    }

    public static MatchCollection GetHashtags(this string text)
    {
        return RegexHashtag().Matches(text);
    }

    #endregion

    #region Timestamp

    [GeneratedRegex(@"(?<!(?:Amos|Apostles|Chronicles|Colossians|Corinthians|Daniel|Deuteronomy|Ecclesiastes|Ephesians|Esther|Exodus|Ezekiel|Ezra|Galatians|Genesis|Habakkuk|Haggai|Hebrews|Hosea|Isaiah|James|Jeremiah|Job|Joel|John|Jonah|Joshua|Jude|Judges|Kings|Lamentations|Leviticus|Luke|Malachi|Mark|Matthew|Micah|Nahum|Nehemiah|Numbers|Obadiah|Peter|Philemon|Philippians|Proverbs|Psalms|Revelation|Romans|Ruth|Samuel|Solomon|Thessalonians|Timothy|Titus|Zechariah|Zephaniah)\s*)\b(\d{1,}:)?(?:[0-5])?\d:[0-5]\d(?!\d)", RegexOptions.IgnoreCase)]
    private static partial Regex RegexTimestamp();

    public static bool HasTimestamp(this string text)
    {
        return !string.IsNullOrEmpty(text) && RegexTimestamp().IsMatch(text);
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
        return !string.IsNullOrEmpty(text) && RegexHeart().IsMatch(text);
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

    [GeneratedRegex(@"[❤❣♾❄☕✅⚠✊☮✝⚖]")]
    private static partial Regex EmojiPlusSpace();

    [GeneratedRegex(@"[\u20E3]")]
    private static partial Regex EmojiMinusSpace();

    [GeneratedRegex("\uD83C[\uDDE6-\uDDFF]")]
    private static partial Regex RegexRegionalIndicatorSymbol();

    public static int GetTextLengthForTextFile(this string text)
    {
        return text.Length
            + EmojiPlusSpace().Count(text)
            - EmojiMinusSpace().Count(text)
            - RegexRegionalIndicatorSymbol().Count(text);
    }

    #endregion

    #region Split To Lines

    public static string[] SplitToLines(this string text, int lineCh)
    {
        return text.SplitTextToLines(lineCh).ToString().Split('\n');
    }

    private static StringBuilder SplitTextToLines(this string text, int lineCh, List<Placeholder> placeholders = null)
    {
        var body = new StringBuilder(text);

        int startIndex = 0;
        while (startIndex < body.Length)
        {
            int endIndex = startIndex + lineCh - 1;

            if (endIndex >= body.Length)
                break;

            bool found = false;
            for (int i = endIndex; i >= startIndex; i--)
            {
                if (body[i] == ' ')
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

            if (found)
                continue;

            found = false;
            for (int i = endIndex; i >= startIndex; i--)
            {
                // break long url
                if (body[i] == '/' || body[i] == '?' || body[i] == '&')
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

    #endregion
}
