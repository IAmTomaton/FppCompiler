namespace FppCompilerLib.SyntacticalAnalysis
{
    internal class FirstSet
    {
        private readonly Dictionary<Token, HashSet<Terminal[]>> firstDict = new();
        private readonly int k;

        private static readonly ArrayComparer<Terminal> comparer = new();

        public FirstSet(Grammar grammar, int k)
        {
            this.k = k;
            InitFirst(grammar);
            GenerateFirst(grammar);
        }

        public HashSet<Terminal[]> GetFirstSet(Token token) => firstDict[token];

        public HashSet<Terminal[]> GetFirstSet(Token[] tokens)
        {
            HashSet<Terminal[]> result = new(comparer);
            if (tokens.Length == 0)
                return result;

            foreach (var token in tokens)
            {
                var first_set = GetFirstSet(token);
                if (first_set.Count == 0)
                    return new(comparer);

                if (result.Count == 0)
                    result = first_set;
                else
                {
                    var addResult = result
                        .Where(seq => seq.Length < k)
                        .SelectMany(seq => first_set.Select(addSeq => seq.Concat(addSeq).Take(k).ToArray()))
                        .Distinct(comparer);
                    result = result
                        .Where(seq => seq.Length == k)
                        .Concat(addResult)
                        .ToHashSet(comparer);
                }

                if (result.All(seq => seq.Length == k))
                    break;
            }

            return result;
        }

        private void InitFirst(Grammar grammar)
        {
            firstDict[Terminal.End] = new(comparer) { new Terminal[] { Terminal.End } };
            foreach (var terminal in grammar.GetAllTerminals())
                firstDict[terminal] = new(comparer) { new Terminal[] { terminal } };
            foreach (var nonTerminal in grammar.GetAllNonTerminals())
                firstDict[nonTerminal] = new(comparer);

            foreach (var rule in grammar.GetAllRules())
                if (rule.tokens.Length == 0)
                    firstDict[rule.source].Add(Array.Empty<Terminal>());
        }

        private void GenerateFirst(Grammar grammar)
        {
            var chahged = true;
            while (chahged)
            {
                chahged = false;
                foreach (var rule in grammar.GetAllRules())
                {
                    var n = firstDict[rule.source].Count;
                    firstDict[rule.source].UnionWith(GetFirstSet(rule.tokens));
                    if (n != firstDict[rule.source].Count)
                        chahged = true;
                }
            }
        }
    }
}
