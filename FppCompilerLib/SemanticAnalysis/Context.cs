using FppCompilerLib.SemanticAnalysis.FunctionManagement;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.Nodes.LoopNodes;
using FppCompilerLib.SemanticAnalysis.TypeManagement;

namespace FppCompilerLib.SemanticAnalysis
{
    internal class Context
    {
        public readonly MemoryManager memoryManager;
        public readonly TypeManager typeManager;
        public readonly FunctionManager functionManager;
        public readonly LoopManager loopManager;

        public Context(MemoryManager memoryManager, TypeManager typeManager, FunctionManager functionManager, LoopManager loopManager)
        {
            this.memoryManager = memoryManager;
            this.typeManager = typeManager;
            this.functionManager = functionManager;
            this.loopManager = loopManager;
        }

        public Context GetChild()
        {
            return new Context(memoryManager.GetChild(), typeManager.GetChild(), functionManager.GetChild(), loopManager.GetChild());
        }
    }
}
