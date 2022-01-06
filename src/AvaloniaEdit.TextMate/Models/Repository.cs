using Newtonsoft.Json;

namespace AvaloniaEdit.TextMate.Models
{
    public class Repository
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
