using FppCompilerLib.SemanticAnalysis.TypeManagement;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.TypeNodes
{
    internal abstract class TypeNode : SemanticNode
    {
        public abstract TypeInfo GetTypeInfo();

        public static SemanticNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            return parceTable.Parse((NonTerminalNode)node.childs[0]);
        }

        public override abstract TypeNode UpdateTypes(Context context);
        public override abstract TypeNode UpdateContext(Context context);
    }
}
