using AvaloniaEdit.TextMate.Storage.Abstractions;
using System.Collections.Generic;
using TextMateSharp.Internal.Types;

namespace AvaloniaEdit.TextMate.Storage
{
    public class GrammarStorage : IGrammarStorage
    {
        public GrammarStorage(Dictionary<string, IRawGrammar> grammars, IRawGrammar selectedGrammar)
        {
            Grammars = grammars;
            SelectedGrammar = selectedGrammar;
        }
        public Dictionary<string, IRawGrammar> Grammars { get; set; }
        public IRawGrammar SelectedGrammar { get; set; }
    }
}
