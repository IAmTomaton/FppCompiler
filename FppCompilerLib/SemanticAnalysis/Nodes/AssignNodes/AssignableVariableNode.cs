using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.AssignNodes
{
    internal class AssignableVariableNode : AssignableNode
    {
        private readonly string name;

        public AssignableVariableNode(string name)
        {
            isStaticTarget = true;
            shouldBeAssignedByPointer = false;
            this.name = name;
        }

        public AssignableVariableNode(string name, TypeInfo targetType) : this(name)
        {
            this.targetType = targetType;
        }

        public AssignableVariableNode(string name, Variable target) : this(name, target.TypeInfo)
        {
            this.staticTarget = target;
        }

        public static AssignableVariableNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            return new AssignableVariableNode(node.childs[0].AsTerminalNode.RealValue);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not AssignableVariableNode other) return false;
            return name == other.name;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override AssignableNode UpdateTypes(Context context)
        {
            var targetType = context.memoryManager.GetVariable(name).TypeInfo;
            return new AssignableVariableNode(name, targetType);
        }

        public override AssignableNode UpdateContext(Context context, Variable? target)
        {
            var targetVariable = context.memoryManager.GetVariable(name);
            return new AssignableVariableNode(name, targetVariable);
        }

        public override AssemblerCommand[] ToCode() => Array.Empty<AssemblerCommand>();
    }
}
