using Newtonsoft.Json;

namespace ClipHunta2.StreamLink;

public   class Metadata
{
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string Id { get; set; }

    [JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
    public string Author { get; set; }

    [JsonProperty("category", NullValueHandling = NullValueHandling.Ignore)]
    public string Category { get; set; }

    [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
    public string Title { get; set; }
}