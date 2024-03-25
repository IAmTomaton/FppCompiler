using System.Linq;
using System.Xml.Linq;

namespace FppCompilerLib.SyntacticalAnalysis
{
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

        public NonTerminalNode Parse(TerminalWithValue[] terminals)
        {
            var results = ParseNonTerminal(terminals, axiom, 0);
            if (results.Length > 1)
                throw new ArgumentException("Пиздец");
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

        //private (NonTerminalNode node, int index)[] ParseRule(TerminalWithValue[] terminals, Rule rule, int index, int ruleIndex)
        //{
        //    var childs = new List<ParseNode>();
        //    for (var i = ruleIndex; i < rule.tokens.Length; i++)
        //    {
        //        var token = rule.tokens[ruleIndex];
        //        if (token is Terminal terminal)
        //        {
        //            if (terminal != terminals[index])
        //                return Array.Empty<(NonTerminalNode?, int)>();
        //            childs.Add(new TerminalNode(terminals[index]));
        //            index++;
        //        }
        //        else if (token is NonTerminal nonTerminal)
        //        {
        //            var parseNonTerminalResults = ParseNonTerminal(terminals, nonTerminal, index).ToArray();
        //            if (parseNonTerminalResults.Length == 0)
        //                return Array.Empty<(NonTerminalNode?, int)>();
        //            foreach (var result in parseNonTerminalResults)
        //            {
        //                var newIndex = result.index;
        //                var nextResults = ParseRule(terminals, rule, newIndex, i + 1);
        //            }
        //            if (node == null)
        //                return (null, 0);
        //            childs.Add(node);
        //        }
        //    }
        //    return (new NonTerminalNode(rule.source, childs.ToArray(), rule), index);
        //}

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
