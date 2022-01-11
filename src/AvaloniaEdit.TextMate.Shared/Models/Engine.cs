using Newtonsoft.Json;

namespace AvaloniaEdit.TextMate.Models
{
    public class Engine
    {
        [JsonProperty("engines")]
        public string VsCode { get; set; }
    }
}
