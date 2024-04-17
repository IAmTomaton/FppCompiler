using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.LoopNodes
{
    internal class InitedForNode : InitedSemanticNode
    {
        private readonly InitedSemanticNode init;
        private readonly InitedForkNode fork;
        private readonly string continueLabel;
        private readonly string breakLabel;

        public InitedForNode(InitedSemanticNode init, InitedForkNode fork, string continueLabel, string breakLabel)
        {
            this.init = init;
            this.fork = fork;
            this.continueLabel = continueLabel;
            this.breakLabel = breakLabel;
        }

        public static InitedForNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var guid = Guid.NewGuid().ToString();
            var continueLabel = "continue_" + guid;
            var breakLabel = "break_" + guid;

            var init = parceTable.Parse(node.childs[2].AsNonTerminalNode);
            var condition = parceTable.Parse<InitedResultableNode>(node.childs[4].AsNonTerminalNode);
            var step = parceTable.Parse(node.childs[6].AsNonTerminalNode);
            var loopBody = parceTable.Parse<InitedBodyNode>(node.childs[9].AsNonTerminalNode);

            var assemblerInsert = new InitedAssemblerInsertNode(new AssemblerCommand[] { AssemblerCommand.Jmp(continueLabel) });

            var forkBody = new InitedBodyNode(new InitedSemanticNode[] { loopBody, step, assemblerInsert });

            var fork = new InitedForkNode(new InitedFork[] { new InitedFork(condition, forkBody) });

            return new InitedForNode(init, fork, continueLabel, breakLabel);
        }

        public override TypedForNode UpdateTypes(Context context)
        {
            context = context.GetChild();
            var typedInit = init.UpdateTypes(context);
            var typedFork = fork.UpdateTypes(context);
            return new TypedForNode(typedInit, typedFork, continueLabel, breakLabel);
        }
    }

    internal class TypedForNode : TypedSemanticNode
    {
        private readonly TypedSemanticNode init;
        private readonly TypedForkNode fork;
        private readonly string continueLabel;
        private readonly string breakLabel;

        public TypedForNode(TypedSemanticNode init, TypedForkNode fork, string continueLabel, string breakLabel)
        {
            this.init = init;
            this.fork = fork;
            this.continueLabel = continueLabel;
            this.breakLabel = breakLabel;
        }

        public override UpdatedForNode UpdateContext(Context context)
        {
            context.loopManager.SetLoopLabels(continueLabel, breakLabel);
            context = context.GetChild();
            var updatedInit = init.UpdateContext(context);
            var updatedFork = fork.UpdateContext(context);
            return new UpdatedForNode(updatedInit, updatedFork, continueLabel, breakLabel);
        }
    }

    internal class UpdatedForNode : UpdatedSemanticNode
    {
        private readonly UpdatedSemanticNode init;
        private readonly UpdatedForkNode fork;
        private readonly string continueLabel;
        private readonly string breakLabel;

        public UpdatedForNode(UpdatedSemanticNode init, UpdatedForkNode fork, string continueLabel, string breakLabel)
        {
            this.init = init;
            this.fork = fork;
            this.continueLabel = continueLabel;
            this.breakLabel = breakLabel;
        }

        public override AssemblerCommand[] ToCode()
        {
            var commands = init.ToCode()
                .Append(AssemblerCommand.Label(continueLabel))
                .Concat(fork.ToCode())
                .Append(AssemblerCommand.Label(breakLabel))
                .ToArray();
            return commands;
        }
    }
}
