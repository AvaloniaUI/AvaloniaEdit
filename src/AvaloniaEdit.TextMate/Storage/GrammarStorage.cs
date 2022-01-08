using AvaloniaEdit.TextMate.Models.Abstractions;
using AvaloniaEdit.TextMate.Storage.Abstractions;
using System.Collections.Generic;
using TextMateSharp.Internal.Types;

namespace AvaloniaEdit.TextMate.Storage
{
    public class GrammarStorage : IGrammarStorage
    {
        public GrammarStorage(Dictionary<string, IRawGrammar> grammars, IRawGrammar selectedGrammar, List<IGrammarDefinition> grammarDefinitions)
        {
            Grammars = grammars;
            SelectedGrammar = selectedGrammar;
            GrammarDefinitions = grammarDefinitions;
        }
        public Dictionary<string, IRawGrammar> Grammars { get; set; }
        public IRawGrammar SelectedGrammar { get; set; }
        public List<IGrammarDefinition> GrammarDefinitions { get; set; }
    }
}
