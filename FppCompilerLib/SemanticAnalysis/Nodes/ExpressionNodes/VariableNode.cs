using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes
{
    internal class VariableNode : ResultableNode
    {
        private readonly string name;
        public new Variable StaticResult
        {
            get
            {
                if (staticResult == null) throw new InvalidOperationException($"Call UpdateContext before calling StaticResult");
                return staticResult as Variable;
            }
        }

        public VariableNode(string name)
        {
            this.name = name;
        }

        private VariableNode(string name, TypeInfo resultType) : this(name)
        {
            this.resultType = resultType;
        }

        private VariableNode(string name, Variable result) : this(name, result.TypeInfo)
        {
            isStaticResult = true;
            staticResult = result;
        }

        private VariableNode(string name, Variable result, Variable target) : this(name, result)
        {
            this.target = target;
        }

        public static VariableNode Parse(NonTerminalNode node, RuleToNodeParseTable _)
        {
            return new VariableNode(((TerminalNode)node.childs[0]).RealValue);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not VariableNode other) return false;
            return name == other.name;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override VariableNode UpdateTypes(Context context)
        {
            var resultType = context.memoryManager.GetVariable(name).TypeInfo;
            return new VariableNode(name, resultType);
        }

        public override VariableNode UpdateContext(Context context, Variable? target)
        {
            var result = context.memoryManager.GetVariable(name);
            return new VariableNode(name, result, target);
        }

        public override AssemblerCommand[] ToCode()
        {
            if (target != null)
                return MemoryManager.MoveOrPut(StaticResult, target);
            return Array.Empty<AssemblerCommand>();
        }
    }
}
