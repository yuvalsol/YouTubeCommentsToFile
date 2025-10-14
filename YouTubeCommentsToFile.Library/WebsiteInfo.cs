namespace YouTubeCommentsToFile.Library;

internal class WebsiteInfo(string siteURL)
{
    public string SiteURL { get; set; } = siteURL;
    public string ChannelURL { get; set; } = siteURL;
    public string HashtagURL { get; set; } = siteURL;
    public bool LowerHashtag { get; set; }
    public bool IsCommentURLPath { get; set; }
    public bool IsCommentURLQueryString { get; set; }
    public string CommentURLParameter { get; set; }

    public static readonly WebsiteInfo YouTube = new("https://www.youtube.com")
    {
        ChannelURL = "https://www.youtube.com",
        HashtagURL = $"https://www.youtube.com/hashtag",
        LowerHashtag = true,
        IsCommentURLQueryString = true,
        CommentURLParameter = "lc"
    };

    public static readonly WebsiteInfo BiliBili = new("https://www.bilibili.com")
    {
        ChannelURL = "https://www.bilibili.com/space",
        HashtagURL = "https://www.bilibili.com"
    };

    public static readonly WebsiteInfo BiliBiliGlobal = new("https://www.bilibili.tv")
    {
        ChannelURL = "https://www.bilibili.tv/space",
        HashtagURL = "https://www.bilibili.tv"
    };

    public WebsiteInfo(UriBuilder uriBuilder)
        : this($"{uriBuilder.Scheme}{Uri.SchemeDelimiter}{uriBuilder.Host}")
    { }

    public override string ToString()
    {
        return SiteURL;
    }
}
