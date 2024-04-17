using FppCompilerLib.CodeGeneration;

namespace FppCompilerLib.SemanticAnalysis.Nodes
{
    internal abstract class InitedSemanticNode
    {
        public abstract TypedSemanticNode UpdateTypes(Context context);
    }

    internal abstract class TypedSemanticNode
    {
        public abstract UpdatedSemanticNode UpdateContext(Context context);
    }

    internal abstract class UpdatedSemanticNode
    {
        public abstract AssemblerCommand[] ToCode();
    }
}
