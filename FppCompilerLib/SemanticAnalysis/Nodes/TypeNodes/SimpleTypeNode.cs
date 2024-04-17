using FppCompilerLib.SemanticAnalysis.TypeManagement;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.TypeNodes
{
    internal class InitedSimpleTypeNode : InitedTypeNode
    {
        private readonly string name;

        public InitedSimpleTypeNode(string name)
        {
            this.name = name;
        }

        public static new InitedSimpleTypeNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            return new InitedSimpleTypeNode(((TerminalNode)node.childs[0]).RealValue);
        }

        public override TypedSimpleTypeNode UpdateTypes(Context context)
        {
            var typeInfo = context.typeManager.ResolveType(name);
            return new TypedSimpleTypeNode(typeInfo);
        }
    }

    internal class TypedSimpleTypeNode : TypedTypeNode
    {
        public override TypeInfo TypeInfo => typeInfo;
        private readonly TypeInfo typeInfo;

        public TypedSimpleTypeNode(TypeInfo typeInfo)
        {
            this.typeInfo = typeInfo;
        }
    }
}
