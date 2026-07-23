namespace YouTubeCommentsToFile.Library;

internal class WebsiteInfo(params string[] hosts)
{
    public string[] Hosts { get; } = hosts;
    public string ChannelPath { get; init; }
    public string HashtagPath { get; init; }
    public bool IsSupportHashtagWithSpace { get; init; }
    public bool IsToLowerCaseHashtag { get; init; }
    public bool IsCommentURLQueryString { get; init; }
    public string CommentURLParameter { get; init; }
    public bool IsCommentURLPath { get; init; }
    public int? EncodingCodePage { get; set; }

    public virtual string ChannelURL => $"{ToString()}{ChannelPath}/";
    public virtual string HashtagURL => $"{ToString()}{HashtagPath}/";

    protected string host;
    public string Host => host ?? Hosts[0];

    public WebsiteInfo(UriBuilder uriBuilder) : this([uriBuilder.Host]) { }

    public virtual bool ValidateURL(UriBuilder uriBuilder, out string cleanURL)
    {
        cleanURL = null;
        if (IsHostExists(uriBuilder) == false)
            return false;

        SetHost(uriBuilder);
        cleanURL = new UriBuilder(uriBuilder.Uri) { Host = host }.Uri.ToString();
        return true;
    }

    protected virtual bool IsHostExists(UriBuilder uriBuilder)
    {
        // URL hostnames are case-insensitive
        return Hosts.HasAny(host => string.Compare(host, uriBuilder.Host, StringComparison.OrdinalIgnoreCase) == 0);
    }

    protected virtual void SetHost(UriBuilder uriBuilder)
    {
        host = uriBuilder.Host.ToLowerInvariant();
    }

    public override string ToString()
    {
        return GetHttpsURL(Host);
    }

    protected static string GetHttpsURL(string path)
    {
        return $"{Uri.UriSchemeHttps}{Uri.SchemeDelimiter}{path}";
    }
}

internal class YouTube : WebsiteInfo
{
    public YouTube() : base(
        "www.youtube.com",
        "www.youtube-nocookie.com",
        "music.youtube.com")
    {
        HashtagPath = "/hashtag";
        IsToLowerCaseHashtag = true;
        // https://www.youtube.com/watch?v=videoId&lc=commentId
        IsCommentURLQueryString = true;
        CommentURLParameter = "lc";
    }

    public override bool ValidateURL(UriBuilder uriBuilder, out string cleanURL)
    {
        cleanURL = null;
        if (IsHostExists(uriBuilder) == false)
            return false;

        if (string.IsNullOrEmpty(uriBuilder.Path))
            return false;

        // https://www.youtube.com/watch?v=id
        if (uriBuilder.Path == "/watch")
        {
            string id = null;
            if (string.IsNullOrEmpty(uriBuilder.Query) == false)
                id = HttpUtility.ParseQueryString(uriBuilder.Query)["v"];

            if (string.IsNullOrEmpty(id) == false)
            {
                SetHost(uriBuilder);
                cleanURL = GetHttpsURL($"{Host}/watch?v={id}");
                return true;
            }
        }

        // https://www.youtube.com/watch/id
        // https://www.youtube.com/shorts/id
        return
            MatchPath(uriBuilder, "/watch/id", out cleanURL) ||
            MatchPath(uriBuilder, "/shorts/id", out cleanURL);
    }

    private bool MatchPath(UriBuilder uriBuilder, string template, out string cleanURL)
    {
        cleanURL = null;
        string[] pSegments = uriBuilder.Path.Split('/');
        string[] tSegments = template.Split('/');

        if (pSegments.Length != tSegments.Length)
            return false;

        if (pSegments.SkipLast(1).Zip(tSegments.SkipLast(1)).HasAny(x => x.First != x.Second))
            return false;

        string id = pSegments.Last();
        if (string.IsNullOrEmpty(id))
            return false;

        SetHost(uriBuilder);
        cleanURL = GetHttpsURL($"{Host}/watch?v={id}");
        return true;
    }
}

internal class BiliBili : WebsiteInfo
{
    public BiliBili() : base(
        "www.bilibili.com",
        "live.bilibili.com",
        "www.bilibili.tv",
        "live.bilibili.tv",
        "b23.tv")
    {
        ChannelPath = "/space";
        HashtagPath = "/search-result?q=";
        IsSupportHashtagWithSpace = true;
        EncodingCodePage = GetEncodingCodePage(nameof(Settings.ChineseSimplified));
    }

    protected override void SetHost(UriBuilder uriBuilder)
    {
        base.SetHost(uriBuilder);
        if (Host.EndsWith(".bilibili.tv"))
            EncodingCodePage = null;
    }

    public override string ChannelURL
    {
        get
        {
            if (Host.EndsWith("bilibili.com"))
                return GetHttpsURL("space.bilibili.com/");
            return base.ChannelURL;
        }
    }

    public override string HashtagURL
    {
        get
        {
            if (Host.EndsWith("bilibili.com"))
                return GetHttpsURL("search.bilibili.com/all?keyword=");
            return base.HashtagURL;
        }
    }

    public override bool ValidateURL(UriBuilder uriBuilder, out string cleanURL)
    {
        cleanURL = null;
        if (IsHostExists(uriBuilder) == false)
            return false;

        if (string.IsNullOrEmpty(uriBuilder.Path))
            return false;

        string partNumber = null;
        if (string.IsNullOrEmpty(uriBuilder.Query) == false)
            partNumber = HttpUtility.ParseQueryString(uriBuilder.Query)["p"];

        var chineseSimplified = GetEncodingCodePage(nameof(Settings.ChineseSimplified));
        var japanese = GetEncodingCodePage(nameof(Settings.Japanese));

        return
            MatchPath(uriBuilder, "/video/id", partNumber, out cleanURL) ||
            MatchPath(uriBuilder, "/play/id", partNumber, out cleanURL) ||
            MatchPath(uriBuilder, "/en/video/id", partNumber, out cleanURL, resetEncoding: true) ||
            MatchPath(uriBuilder, "/en/play/id", partNumber, out cleanURL, resetEncoding: true) ||
            MatchPath(uriBuilder, "/ch/video/id", partNumber, out cleanURL, encodingCodePage: chineseSimplified) ||
            MatchPath(uriBuilder, "/ch/play/id", partNumber, out cleanURL, encodingCodePage: chineseSimplified) ||
            MatchPath(uriBuilder, "/bangumi/video/id", partNumber, out cleanURL, encodingCodePage: japanese) ||
            MatchPath(uriBuilder, "/bangumi/play/id", partNumber, out cleanURL, encodingCodePage: japanese);
    }

    private bool MatchPath(UriBuilder uriBuilder, string template, string partNumber, out string cleanURL, bool resetEncoding = false, int? encodingCodePage = null)
    {
        // /?p=partNumber
        bool hasPartNumber = string.IsNullOrEmpty(partNumber) == false;
        if (hasPartNumber)
            template += "/";

        cleanURL = null;
        string[] pSegments = uriBuilder.Path.Split('/');
        string[] tSegments = template.Split('/');

        if (pSegments.Length != tSegments.Length)
            return false;

        if (pSegments.SkipLast(hasPartNumber ? 2 : 1).Zip(tSegments.SkipLast(hasPartNumber ? 2 : 1)).HasAny(x => x.First != x.Second))
            return false;

        string id = null;
        if (hasPartNumber)
            id = pSegments.TakeLast(2).First();
        else
            id = pSegments.Last();

        if (string.IsNullOrEmpty(id))
            return false;

        SetHost(uriBuilder);
        cleanURL = GetHttpsURL($"{Host}{uriBuilder.Path}{(hasPartNumber ? $"?p={partNumber}" : null)}");

        if (resetEncoding)
            EncodingCodePage = null;
        else if (encodingCodePage != null)
            EncodingCodePage = encodingCodePage;

        return true;
    }

    private static int? GetEncodingCodePage(string name)
    {
        return typeof(Settings)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
            .Where(getter => getter.Name == name)
            .Select(getter => getter.GetCustomAttribute<CodePageAttribute>())
            .Where(attr => attr != null)
            .Select(attr => attr.CodePage)
            .FirstOrDefault();
    }
}