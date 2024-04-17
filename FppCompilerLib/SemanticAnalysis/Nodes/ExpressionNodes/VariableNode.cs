using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement;
using FppCompilerLib.SyntacticalAnalysis;
using System.Reflection.Metadata;

namespace FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes
{
    internal class InitedVariableNode : InitedResultableNode
    {
        public string Name => name;
        private readonly string name;

        public InitedVariableNode(string name)
        {
            this.name = name;
        }

        public static InitedVariableNode Parse(NonTerminalNode node, RuleToNodeParseTable _)
        {
            return new InitedVariableNode(((TerminalNode)node.childs[0]).RealValue);
        }

        public override TypedVariableNode UpdateTypes(Context context)
        {
            var resultType = context.memoryManager.GetVariable(name).TypeInfo;
            return new TypedVariableNode(name, resultType);
        }
    }

    internal class TypedVariableNode : TypedResultableNode
    {
        public override TypeInfo ResultType => resultType;
        public override bool IsVariableResult => true;

        private readonly string name;
        private readonly TypeInfo resultType;

        public TypedVariableNode(string name, TypeInfo resultType)
        {
            this.name = name;
            this.resultType = resultType;
        }

        public override UpdatedVariableNode UpdateContext(Context context, Variable? target)
        {
            var result = context.memoryManager.GetVariable(name);
            return new UpdatedVariableNode(result, target);
        }
    }

    internal class UpdatedVariableNode : UpdatedResultableNode
    {
        protected override Variable VariableResult => variable;

        private readonly Variable? target;
        private readonly Variable variable;

        public UpdatedVariableNode(Variable variable, Variable? target)
        {
            this.variable = variable;
            this.target = target;
        }

        public override AssemblerCommand[] ToCode()
        {
            if (target != null)
                return MemoryManager.MoveOrPut(variable, target);
            return Array.Empty<AssemblerCommand>();
        }
    }
}
