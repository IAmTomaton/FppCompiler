﻿using FppCompilerLib.SemanticAnalysis.FunctionManagement;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.Nodes;
using FppCompilerLib.SemanticAnalysis.Nodes.LoopNodes;
using FppCompilerLib.SemanticAnalysis.TypeManagement;

namespace FppCompilerLib.SemanticAnalysis
{
    /// <summary>
    /// Performs semantic analysis of the syntax tree
    /// </summary>
    internal class SemanticAnalyzer
    {
        /// <summary>
        /// Converts a program's syntax tree into a semantic tree
        /// </summary>
        /// <param name="rootNode"></param>
        /// <returns></returns>
        public UpdatedSemanticNode Parse(InitedSemanticNode rootNode)
        {
            var ramManager = new RAMManager(8);
            var memoryManager = new MemoryManager(ramManager);
            var typeManager = new TypeManager();
            var functionManager = new FunctionManager();
            var loopManager = new LoopManager();
            var context = new Context(memoryManager, typeManager, functionManager, loopManager);

            var typedRootNode = rootNode.UpdateTypes(context.GetChild());
            var updatedRootNode = typedRootNode.UpdateContext(context);
            return updatedRootNode;
        }
    }
}
