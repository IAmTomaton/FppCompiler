using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement;

namespace FppCompilerLib.SemanticAnalysis.Nodes.AssignNodes
{
    internal abstract class AssignableNode : SemanticNode
    {
        protected TypeInfo? targetType;
        public TypeInfo TargetType
        {
            get
            {
                if (targetType == null) throw new InvalidOperationException($"Call UpdateTypes before calling TargetType");
                return targetType;
            }
        }
        protected Data? staticTarget;
        public Data StaticTarget
        {
            get
            {
                if (staticTarget == null) throw new InvalidOperationException($"Call UpdateContext before calling StaticTarget");
                return staticTarget;
            }
        }
        protected bool? isStaticTarget;
        public bool IsStaticTarget
        {
            get
            {
                if (isStaticTarget == null) throw new InvalidOperationException($"Call UpdateContext before calling IsStaticTarget");
                return (bool)isStaticTarget;
            }
        }
        protected bool? shouldBeAssignedByPointer;
        public bool ShouldBeAssignedByPointer
        {
            get
            {
                if (shouldBeAssignedByPointer == null) throw new InvalidOperationException($"Call UpdateContext before calling ShouldBeAssignedByPointer");
                return (bool)shouldBeAssignedByPointer;
            }
        }

        public override AssignableNode UpdateContext(Context context) => UpdateContext(context, null);
        public abstract override AssignableNode UpdateTypes(Context context);
        public abstract AssignableNode UpdateContext(Context context, Variable? target);
    }
}
