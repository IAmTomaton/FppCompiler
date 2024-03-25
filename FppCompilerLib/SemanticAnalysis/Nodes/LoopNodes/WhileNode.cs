using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.LoopNodes
{
    internal class WhileNode : SemanticNode
    {
        private readonly ForkNode fork;
        private readonly string continueLabel;
        private readonly string breakLabel;

        public WhileNode(ForkNode fork, string continueLabel, string breakLabel)
        {
            this.fork = fork;
            this.continueLabel = continueLabel;
            this.breakLabel = breakLabel;
        }

        public static WhileNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var guid = Guid.NewGuid().ToString();
            var continueLabel = "continue_" + guid;
            var breakLabel = "break_" + guid;

            var condition = parceTable.Parse<ResultableNode>(node.childs[2].AsNonTerminalNode);
            var loopBody = parceTable.Parse<BodyNode>(node.childs[5].AsNonTerminalNode);

            var assemblerInsert = new AssemblerInsertNode(new AssemblerCommand[] { AssemblerCommand.Jmp(continueLabel) });

            var forkBody = new BodyNode(new SemanticNode[] { loopBody, assemblerInsert });

            var fork = new ForkNode(new Fork[] { new Fork(condition, forkBody) });

            return new WhileNode(fork, continueLabel, breakLabel);
        }

        public override WhileNode UpdateTypes(Context context)
        {
            var typedFork = fork.UpdateTypes(context.GetChild());
            return new WhileNode(typedFork, continueLabel, breakLabel);
        }

        public override WhileNode UpdateContext(Context context)
        {
            context.loopManager.SetLoopLabels(continueLabel, breakLabel);
            var updatedFork = fork.UpdateContext(context.GetChild());
            return new WhileNode(updatedFork, continueLabel, breakLabel);
        }

        public override AssemblerCommand[] ToCode()
        {
            var commands = new AssemblerCommand[] { AssemblerCommand.Label(continueLabel) };
            return commands
                .Concat(fork.ToCode())
                .Append(AssemblerCommand.Label(breakLabel))
                .ToArray();
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
