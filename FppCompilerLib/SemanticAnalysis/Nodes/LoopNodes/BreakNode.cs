using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.LoopNodes
{
    internal class BreakNode : SemanticNode
    {
        private readonly string? breakLabel;

        public BreakNode() { }

        public BreakNode(string breakLabel)
        {
            this.breakLabel = breakLabel;
        }

        public static BreakNode Parse(NonTerminalNode _, RuleToNodeParseTable __)
        {
            return new BreakNode();
        }

        public override BreakNode UpdateTypes(Context context)
        {
            return this;
        }

        public override BreakNode UpdateContext(Context context)
        {
            return new BreakNode(context.loopManager.BreakLabel);
        }

        public override AssemblerCommand[] ToCode()
        {
            return new AssemblerCommand[] { AssemblerCommand.Jmp(breakLabel) };
        }

        public override bool Equals(object? obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
