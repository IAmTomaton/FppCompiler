using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes
{
    internal class BodyNode : SemanticNode
    {
        private readonly SemanticNode[] childs;

        public BodyNode(SemanticNode[] childs)
        {
            this.childs = childs;
        }

        public static BodyNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            return new BodyNode(ParceChilds((NonTerminalNode)node.childs[0], parceTable).ToArray());
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not BodyNode other) return false;
            if (childs.Length != other.childs.Length) return false;
            return childs.SequenceEqual(other.childs);
        }

        public override int GetHashCode()
        {
            return childs.Select(ch => ch.GetHashCode()).Aggregate((a, b) => a * 37 + b);
        }

        private static IEnumerable<SemanticNode> ParceChilds(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            while (node.childs.Length > 0)
            {
                yield return parceTable.Parse((NonTerminalNode)node.childs[0]);
                node = (NonTerminalNode)node.childs.Last();
            }
        }

        public override BodyNode UpdateTypes(Context context)
        {
            var typedChilds = childs.Select(child => child.UpdateTypes(context)).ToArray();
            return new BodyNode(typedChilds);
        }

        public override BodyNode UpdateContext(Context context)
        {
            var updatedChilds = childs.Select(child => child.UpdateContext(GetRightContext(child, context))).ToArray();
            return new BodyNode(updatedChilds);
        }

        private static Context GetRightContext(SemanticNode node, Context context)
        {
            if (node is CreateVariableNode)
            {
                return context;
            }
            return context.GetChild();
        }

        public override AssemblerCommand[] ToCode()
        {
            return childs.SelectMany(child => child.ToCode()).ToArray();
        }
    }
}
