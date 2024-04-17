using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SyntacticalAnalysis;
using System.Collections.ObjectModel;

namespace FppCompilerLib.SemanticAnalysis.Nodes
{
    internal class InitedBodyNode : InitedSemanticNode
    {
        public ReadOnlyCollection<InitedSemanticNode> Childs => Array.AsReadOnly(childs);
        private readonly InitedSemanticNode[] childs;

        public InitedBodyNode(InitedSemanticNode[] childs)
        {
            this.childs = childs;
        }

        public static InitedBodyNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            return new InitedBodyNode(ParceChilds((NonTerminalNode)node.childs[0], parceTable).ToArray());
        }

        private static IEnumerable<InitedSemanticNode> ParceChilds(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            while (node.childs.Length > 0)
            {
                yield return parceTable.Parse((NonTerminalNode)node.childs[0]);
                node = (NonTerminalNode)node.childs.Last();
            }
        }

        public override TypedBodyNode UpdateTypes(Context context)
        {
            var typedChilds = childs.Select(child => child.UpdateTypes(context)).ToArray();
            return new TypedBodyNode(typedChilds);
        }
    }

    internal class TypedBodyNode : TypedSemanticNode
    {
        public ReadOnlyCollection<TypedSemanticNode> Childs => Array.AsReadOnly(childs);
        private readonly TypedSemanticNode[] childs;

        public TypedBodyNode(TypedSemanticNode[] childs)
        {
            this.childs = childs;
        }

        public override UpdatedBodyNode UpdateContext(Context context)
        {
            var updatedChilds = childs.Select(child => child.UpdateContext(GetRightContext(child, context))).ToArray();
            return new UpdatedBodyNode(updatedChilds);
        }

        private static Context GetRightContext(TypedSemanticNode node, Context context)
        {
            if (node is TypedCreateVariableNode)
            {
                return context;
            }
            return context.GetChild();
        }
    }

    internal class UpdatedBodyNode : UpdatedSemanticNode
    {
        public ReadOnlyCollection<UpdatedSemanticNode> Childs => Array.AsReadOnly(childs);
        private readonly UpdatedSemanticNode[] childs;

        public UpdatedBodyNode(UpdatedSemanticNode[] childs)
        {
            this.childs = childs;
        }

        public override AssemblerCommand[] ToCode()
        {
            return childs.SelectMany(child => child.ToCode()).ToArray();
        }
    }
}
