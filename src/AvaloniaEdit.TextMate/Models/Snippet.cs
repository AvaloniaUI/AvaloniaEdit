using Newtonsoft.Json;

namespace AvaloniaEdit.TextMate.Models
{
    public class Snippet
    {
        [JsonProperty("language")]
        public string Language { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
    }
}
