using Newtonsoft.Json;

namespace ClipHunta2.StreamLink;

public   class Headers
{
    [JsonProperty("User-Agent", NullValueHandling = NullValueHandling.Ignore)]
    public string UserAgent { get; set; }

    [JsonProperty("Accept-Encoding", NullValueHandling = NullValueHandling.Ignore)]
    public string AcceptEncoding { get; set; }

    [JsonProperty("Accept", NullValueHandling = NullValueHandling.Ignore)]
    public string Accept { get; set; }

    [JsonProperty("Connection", NullValueHandling = NullValueHandling.Ignore)]
    public string Connection { get; set; }

    [JsonProperty("referer", NullValueHandling = NullValueHandling.Ignore)]
    public Uri Referer { get; set; }

    [JsonProperty("origin", NullValueHandling = NullValueHandling.Ignore)]
    public Uri Origin { get; set; }
}