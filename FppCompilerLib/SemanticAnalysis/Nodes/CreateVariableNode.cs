using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.Nodes.AssignNodes;
using FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes;
using FppCompilerLib.SemanticAnalysis.Nodes.TypeNodes;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes
{
    internal class InitedCreateVariableNode : InitedSemanticNode
    {
        private readonly string name;
        private readonly InitedTypeNode typeNode;
        private readonly InitedAssignNode? assignNode;

        public InitedCreateVariableNode(InitedTypeNode typeNode, string name, InitedAssignNode? assignNode = null)
        {
            this.typeNode = typeNode;
            this.name = name;
            this.assignNode = assignNode;
        }

        public static InitedCreateVariableNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var name = ((TerminalNode)node.childs[1]).RealValue;
            var typeNode = parceTable.Parse<InitedTypeNode>((NonTerminalNode)node.childs[0]);

            InitedAssignNode? assignNode = null;
            var assignChilds = node.childs[2].AsNonTerminalNode.childs;
            if (assignChilds.Length > 0)
            {
                var expression = parceTable.Parse<InitedResultableNode>(assignChilds[1].AsNonTerminalNode);
                assignNode = new InitedAssignNode(new InitedAssignableVariableNode(name), expression);
            }

            return new InitedCreateVariableNode(typeNode, name, assignNode);
        }

        public override TypedCreateVariableNode UpdateTypes(Context context)
        {
            var typedTypeNode = typeNode.UpdateTypes(context.GetChild());
            context.memoryManager.AddVariable(name, typedTypeNode.TypeInfo);

            var typedAssignNode = assignNode?.UpdateTypes(context.GetChild());

            return new TypedCreateVariableNode(typedTypeNode, name, typedAssignNode);
        }
    }

    internal class TypedCreateVariableNode : TypedSemanticNode
    {
        private readonly string name;
        private readonly TypedTypeNode typeNode;
        private readonly TypedAssignNode? assignNode;

        public TypedCreateVariableNode(TypedTypeNode typeNode, string name, TypedAssignNode? assignNode)
        {
            this.typeNode = typeNode;
            this.name = name;
            this.assignNode = assignNode;
        }

        public override UpdatedCreateVariableNode UpdateContext(Context context)
        {
            context.memoryManager.AddVariable(name, typeNode.TypeInfo);
            var updatedAssignNode = assignNode?.UpdateContext(context.GetChild());

            return new UpdatedCreateVariableNode(updatedAssignNode);
        }
    }

    internal class UpdatedCreateVariableNode : UpdatedSemanticNode
    {
        private readonly UpdatedAssignNode? assignNode;

        public UpdatedCreateVariableNode(UpdatedAssignNode? assignNode = null)
        {
            this.assignNode = assignNode;
        }

        public override AssemblerCommand[] ToCode()
        {
            var commands = Enumerable.Empty<AssemblerCommand>();
            if (assignNode is not null)
                commands = commands.Concat(assignNode.ToCode());
            return commands.ToArray();
        }
    }
}
