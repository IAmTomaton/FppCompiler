using FppCompilerLib.SemanticAnalysis.FunctionManagement;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.Nodes;
using FppCompilerLib.SemanticAnalysis.Nodes.LoopNodes;
using FppCompilerLib.SemanticAnalysis.TypeManagement;

namespace FppCompilerLib.SemanticAnalysis
{
    internal class SemanticAnalyzer
    {
        public SemanticNode Parse(SemanticNode rootNode)
        {
            var ramManager = new RAMManager(8);
            var memoryManager = new MemoryManager(ramManager);
            var typeManager = new TypeManager();
            var functionManager = new FunctionManager();
            var loopManager = new LoopManager();
            var context = new Context(memoryManager, typeManager, functionManager, loopManager);

            rootNode = rootNode.UpdateTypes(context.GetChild());
            rootNode = rootNode.UpdateContext(context);
            return rootNode;
        }
    }
}
