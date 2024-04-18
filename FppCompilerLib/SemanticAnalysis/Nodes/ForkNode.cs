using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes;
using FppCompilerLib.SemanticAnalysis.TypeManagement.Types;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes
{

    internal class InitedForkNode : InitedSemanticNode
    {
        private readonly InitedFork[] forks;

        public InitedForkNode(InitedFork[] forks)
        {
            this.forks = forks;
        }

        public static InitedForkNode Parce(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var condition = parceTable.Parse<InitedResultableNode>(node.childs[2].AsNonTerminalNode);
            var body = parceTable.Parse<InitedBodyNode>(node.childs[5].AsNonTerminalNode);
            var forks = new List<InitedFork> { new InitedFork(condition, body) };

            node = node.childs[7].AsNonTerminalNode;
            while (node.childs.Length > 0 && node.childs[1].AsTerminalNode.RealValue == "if")
            {
                condition = parceTable.Parse<InitedResultableNode>(node.childs[3].AsNonTerminalNode);
                body = parceTable.Parse<InitedBodyNode>(node.childs[6].AsNonTerminalNode);
                forks.Add(new InitedFork(condition, body));
                node = node.childs.Last().AsNonTerminalNode;
            }

            if (node.childs.Length > 0 && node.childs[1].AsTerminalNode.RealValue != "if")
            {
                body = parceTable.Parse<InitedBodyNode>(node.childs[2].AsNonTerminalNode);
                condition = new InitedConstantNode("true");
                forks.Add(new InitedFork(condition, body));
            }

            return new InitedForkNode(forks.ToArray());
        }

        public override TypedForkNode UpdateTypes(Context context)
        {
            var typedForks = forks.Select(fork => UpdateTypesFork(fork, context)).ToArray();
            return new TypedForkNode(typedForks);
        }

        private static TypedFork UpdateTypesFork(InitedFork fork, Context context)
        {
            var condition = fork.condition.UpdateTypes(context.GetChild());
            if (condition.ResultType is not Bool)
                throw new ArgumentException("The result of the expression for the condition should be bool");
            var body = fork.body.UpdateTypes(context.GetChild());
            return new TypedFork(condition, body);
        }
    }

    internal class InitedFork
    {
        public readonly InitedResultableNode condition;
        public readonly InitedBodyNode body;

        public InitedFork(InitedResultableNode condition, InitedBodyNode body)
        {
            this.condition = condition;
            this.body = body;
        }
    }

    internal class TypedForkNode : TypedSemanticNode
    {
        private readonly TypedFork[] forks;

        public TypedForkNode(TypedFork[] forks)
        {
            this.forks = forks;
        }

        public override UpdatedForkNode UpdateContext(Context context)
        {
            var typedForks = forks
                .Where(
                fork =>
                {
                    // If the condition is always false
                    return !(fork.condition.IsConstantResult && fork.condition.GetConstantResult.machineValues[0] == 0);
                })
                .TakeWhile(
                fork =>
                {
                    // Drop all remaining conditions if a condition that is always true is found
                    return !(fork.condition.IsConstantResult && fork.condition.GetConstantResult.machineValues[0] != 0);
                });

            var constTrue = forks
                .FirstOrDefault(
                fork =>
                {
                    // Take condition that is always true if it was found
                    return fork.condition.IsConstantResult && fork.condition.GetConstantResult.machineValues[0] != 0;
                });
            if (constTrue != null)
                typedForks = typedForks.Append(constTrue);

            var updatedForks = typedForks
                .Select(fork => UpdateContextFork(fork, context.GetChild()))
                .ToArray();
            return new UpdatedForkNode(updatedForks);
        }

        private static UpdatedFork UpdateContextFork(TypedFork fork, Context context)
        {
            UpdatedResultableNode? updatedCondition = null;
            Variable? conditionVariable = null;
            if (fork.condition.IsConstantResult) { }
            else if (fork.condition.IsVariableResult)
            {
                var updatedArg = fork.condition.UpdateContext(context.GetChild(), null);
                conditionVariable = updatedArg.GetVariableResult;
            }
            else
            {
                var conditionContext = context.GetChild();
                (_, conditionVariable) = conditionContext.memoryManager.CreateTempVariable(new Bool());
                updatedCondition = fork.condition.UpdateContext(conditionContext, conditionVariable);
            }

            var updatedBody = fork.body.UpdateContext(context.GetChild());
            return new UpdatedFork(updatedCondition, updatedBody, conditionVariable);
        }
    }

    internal class TypedFork
    {
        public readonly TypedResultableNode condition;
        public readonly TypedBodyNode body;

        public TypedFork(TypedResultableNode condition, TypedBodyNode body)
        {
            this.condition = condition;
            this.body = body;
        }
    }

    internal class UpdatedForkNode : UpdatedSemanticNode
    {
        private readonly UpdatedFork[] forks;

        public UpdatedForkNode(UpdatedFork[] forks)
        {
            this.forks = forks;
        }

        public override AssemblerCommand[] ToCode()
        {
            var endLabel = "end_" + Guid.NewGuid().ToString();
            var commands = forks.SelectMany((fork, index) => ForkToCode(fork, endLabel, index == forks.Length - 1));
            if (forks.Length > 1)
                commands = commands.Append(AssemblerCommand.Label(endLabel));
            return commands.ToArray();
        }

        private static IEnumerable<AssemblerCommand> ForkToCode(UpdatedFork fork, string endLabel, bool isLast)
        {
            var skipLabel = "skip_" + Guid.NewGuid().ToString();

            var commands = Enumerable.Empty<AssemblerCommand>();
            if (fork.condition != null)
            {
                commands = commands
                    .Concat(fork.condition.ToCode())
                    .Append(AssemblerCommand.JmpIfEq0(fork.conditionVariable.address, skipLabel));
            }
            else if (fork.conditionVariable != null)
            {
                commands = commands.Append(AssemblerCommand.JmpIfEq0(fork.conditionVariable.address, skipLabel));
            }

            commands = commands.Concat(fork.body.ToCode());
            if (!isLast)
                commands = commands.Append(AssemblerCommand.Jmp(endLabel));
            commands = commands.Append(AssemblerCommand.Label(skipLabel));
            return commands;
        }
    }

    internal class UpdatedFork
    {
        public readonly UpdatedResultableNode? condition;
        public readonly UpdatedBodyNode body;
        public readonly Variable? conditionVariable;

        public UpdatedFork(UpdatedResultableNode? condition, UpdatedBodyNode body, Variable? conditionVariable)
        {
            this.condition = condition;
            this.body = body;
            this.conditionVariable = conditionVariable;
        }
    }
}
