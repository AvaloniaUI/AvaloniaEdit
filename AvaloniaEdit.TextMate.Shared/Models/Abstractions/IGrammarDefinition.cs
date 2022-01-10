using AvaloniaEdit.TextMate.Shared.Models.Abstractions;

namespace AvaloniaEdit.TextMate.Models.Abstractions
{
    public interface IGrammarDefinition
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Publisher { get; set; }
        public string License { get; set; }
        public Engine Engine { get; set; }
        public Script Script { get; set; }
        public Contributes Contributes { get; set; }
        public Repository Repository { get; set; }
    }
}
