using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement.Types;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes
{
    internal class InitedDereferenceOperatorNode : InitedResultableNode
    {
        private readonly InitedResultableNode arg;

        public InitedDereferenceOperatorNode(InitedResultableNode arg)
        {
            this.arg = arg;
        }

        public static InitedDereferenceOperatorNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var arg = parceTable.Parse<InitedResultableNode>((NonTerminalNode)node.childs[1]);
            return new InitedDereferenceOperatorNode(arg);
        }

        public override TypedDereferenceOperatorNode UpdateTypes(Context context)
        {
            var typedArg = arg.UpdateTypes(context.GetChild());
            return new TypedDereferenceOperatorNode(typedArg);
        }
    }

    internal class TypedDereferenceOperatorNode : TypedResultableNode
    {
        public override TypeInfo ResultType => resultType;

        private readonly TypeInfo resultType;
        private readonly TypedResultableNode arg;
        private readonly bool castArrayToPointer;

        public TypedDereferenceOperatorNode(TypedResultableNode arg)
        {
            if (arg.ResultType is not Pointer pointer)
                throw new ArgumentException("Only a pointerType can be dereferenced");
            if (pointer.ChildType is ArrayFpp arrayFpp)
            {
                resultType = new Pointer(arrayFpp.ChildType);
                castArrayToPointer = true;
            }
            else
            {
                resultType = pointer.ChildType;
                castArrayToPointer = false;
            }
            this.arg = arg;
        }

        public override UpdatedDereferenceOperatorNode UpdateContext(Context context, Variable? target)
        {
            if (arg.IsConstantResult)
            {
                var pointerData = arg.GetConstantResult;
                var updatedArg = arg.UpdateContext(context.GetChild());
                return new UpdatedDereferenceOperatorNode(updatedArg, pointerData, target, castArrayToPointer);
            }
            else if (arg.IsVariableResult)
            {
                var updatedArg = arg.UpdateContext(context.GetChild());
                return new UpdatedDereferenceOperatorNode(updatedArg, updatedArg.GetVariableResult, target, castArrayToPointer);
            }
            else
            {
                (_, var pointerData) = context.memoryManager.CreateTempVariable(arg.ResultType);
                var updatedArg = arg.UpdateContext(context.GetChild(), pointerData);
                return new UpdatedDereferenceOperatorNode(updatedArg, pointerData, target, castArrayToPointer);
            }
        }
    }

    internal class UpdatedDereferenceOperatorNode : UpdatedResultableNode
    {
        private readonly UpdatedResultableNode arg;
        private readonly Data pointerData;
        private readonly Variable? target;
        private readonly bool castArrayToPointer;

        public UpdatedDereferenceOperatorNode(UpdatedResultableNode arg, Data pointerData, Variable? target, bool castArrayToPointer)
        {
            this.arg = arg;
            this.target = target;
            this.pointerData = pointerData;
            this.castArrayToPointer = castArrayToPointer;
        }

        public override AssemblerCommand[] ToCode()
        {
            IEnumerable<AssemblerCommand> commands = arg.ToCode();
            if (target != null)
            {
                if (castArrayToPointer)
                    commands = commands.Concat(MemoryManager.MoveOrPut(pointerData, target));
                else
                    commands = commands.Concat(MemoryManager.MoveFromPointer(pointerData, target));
            }
            return commands.ToArray();
        }
    }
}
