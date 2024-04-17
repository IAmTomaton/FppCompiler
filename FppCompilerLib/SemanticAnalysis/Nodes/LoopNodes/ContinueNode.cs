using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.LoopNodes
{
    internal class InitedContinueNode : InitedSemanticNode
    {
        public InitedContinueNode() { }

        public static InitedContinueNode Parse(NonTerminalNode _, RuleToNodeParseTable __)
        {
            return new InitedContinueNode();
        }

        public override TypedContinueNode UpdateTypes(Context context)
        {
            return new TypedContinueNode();
        }
    }

    internal class TypedContinueNode : TypedSemanticNode
    {
        public TypedContinueNode() { }

        public override UpdatedContinueNode UpdateContext(Context context)
        {
            return new UpdatedContinueNode(context.loopManager.ContinueLabel);
        }
    }

    internal class UpdatedContinueNode : UpdatedSemanticNode
    {
        private readonly string continueLabel;

        public UpdatedContinueNode(string continueLabel)
        {
            this.continueLabel = continueLabel;
        }

        public override AssemblerCommand[] ToCode()
        {
            return new AssemblerCommand[] { AssemblerCommand.Jmp(continueLabel) };
        }
    }
}
