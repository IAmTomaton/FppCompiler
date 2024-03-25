namespace FppCompilerLib.SyntacticalAnalysis
{
    internal class FollowSet
    {
        private readonly Dictionary<NonTerminal, HashSet<Terminal[]>> followDict = new();

        private static readonly ArrayComparer<Terminal> comparer = new();

        public FollowSet(Grammar grammar, FirstSet firstSet, int k)
        {
            InitFollow(grammar);
            GenerateFollow(grammar, firstSet, k);
        }

        public HashSet<Terminal[]> GetFollowSet(NonTerminal nonTerminal) => followDict[nonTerminal].ToHashSet();

        private void InitFollow(Grammar grammar)
        {
            foreach (var nonTerminal in grammar.GetAllNonTerminals())
                followDict[nonTerminal] = new HashSet<Terminal[]>(comparer);

            followDict[grammar.Axiom].Add(new Terminal[] { Terminal.End });
        }

        private void GenerateFollow(Grammar grammar, FirstSet firstSet, int k)
        {
            var chahged = true;
            while (chahged)
            {
                chahged = false;
                foreach (var rule in grammar.GetAllRules())
                {
                    for (var i = 0; i < rule.tokens.Length; i++)
                    {
                        var token = rule.tokens[i];
                        if (token is NonTerminal nonTerminal)
                        {
                            var first = firstSet.GetFirstSet(rule.tokens.Skip(i + 1).ToArray());
                            var follow = followDict[rule.source];

                            var newFollow = new HashSet<Terminal[]>(comparer);

                            if (first.Count == 0)
                                newFollow.UnionWith(follow);
                            else if (follow.Count == 0)
                                newFollow.UnionWith(first);
                            else
                            {
                                newFollow.UnionWith(first.Where(firstSeq => firstSeq.Length == k));
                                foreach (var firstSeq in first.Where(firstSeq => firstSeq.Length < k))
                                    foreach (var followSeq in follow)
                                        newFollow.Add(firstSeq.Concat(followSeq).Take(k).ToArray());
                            }

                            var prevCount = followDict[nonTerminal].Count;
                            followDict[nonTerminal].UnionWith(newFollow);

                            if (followDict[nonTerminal].Count != prevCount)
                                chahged = true;
                        }
                    }
                }
            }
        }
    }
}
