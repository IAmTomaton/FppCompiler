using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.AssignNodes
{
    internal class InitedAssignableVariableNode : InitedAssignableNode
    {
        private readonly string name;

        public InitedAssignableVariableNode(string name)
        {
            this.name = name;
        }

        public static InitedAssignableVariableNode Parse(NonTerminalNode node, RuleToNodeParseTable _)
        {
            return new InitedAssignableVariableNode(node.childs[0].AsTerminalNode.RealValue);
        }

        public override TypedAssignableVariableNode UpdateTypes(Context context)
        {
            var targetType = context.memoryManager.GetVariable(name).TypeInfo;
            return new TypedAssignableVariableNode(name, targetType);
        }
    }

    internal class TypedAssignableVariableNode : TypedAssignableNode
    {
        public override TypeInfo TargetType => targetType;
        public override AssignableNodeType AssignableNodeType => AssignableNodeType.Variable;

        private readonly string name;
        private readonly TypeInfo targetType;

        public TypedAssignableVariableNode(string name, TypeInfo targetType)
        {
            this.name = name;
            this.targetType = targetType;
        }

        public override UpdatedAssignableVariableNode UpdateContext(Context context, Variable? target)
        {
            if (target != null)
                throw new ArgumentException("Cannot move target variable to temporary variable");

            var targetVariable = context.memoryManager.GetVariable(name);
            return new UpdatedAssignableVariableNode(targetVariable);
        }
    }

    internal class UpdatedAssignableVariableNode : UpdatedAssignableNode
    {
        public override Constant? ConstantTraget => null;
        public override Variable VariableTraget => targetVariable;
        public override AssignableNodeType AssignableNodeType => AssignableNodeType.Variable;

        private readonly Variable targetVariable;

        public UpdatedAssignableVariableNode(Variable targetVariable)
        {
            this.targetVariable = targetVariable;
        }

        public override AssemblerCommand[] ToCode() => Array.Empty<AssemblerCommand>();
    }
}
