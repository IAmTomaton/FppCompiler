namespace FppCompilerLib.SyntacticalAnalysis
{
    internal class SelectSet
    {
        private readonly Dictionary<NonTerminal, Dictionary<Terminal[], List<Rule>>> selectDict = new();
        private readonly int k;

        private static readonly ArrayComparer<Terminal> comparer = new();

        public SelectSet(Grammar grammar, FirstSet firstSet, FollowSet followSet, int k)
        {
            this.k = k;
            GenerateSelect(grammar, firstSet, followSet, k);
        }

        public List<Rule> GetRules(NonTerminal nonTerminal, Terminal[] select)
        {
            return selectDict[nonTerminal][select];
        }

        private void GenerateSelect(Grammar grammar, FirstSet firstSet, FollowSet followSet, int k)
        {
            foreach (var rule in grammar.GetAllRules())
            {
                if (!selectDict.ContainsKey(rule.source))
                    selectDict.Add(rule.source, new Dictionary<Terminal[], List<Rule>>(comparer));

                var first = firstSet.GetFirstSet(rule.tokens);
                var follow = followSet.GetFollowSet(rule.source);
                IEnumerable<Terminal[]> select;
                if (first.Count == 0)
                    select = follow;
                else if (follow.Count == 0)
                    select = first;
                else
                    select = first.Where(firstSeq => firstSeq.Length < k)
                        .SelectMany(firstSeq => follow.Select(followSeq => firstSeq.Concat(followSeq).Take(k).ToArray()))
                        .Concat(first.Where(firstSeq => firstSeq.Length == k))
                        .Distinct(comparer);
                foreach (var selectSeq in select)
                {
                    if (!selectDict[rule.source].ContainsKey(selectSeq))
                        selectDict[rule.source].Add(selectSeq, new List<Rule>());
                    selectDict[rule.source][selectSeq].Add(rule);
                }
            }
        }
    }
}
