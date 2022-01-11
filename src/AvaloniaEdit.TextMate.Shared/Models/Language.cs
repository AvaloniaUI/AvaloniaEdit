using Newtonsoft.Json;
using System.Collections.Generic;

namespace AvaloniaEdit.TextMate.Models
{
    public class Language
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("extensions")]
        public List<string> Extensions { get; set; }
        [JsonProperty("aliases")]
        public List<string> Aliases { get; set; }
        [JsonProperty("configuration")]
        public string Configuration { get; set; }

        public override string ToString()
        {
            if (Aliases != null && Aliases.Count > 0)
                return string.Format("{0} ({1})", Aliases[0], Id);

            return Id;
        }
    }
}
