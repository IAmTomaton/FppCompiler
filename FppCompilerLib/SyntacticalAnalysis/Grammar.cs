namespace FppCompilerLib.SyntacticalAnalysis
{
    internal class Grammar
    {
        public readonly NonTerminal Axiom;

        private readonly Dictionary<NonTerminal, List<Rule>> grammar = new();

        public Grammar(NonTerminal axiom)
        {
            Axiom = axiom;
        }

        public void AddRule(Rule rule)
        {
            if (!grammar.ContainsKey(rule.source))
                grammar.Add(rule.source, new List<Rule>());
            grammar[rule.source].Add(rule);
        }

        public List<Rule> GetRules(NonTerminal source) => grammar[source];

        public IEnumerable<Rule> GetAllRules() => grammar.Values.Aggregate<IEnumerable<Rule>>((a, b) => a.Union(b));

        public IEnumerable<NonTerminal> GetAllNonTerminals() => grammar.Keys;

        public IEnumerable<Terminal> GetAllTerminals() => grammar.Values
            .SelectMany(rules => rules)
            .SelectMany(rule => rule.tokens)
            .Where(token => token is Terminal)
            .Cast<Terminal>()
            .Distinct();
    }
}
