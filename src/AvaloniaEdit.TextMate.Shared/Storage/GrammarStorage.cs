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
        /// <summary>
        /// Dictionary where the key is scope name and the value is IRawGrammar.
        /// IRawGrammar is created from *.tmLanguage.json file.
        /// </summary>
        public Dictionary<string, IRawGrammar> Grammars { get; set; }
        public IRawGrammar SelectedGrammar { get; set; }

        /// <summary>
        /// IGrammarDefinition is generated from package.json file.
        /// </summary>
        public List<IGrammarDefinition> GrammarDefinitions { get; set; }
    }
}
