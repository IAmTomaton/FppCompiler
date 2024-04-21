using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement.Types;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes
{
    internal class InitedAddressOfOperatorNode : InitedResultableNode
    {
        private readonly InitedResultableNode arg;

        public InitedAddressOfOperatorNode(InitedResultableNode arg)
        {
            this.arg = arg;
        }

        public static InitedAddressOfOperatorNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var arg = parceTable.Parse<InitedResultableNode>((NonTerminalNode)node.childs[1]);
            return new InitedAddressOfOperatorNode(arg);
        }

        public override TypedAddressOfOperatorNode UpdateTypes(Context context)
        {
            var typedArg = arg.UpdateTypes(context.GetChild());
            if (typedArg is not TypedVariableNode typedVariable)
            {
                throw new ArgumentException("A pointerType can only be obtained to a variable");
            }
            return new TypedAddressOfOperatorNode(typedVariable);
        }
    }

    internal class TypedAddressOfOperatorNode : TypedResultableNode
    {
        public override TypeInfo ResultType => new Pointer(arg.ResultType);

        private readonly TypedVariableNode arg;

        public TypedAddressOfOperatorNode(TypedVariableNode arg)
        {
            this.arg = arg;
        }

        public override UpdatedAddressOfOperatorNode UpdateContext(Context context, Variable? target)
        {
            var updatedArg = arg.UpdateContext(context.GetChild());
            return new UpdatedAddressOfOperatorNode(updatedArg.GetVariableResult, target);
        }
    }

    internal class UpdatedAddressOfOperatorNode : UpdatedResultableNode
    {
        private readonly Variable variable;
        private readonly Variable? target;

        public UpdatedAddressOfOperatorNode(Variable variable, Variable? target)
        {
            this.variable = variable;
            this.target = target;
        }

        public override AssemblerCommand[] ToCode()
        {
            if (target != null)
                return MemoryManager.MoveOrPut(new Constant(variable.address, new Pointer(variable.TypeInfo)), target).ToArray();
            return Array.Empty<AssemblerCommand>();
        }
    }
}
