using Newtonsoft.Json;

namespace YouTubeCommentsToFile.Library;

internal class CleanTextJsonConverter : JsonConverter<string>
{
    public override bool CanRead => true;
    public override bool CanWrite => false;

    public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return (reader.Value as string).CleanText();
    }

    public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
