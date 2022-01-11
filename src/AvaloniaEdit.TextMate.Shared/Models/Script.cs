using Newtonsoft.Json;

namespace AvaloniaEdit.TextMate.Models
{
    public class Script
    {
        [JsonProperty("update-grammar")]
        public string UpdateGrammar { get; set; }
    }
}
