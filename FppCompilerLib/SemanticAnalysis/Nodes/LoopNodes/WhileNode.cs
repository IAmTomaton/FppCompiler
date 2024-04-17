using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.LoopNodes
{
    internal class InitedWhileNode : InitedSemanticNode
    {
        private readonly InitedForkNode fork;
        private readonly string continueLabel;
        private readonly string breakLabel;

        public InitedWhileNode(InitedForkNode fork, string continueLabel, string breakLabel)
        {
            this.fork = fork;
            this.continueLabel = continueLabel;
            this.breakLabel = breakLabel;
        }

        public static InitedWhileNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var guid = Guid.NewGuid().ToString();
            var continueLabel = "continue_" + guid;
            var breakLabel = "break_" + guid;

            var condition = parceTable.Parse<InitedResultableNode>(node.childs[2].AsNonTerminalNode);
            var loopBody = parceTable.Parse<InitedBodyNode>(node.childs[5].AsNonTerminalNode);

            var assemblerInsert = new InitedAssemblerInsertNode(new AssemblerCommand[] { AssemblerCommand.Jmp(continueLabel) });

            var forkBody = new InitedBodyNode(new InitedSemanticNode[] { loopBody, assemblerInsert });

            var fork = new InitedForkNode(new InitedFork[] { new InitedFork(condition, forkBody) });

            return new InitedWhileNode(fork, continueLabel, breakLabel);
        }

        public override TypedWhileNode UpdateTypes(Context context)
        {
            var typedFork = fork.UpdateTypes(context.GetChild());
            return new TypedWhileNode(typedFork, continueLabel, breakLabel);
        }
    }

    internal class TypedWhileNode : TypedSemanticNode
    {
        private readonly TypedForkNode fork;
        private readonly string continueLabel;
        private readonly string breakLabel;

        public TypedWhileNode(TypedForkNode fork, string continueLabel, string breakLabel)
        {
            this.fork = fork;
            this.continueLabel = continueLabel;
            this.breakLabel = breakLabel;
        }

        public override UpdatedWhileNode UpdateContext(Context context)
        {
            context.loopManager.SetLoopLabels(continueLabel, breakLabel);
            var updatedFork = fork.UpdateContext(context.GetChild());
            return new UpdatedWhileNode(updatedFork, continueLabel, breakLabel);
        }
    }

    internal class UpdatedWhileNode : UpdatedSemanticNode
    {
        private readonly UpdatedForkNode fork;
        private readonly string continueLabel;
        private readonly string breakLabel;

        public UpdatedWhileNode(UpdatedForkNode fork, string continueLabel, string breakLabel)
        {
            this.fork = fork;
            this.continueLabel = continueLabel;
            this.breakLabel = breakLabel;
        }

        public override AssemblerCommand[] ToCode()
        {
            var commands = new AssemblerCommand[] { AssemblerCommand.Label(continueLabel) }
                .Concat(fork.ToCode())
                .Append(AssemblerCommand.Label(breakLabel))
                .ToArray();
            return commands;
        }
    }
}
