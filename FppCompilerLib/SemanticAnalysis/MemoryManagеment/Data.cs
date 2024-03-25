using FppCompilerLib.SemanticAnalysis.TypeManagement;

namespace FppCompilerLib.SemanticAnalysis.MemoryManagement
{
    internal abstract class Data
    {
        public abstract TypeInfo TypeInfo { get; }
    }
}
