using AvaloniaEdit.TextMate.Models.Abstractions;
using AvaloniaEdit.TextMate.Shared.Models.Abstractions;
using Newtonsoft.Json;

namespace AvaloniaEdit.TextMate.Models
{
    public class GrammarDefinition : IGrammarDefinition
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("publisher")]
        public string Publisher { get; set; }
        [JsonProperty("license")]
        public string License { get; set; }
        [JsonProperty("engines")]
        public Engine Engine { get; set; }
        [JsonProperty("scripts")]
        public Script Script { get; set; }
        [JsonProperty("contributes")]
        public Contributes Contributes { get; set; }
        [JsonProperty("repository")]
        public Repository Repository { get; set; }
    }
}
