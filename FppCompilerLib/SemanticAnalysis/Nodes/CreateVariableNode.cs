using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.Nodes.AssignNodes;
using FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes;
using FppCompilerLib.SemanticAnalysis.Nodes.TypeNodes;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes
{
    internal class CreateVariableNode : SemanticNode
    {
        private readonly string name;
        private readonly TypeNode typeNode;
        private readonly AssignNode? assignNode;

        public CreateVariableNode(TypeNode typeNode, string name, AssignNode? assignNode = null)
        {
            this.typeNode = typeNode;
            this.name = name;
            this.assignNode = assignNode;
        }

        public static CreateVariableNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var name = ((TerminalNode)node.childs[1]).RealValue;
            var typeNode = parceTable.Parse<TypeNode>((NonTerminalNode)node.childs[0]);

            AssignNode? assignNode = null;
            var assignChilds = node.childs[2].AsNonTerminalNode.childs;
            if (assignChilds.Length > 0)
            {
                var expression = parceTable.Parse<ResultableNode>(assignChilds[1].AsNonTerminalNode);
                assignNode = new AssignNode(new AssignableVariableNode(name), expression);
            }

            return new CreateVariableNode(typeNode, name, assignNode);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not CreateVariableNode other) return false;
            return name == other.name && typeNode.Equals(other.typeNode) &&
                (assignNode != null ? assignNode.Equals(other.assignNode) : other.assignNode == null);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode() + typeNode.GetHashCode() * 37 +
                (assignNode != null ? assignNode.GetHashCode() : 0) * 37 * 37;
        }

        public override CreateVariableNode UpdateTypes(Context context)
        {
            var typedTypeNode = typeNode.UpdateTypes(context.GetChild());
            context.memoryManager.AddVariable(name, typedTypeNode.GetTypeInfo());

            var typedAssignNode = assignNode?.UpdateTypes(context.GetChild());

            return new CreateVariableNode(typedTypeNode, name, typedAssignNode);
        }

        public override CreateVariableNode UpdateContext(Context context)
        {
            context.memoryManager.AddVariable(name, typeNode.GetTypeInfo());
            var updatedAssignNode = assignNode?.UpdateContext(context.GetChild());

            return new CreateVariableNode(typeNode, name, updatedAssignNode);
        }

        public override AssemblerCommand[] ToCode()
        {
            IEnumerable<AssemblerCommand> commands = typeNode.ToCode();
            if (assignNode is not null)
                commands = commands.Concat(assignNode.ToCode());
            return commands.ToArray();
        }
    }
}
