using Newtonsoft.Json;

namespace ClipHunta2.StreamLink;

public   class Stream
{
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public string Type { get; set; }

    [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
    public Uri Url { get; set; }

    [JsonProperty("headers", NullValueHandling = NullValueHandling.Ignore)]
    public Headers Headers { get; set; }

    [JsonProperty("master", NullValueHandling = NullValueHandling.Ignore)]
    public Uri Master { get; set; }
}