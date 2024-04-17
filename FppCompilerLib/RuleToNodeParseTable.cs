using FppCompilerLib.SemanticAnalysis.Nodes;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib
{
    internal class RuleToNodeParseTable
    {
        private readonly Dictionary<Rule, Func<NonTerminalNode, RuleToNodeParseTable, InitedSemanticNode>> parseTable = new();

        public RuleToNodeParseTable() { }

        public InitedSemanticNode Parse(NonTerminalNode rootNode)
        {
            return parseTable[rootNode.rule](rootNode, this);
        }

        public T Parse<T>(NonTerminalNode rootNode) where T : InitedSemanticNode
        {
            return (T)parseTable[rootNode.rule](rootNode, this);
        }

        public void Add(Rule source, Func<NonTerminalNode, RuleToNodeParseTable, InitedSemanticNode> parser)
        {
            parseTable.Add(source, parser);
        }
    }
}
