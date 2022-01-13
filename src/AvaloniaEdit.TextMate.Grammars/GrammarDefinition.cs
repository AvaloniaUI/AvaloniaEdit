using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace AvaloniaEdit.TextMate
{
    public class Engines
    {
        [JsonProperty("engines")]
        public string VsCode { get; set; }
    }

    public class Scripts
    {
        [JsonProperty("update-grammar")]
        public string UpdateGrammar { get; set; }
    }

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

    public class Grammar
    {
        [JsonProperty("language")]
        public string Language { get; set; }
        [JsonProperty("scopeName")]
        public string ScopeName { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
    }

    public class Snippet
    {
        [JsonProperty("language")]
        public string Language { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
    }

    public class Contributes
    {
        [JsonProperty("languages")]
        public List<Language> Languages { get; set; }
        [JsonProperty("grammars")]
        public List<Grammar> Grammars { get; set; }
        [JsonProperty("snippets")]
        public List<Snippet> Snippets { get; set; }
    }

    public class Repository
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class GrammarDefinition
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
