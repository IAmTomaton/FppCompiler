using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.LoopNodes
{
    internal class ContinueNode : SemanticNode
    {
        private readonly string? continueLabel;

        public ContinueNode() { }

        public ContinueNode(string continueLabel)
        {
            this.continueLabel = continueLabel;
        }

        public static ContinueNode Parse(NonTerminalNode _, RuleToNodeParseTable __)
        {
            return new ContinueNode();
        }

        public override ContinueNode UpdateTypes(Context context)
        {
            return this;
        }

        public override ContinueNode UpdateContext(Context context)
        {
            return new ContinueNode(context.loopManager.ContinueLabel);
        }

        public override AssemblerCommand[] ToCode()
        {
            return new AssemblerCommand[] { AssemblerCommand.Jmp(continueLabel) };
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
