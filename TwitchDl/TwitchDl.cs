using Newtonsoft.Json;

namespace ClipHunta2.TwitchDl;

public class TwitchDl
{
    public class ClipLookupResult
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("slug", NullValueHandling = NullValueHandling.Ignore)]
        public string Slug { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("viewCount", NullValueHandling = NullValueHandling.Ignore)]
        public long? ViewCount { get; set; }

        [JsonProperty("durationSeconds", NullValueHandling = NullValueHandling.Ignore)]
        public long? DurationSeconds { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }

        [JsonProperty("videoQualities", NullValueHandling = NullValueHandling.Ignore)]
        public List<VideoQuality> VideoQualities { get; set; }

        [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
        public Game Game { get; set; }

        [JsonProperty("broadcaster", NullValueHandling = NullValueHandling.Ignore)]
        public Broadcaster Broadcaster { get; set; }
    }

    public class Broadcaster
    {
        [JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }

        [JsonProperty("login", NullValueHandling = NullValueHandling.Ignore)]
        public string Login { get; set; }
    }

    public class Game
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]

        public long? Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }

    public class VideoQuality
    {
        [JsonProperty("frameRate", NullValueHandling = NullValueHandling.Ignore)]
        public long? FrameRate { get; set; }

        [JsonProperty("quality", NullValueHandling = NullValueHandling.Ignore)]

        public long? Quality { get; set; }

        [JsonProperty("sourceURL", NullValueHandling = NullValueHandling.Ignore)]
        public Uri SourceUrl { get; set; }
    }
}