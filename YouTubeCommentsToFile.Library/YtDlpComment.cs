using Newtonsoft.Json;

namespace YouTubeCommentsToFile.Library;

[JsonObject(
    MemberSerialization = MemberSerialization.OptIn,
    ItemNullValueHandling = NullValueHandling.Ignore
)]
internal class YtDlpComment : Comment
{
    [JsonProperty("id")]
    public override string Id { get; set; }

    [JsonProperty("parent")]
    public override string Parent { get; set; }

    [JsonProperty("text")]
    [JsonConverter(typeof(CleanTextJsonConverter))]
    public override string Text { get; set; }

    [JsonProperty("author")]
    public override string Author { get; set; }

    [JsonProperty("author_is_uploader")]
    public override bool AuthorIsUploader { get; set; }

    [JsonProperty("like_count")]
    protected override int LikeCount { get; set; }

    [JsonProperty("author_url")]
    protected override string AuthorURL { get; set; }

    [JsonProperty("is_favorited")]
    protected override bool IsFavorited { get; set; }

    [JsonProperty("_time_text")]
    protected override string TimeText { get; set; }

    [JsonProperty("is_pinned")]
    protected override bool IsPinned { get; set; }

    public override bool IsTopLevelComment => base.IsTopLevelComment || Parent == "root";

    protected override string MissingAuthor => "Author Missing";
}
