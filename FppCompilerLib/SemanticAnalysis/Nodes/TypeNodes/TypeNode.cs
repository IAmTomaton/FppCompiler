using FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes;
using FppCompilerLib.SemanticAnalysis.TypeManagement;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.TypeNodes
{
    internal abstract class InitedTypeNode : InitedSemanticNode
    {
        public static InitedSemanticNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var typeNode = parceTable.Parse<InitedTypeNode>((NonTerminalNode)node.childs[0]);

            var tokens = node.childs.Last().AsNonTerminalNode.childs;
            while (tokens.Length > 0)
            {
                var nonTerminal = tokens[0].AsNonTerminalNode;
                if (nonTerminal.nonTerminal.value == "pointer_type")
                {
                    typeNode = new InitedPointerTypeNode(typeNode);
                }
                else if (nonTerminal.nonTerminal.value == "array_type")
                {
                    var sizeExp = parceTable.Parse<InitedResultableNode>(nonTerminal.childs[1].AsNonTerminalNode);
                    typeNode = new InitedArrayTypeNode(sizeExp, typeNode);
                }
                tokens = nonTerminal.childs.Last().AsNonTerminalNode.childs;
            }

            return typeNode;
        }

        public override abstract TypedTypeNode UpdateTypes(Context context);
    }

    internal abstract class TypedTypeNode : TypedSemanticNode
    {
        public abstract TypeInfo TypeInfo { get; }

        public override UpdatedSemanticNode UpdateContext(Context context) { throw new NotImplementedException(); }
    }
}
