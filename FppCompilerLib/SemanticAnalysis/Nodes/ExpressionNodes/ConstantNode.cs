using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes
{
    internal class InitedConstantNode : InitedResultableNode
    {
        private readonly string value;

        public InitedConstantNode(string value)
        {
            this.value = value;
        }

        public static InitedConstantNode Parse(NonTerminalNode node, RuleToNodeParseTable _)
        {
            return new InitedConstantNode(((TerminalNode)node.childs[0]).RealValue);
        }

        public override TypedConstantNode UpdateTypes(Context context)
        {
            var result = context.typeManager.Parse(value);
            return new TypedConstantNode(result);
        }
    }

    internal class TypedConstantNode : TypedResultableNode
    {
        public override TypeInfo ResultType => ConstantResult.TypeInfo;
        protected override Constant ConstantResult => constant;

        private readonly Constant constant;

        public TypedConstantNode(Constant constant)
        {
            this.constant = constant;
        }

        public override UpdatedConstantNode UpdateContext(Context context, Variable? target)
        {
            return new UpdatedConstantNode(constant, target);
        }
    }

    internal class UpdatedConstantNode : UpdatedResultableNode
    {
        public Constant Constant => constant;

        private readonly Variable? target;
        private readonly Constant constant;

        public UpdatedConstantNode(Constant constant, Variable? target)
        {
            this.constant = constant;
            this.target = target;
        }

        public override AssemblerCommand[] ToCode()
        {
            if (target != null)
                return MemoryManager.MoveOrPut(constant, target);
            return Array.Empty<AssemblerCommand>();
        }
    }
}
