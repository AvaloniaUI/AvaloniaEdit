using AvaloniaEdit.TextMate.Models.Abstractions;
using System.Collections.Generic;
using TextMateSharp.Internal.Types;

namespace AvaloniaEdit.TextMate.Storage.Abstractions
{
    public interface IGrammarStorage
    {
        public Dictionary<string, IRawGrammar> Grammars { get; }
        public IRawGrammar SelectedGrammar { get; }
        public List<IGrammarDefinition> GrammarDefinitions { get; }
    }
}
