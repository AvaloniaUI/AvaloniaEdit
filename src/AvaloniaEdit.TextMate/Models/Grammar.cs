using Newtonsoft.Json;

namespace AvaloniaEdit.TextMate.Models
{
    public class Grammar
    {
        [JsonProperty("language")]
        public string Language { get; set; }
        [JsonProperty("scopeName")]
        public string ScopeName { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
    }
}
