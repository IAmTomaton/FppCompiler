using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.LoopNodes
{
    internal class InitedBreakNode : InitedSemanticNode
    {
        public InitedBreakNode() { }

        public static InitedBreakNode Parse(NonTerminalNode _, RuleToNodeParseTable __)
        {
            return new InitedBreakNode();
        }

        public override TypedBreakNode UpdateTypes(Context context)
        {
            return new TypedBreakNode();
        }
    }

    internal class TypedBreakNode : TypedSemanticNode
    {
        public TypedBreakNode() { }

        public override UpdatedBreakNode UpdateContext(Context context)
        {
            return new UpdatedBreakNode(context.loopManager.BreakLabel);
        }
    }

    internal class UpdatedBreakNode : UpdatedSemanticNode
    {
        private readonly string breakLabel;

        public UpdatedBreakNode(string breakLabel)
        {
            this.breakLabel = breakLabel;
        }

        public override AssemblerCommand[] ToCode()
        {
            return new AssemblerCommand[] { AssemblerCommand.Jmp(breakLabel) };
        }
    }
}
