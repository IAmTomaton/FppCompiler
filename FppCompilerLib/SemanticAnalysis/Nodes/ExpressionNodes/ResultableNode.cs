using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement;

namespace FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes
{
    internal abstract class InitedResultableNode : InitedSemanticNode
    {
        public override abstract TypedResultableNode UpdateTypes(Context context);
    }

    internal abstract class TypedResultableNode : TypedSemanticNode
    {
        public Constant GetConstantResult => ConstantResult ?? throw new InvalidOperationException("Node does not have constant result");
        public bool IsConstantResult => ConstantResult != null;
        public virtual bool IsVariableResult => false;

        protected virtual Constant? ConstantResult => null;

        public abstract TypeInfo ResultType { get; }
        public abstract UpdatedResultableNode UpdateContext(Context context, Variable? target);

        public override UpdatedResultableNode UpdateContext(Context context) => UpdateContext(context, null);
    }

    internal abstract class UpdatedResultableNode : UpdatedSemanticNode
    {
        public Variable GetVariableResult => VariableResult ?? throw new InvalidOperationException("Node does not have variable result");
        public bool IsVariableResult => VariableResult != null;

        protected virtual Variable? VariableResult => null;
    }
}
