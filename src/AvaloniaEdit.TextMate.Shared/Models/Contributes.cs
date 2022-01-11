using Newtonsoft.Json;
using System.Collections.Generic;

namespace AvaloniaEdit.TextMate.Models
{
    public class Contributes
    {
        [JsonProperty("languages")]
        public List<Language> Languages { get; set; }
        [JsonProperty("grammars")]
        public List<Grammar> Grammars { get; set; }
        [JsonProperty("snippets")]
        public List<Snippet> Snippets { get; set; }
    }
}
