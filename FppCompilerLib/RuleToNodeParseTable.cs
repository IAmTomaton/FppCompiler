using FppCompilerLib.SemanticAnalysis.Nodes;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib
{
    internal class RuleToNodeParseTable
    {
        private readonly Dictionary<Rule, Func<NonTerminalNode, RuleToNodeParseTable, SemanticNode>> parseTable = new();

        public RuleToNodeParseTable() { }

        public SemanticNode Parse(NonTerminalNode rootNode)
        {
            return parseTable[rootNode.rule](rootNode, this);
        }

        public T Parse<T>(NonTerminalNode rootNode) where T : SemanticNode
        {
            return (T)parseTable[rootNode.rule](rootNode, this);
        }

        public void Add(Rule source, Func<NonTerminalNode, RuleToNodeParseTable, SemanticNode> parser)
        {
            parseTable.Add(source, parser);
        }
    }
}
