using System;
using System.Collections.Generic;
using System.Text;
using TextMateSharp.Internal.Types;

namespace AvaloniaEdit.TextMate.Storage.Abstractions
{
    public interface IGrammarStorage
    {
        public Dictionary<string, IRawGrammar> Grammars { get; set; }
        public IRawGrammar SelectedGrammar { get; set; }
    }
}
