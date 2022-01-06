using Newtonsoft.Json;

namespace AvaloniaEdit.TextMate.Models
{
    public class Scripts
    {
        [JsonProperty("update-grammar")]
        public string UpdateGrammar { get; set; }
    }
}
