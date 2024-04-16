namespace FppCompilerLib.SyntacticalAnalysis
{
    /// <summary>
    /// 
    /// </summary>
    internal class SyntacticalAnalyzer
    {
        private readonly SelectSet selectSet;
        private readonly NonTerminal axiom;
        private readonly int k;

        public SyntacticalAnalyzer(Grammar grammar, int k)
        {
            this.k = k;
            axiom = grammar.Axiom;
            var firstSet = new FirstSet(grammar, k);
            var followSet = new FollowSet(grammar, firstSet, k);
            selectSet = new SelectSet(grammar, firstSet, followSet, k);
        }

        /// <summary>
        /// Parses an array of program tokens into a syntax tree
        /// </summary>
        /// <param name="terminals"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public NonTerminalNode Parse(TerminalWithValue[] terminals)
        {
            var results = ParseNonTerminal(terminals, axiom, 0);
            if (results.Length > 1)
                throw new ArgumentException("It is not possible to unambiguously parse the program");
            if (results.Length == 0)
                throw new ArgumentException("I can't parse this shit");
            return results.First().node;
        }

        private (NonTerminalNode node, int index)[] ParseNonTerminal(TerminalWithValue[] terminals, NonTerminal nonTerminal, int index)
        {
            var selectSeq = terminals.Append(Terminal.End).Skip(index).Take(k).ToArray();

            var rules = selectSet.GetRules(nonTerminal, selectSeq);
            var result = rules
                .SelectMany(rule => ParseRule(terminals, rule, index))
                .ToArray();
            return result;
        }

        private (NonTerminalNode node, int index)[] ParseRule(TerminalWithValue[] terminals, Rule rule, int index)
        {
            var childsSet = new List<(ParseNode[] childs, int index)>() { (Array.Empty<ParseNode>(), index) };
            for (var i = 0; i < rule.tokens.Length; i++)
            {
                var token = rule.tokens[i];
                if (token is Terminal terminal)
                {
                    var newChilddsSet = new List<(ParseNode[] childs, int index)>();
                    foreach (var childs in childsSet)
                    {
                        if (terminal == terminals[childs.index])
                            newChilddsSet.Add((childs.childs.Append(new TerminalNode(terminals[childs.index])).ToArray(), childs.index + 1));
                    }
                    childsSet = newChilddsSet;
                }
                else if (token is NonTerminal nonTerminal)
                {
                    var newChilddsSet = new List<(ParseNode[] childs, int index)>();
                    foreach (var childs in childsSet)
                    {
                        var parseResults = ParseNonTerminal(terminals, nonTerminal, childs.index).ToArray();
                        foreach (var result in parseResults)
                        {
                            newChilddsSet.Add((childs.childs.Append(result.node).ToArray(), result.index));
                        }
                    }
                    childsSet = newChilddsSet;
                }
                if (childsSet.Count == 0)
                    return Array.Empty<(NonTerminalNode, int)>();
            }
            return childsSet.Select(childs => (new NonTerminalNode(rule.source, childs.childs, rule), childs.index)).ToArray();
        }
    }
}
