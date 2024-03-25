using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement;

namespace FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes
{
    internal abstract class ResultableNode : SemanticNode
    {
        protected Variable? target;

        protected TypeInfo? resultType;
        public TypeInfo ResultType
        {
            get
            {
                if (resultType == null) throw new InvalidOperationException($"Call UpdateTypes before calling ResultType");
                return resultType;
            }
        }
        protected Data? staticResult;
        public Data StaticResult
        {
            get
            {
                if (staticResult == null) throw new InvalidOperationException($"Call UpdateContext before calling StaticResult");
                return staticResult;
            }
        }
        protected bool? isStaticResult;
        public bool IsStaticResult
        {
            get
            {
                if (isStaticResult == null) throw new InvalidOperationException($"Call UpdateContext before calling IsStaticResult");
                return (bool)isStaticResult;
            }
        }

        public override ResultableNode UpdateContext(Context context) => UpdateContext(context, null);
        public abstract override ResultableNode UpdateTypes(Context context);
        public abstract ResultableNode UpdateContext(Context context, Variable? target);
    }
}
