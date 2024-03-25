using FppCompilerLib.CodeGeneration;

namespace FppCompilerLib.SemanticAnalysis.Nodes
{
    internal abstract class SemanticNode
    {
        public abstract override int GetHashCode();
        public abstract override bool Equals(object? obj);
        public abstract SemanticNode UpdateTypes(Context context);
        public abstract SemanticNode UpdateContext(Context context);
        public abstract AssemblerCommand[] ToCode();
    }
}
