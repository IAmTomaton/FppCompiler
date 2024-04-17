using FppCompilerLib.SemanticAnalysis.TypeManagement;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.TypeNodes
{
    internal abstract class InitedTypeNode : InitedSemanticNode
    {
        public static InitedSemanticNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            return parceTable.Parse((NonTerminalNode)node.childs[0]);
        }

        public override abstract TypedTypeNode UpdateTypes(Context context);
    }

    internal abstract class TypedTypeNode : TypedSemanticNode
    {
        public abstract TypeInfo TypeInfo { get; }

        public override UpdatedSemanticNode UpdateContext(Context context) { throw new NotImplementedException(); }
    }
}
