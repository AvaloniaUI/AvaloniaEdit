using Newtonsoft.Json;

namespace AvaloniaEdit.TextMate.Models
{
    public class Engines
    {
        [JsonProperty("engines")]
        public string VsCode { get; set; }
    }
}
