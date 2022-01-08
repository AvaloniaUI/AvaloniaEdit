using AvaloniaEdit.TextMate.Models.Abstractions;
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
        public Engines Engines { get; set; }
        [JsonProperty("scripts")]
        public Scripts Scripts { get; set; }
        [JsonProperty("contributes")]
        public Contributes Contributes { get; set; }
        [JsonProperty("repository")]
        public Repository Repository { get; set; }
    }
}
