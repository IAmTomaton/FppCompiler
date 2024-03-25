using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes
{
    internal class ConstantNode : ResultableNode
    {
        private readonly string value;
        public new Constant StaticResult
        {
            get
            {
                if (staticResult == null) throw new InvalidOperationException($"Call UpdateContext before calling StaticResult");
                return staticResult as Constant;
            }
        }

        public ConstantNode(string value)
        {
            this.value = value;
        }

        private ConstantNode(string value, Constant result) : this(value)
        {
            isStaticResult = true;
            resultType = result.TypeInfo;
            staticResult = result;
        }

        private ConstantNode(string value, Constant result, Variable target) : this(value, result)
        {
            this.target = target;
        }

        public static ConstantNode Parse(NonTerminalNode node, RuleToNodeParseTable _)
        {
            return new ConstantNode(((TerminalNode)node.childs[0]).RealValue);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not ConstantNode other) return false;
            return value == other.value;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override ConstantNode UpdateTypes(Context context)
        {
            var result = context.typeManager.Parse(value);
            return new ConstantNode(value, result);
        }

        public override ConstantNode UpdateContext(Context context, Variable? target)
        {
            return new ConstantNode(value, StaticResult, target);
        }

        public override AssemblerCommand[] ToCode()
        {
            if (target != null)
                return MemoryManager.MoveOrPut(StaticResult, target);
            return Array.Empty<AssemblerCommand>();
        }
    }
}
