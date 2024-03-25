using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.LoopNodes
{
    internal class ForNode : SemanticNode
    {
        private readonly SemanticNode init;
        private readonly ForkNode fork;
        private readonly string continueLabel;
        private readonly string breakLabel;

        public ForNode(SemanticNode init, ForkNode fork, string continueLabel, string breakLabel)
        {
            this.init = init;
            this.fork = fork;
            this.continueLabel = continueLabel;
            this.breakLabel = breakLabel;
        }

        public static ForNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var guid = Guid.NewGuid().ToString();
            var continueLabel = "continue_" + guid;
            var breakLabel = "break_" + guid;

            var init = parceTable.Parse(node.childs[2].AsNonTerminalNode);
            var condition = parceTable.Parse<ResultableNode>(node.childs[4].AsNonTerminalNode);
            var step = parceTable.Parse(node.childs[6].AsNonTerminalNode);
            var loopBody = parceTable.Parse<BodyNode>(node.childs[9].AsNonTerminalNode);

            var assemblerInsert = new AssemblerInsertNode(new AssemblerCommand[] { AssemblerCommand.Jmp(continueLabel) });

            var forkBody = new BodyNode(new SemanticNode[] { loopBody, step, assemblerInsert });

            var fork = new ForkNode(new Fork[] { new Fork(condition, forkBody) });

            return new ForNode(init, fork, continueLabel, breakLabel);
        }

        public override ForNode UpdateTypes(Context context)
        {
            context = context.GetChild();
            var typedInit = init.UpdateTypes(context);
            var typedFork = fork.UpdateTypes(context);
            return new ForNode(typedInit, typedFork, continueLabel, breakLabel);
        }

        public override ForNode UpdateContext(Context context)
        {
            context.loopManager.SetLoopLabels(continueLabel, breakLabel);
            context = context.GetChild();
            var updatedInit = init.UpdateContext(context);
            var updatedFork = fork.UpdateContext(context);
            return new ForNode(updatedInit, updatedFork, continueLabel, breakLabel);
        }

        public override AssemblerCommand[] ToCode()
        {
            var commands = init.ToCode().Append(AssemblerCommand.Label(continueLabel));
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
