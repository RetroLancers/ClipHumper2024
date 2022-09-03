using Newtonsoft.Json;

namespace ClipHunta2.StreamLink;

public   class StreamLinkResult
{
    [JsonProperty("plugin", NullValueHandling = NullValueHandling.Ignore)]
    public string Plugin { get; set; }

    [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
    public Metadata Metadata { get; set; }

    [JsonProperty("streams", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, Stream> Streams { get; set; }
}