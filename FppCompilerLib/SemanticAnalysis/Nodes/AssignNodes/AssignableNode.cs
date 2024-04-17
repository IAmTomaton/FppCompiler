using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement;

namespace FppCompilerLib.SemanticAnalysis.Nodes.AssignNodes
{
    internal abstract class InitedAssignableNode : InitedSemanticNode
    {
        public override abstract TypedAssignableNode UpdateTypes(Context context);
    }

    internal abstract class TypedAssignableNode : TypedSemanticNode
    {
        public abstract TypeInfo TargetType { get; }
        public abstract AssignableNodeType AssignableNodeType { get; }
        public abstract UpdatedAssignableNode UpdateContext(Context context, Variable? target);

        public override UpdatedAssignableNode UpdateContext(Context context) => UpdateContext(context, null);
    }

    internal abstract class UpdatedAssignableNode : UpdatedSemanticNode
    {
        public abstract Constant? ConstantTraget { get; }
        public abstract Variable? VariableTraget { get; }
        public abstract AssignableNodeType AssignableNodeType { get; }
    }
}
