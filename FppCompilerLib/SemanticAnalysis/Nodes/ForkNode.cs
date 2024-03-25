using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes;
using FppCompilerLib.SemanticAnalysis.TypeManagement.Types.NumericTypes;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes
{
    internal class Fork
    {
        public readonly ResultableNode condition;
        public readonly BodyNode body;
        public readonly Variable? tempVariable;

        public Fork(ResultableNode condition, BodyNode body)
        {
            this.condition = condition;
            this.body = body;
        }

        public Fork(ResultableNode condition, BodyNode body, Variable tempVariable)
        {
            this.condition = condition;
            this.body = body;
            this.tempVariable = tempVariable;
        }

        public Fork Copy() => new Fork(condition, body, tempVariable);

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not Fork other) return false;
            if (condition.Equals(other.condition)) return false;
            return body.Equals(other.body);
        }

        public override int GetHashCode()
        {
            return condition.GetHashCode() * 37 + body.GetHashCode();
        }
    }

    internal class ForkNode : SemanticNode
    {
        private readonly Fork[] forks;

        public ForkNode(Fork[] forks)
        {
            this.forks = forks;
        }

        public static ForkNode Parce(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var condition = parceTable.Parse<ResultableNode>(node.childs[2].AsNonTerminalNode);
            var body = parceTable.Parse<BodyNode>(node.childs[5].AsNonTerminalNode);
            var forks = new List<Fork> { new Fork(condition, body) };

            node = node.childs[7].AsNonTerminalNode;
            while (node.childs.Length > 0 && node.childs[1].AsTerminalNode.RealValue == "if")
            {
                condition = parceTable.Parse<ResultableNode>(node.childs[3].AsNonTerminalNode);
                body = parceTable.Parse<BodyNode>(node.childs[6].AsNonTerminalNode);
                forks.Add(new Fork(condition, body));
                node = node.childs.Last().AsNonTerminalNode;
            }

            if (node.childs.Length > 0 && node.childs[1].AsTerminalNode.RealValue != "if")
            {
                body = parceTable.Parse<BodyNode>(node.childs[2].AsNonTerminalNode);
                condition = new ConstantNode("true");
                forks.Add(new Fork(condition, body));
            }

            return new ForkNode(forks.ToArray());
        }

        public override ForkNode UpdateTypes(Context context)
        {
            var typedForks = forks.Select(fork => UpdateTypesFork(fork, context)).ToArray();
            return new ForkNode(typedForks);
        }

        private static Fork UpdateTypesFork(Fork fork, Context context)
        {
            var condition = fork.condition.UpdateTypes(context.GetChild());
            if (condition.ResultType is not Bool)
                throw new ArgumentException("The result of the expression for the condition should be bool");
            var body = fork.body.UpdateTypes(context.GetChild());
            return new Fork(condition, body);
        }

        public override ForkNode UpdateContext(Context context)
        {
            var updatedForks = forks
                .Where(
                fork =>
                {
                    var updatedCond = fork.condition.UpdateContext(context.GetChild());
                    return !(updatedCond.IsStaticResult && updatedCond.StaticResult is Constant constCond && constCond.machineValues[0] == 0);
                })
                .TakeWhile(
                fork =>
                {
                    var updatedCond = fork.condition.UpdateContext(context.GetChild());
                    return !(updatedCond.IsStaticResult && updatedCond.StaticResult is Constant constCond && constCond.machineValues[0] != 0);
                });
            var constTrue = forks
                .FirstOrDefault(
                fork =>
                {
                    var updatedCond = fork.condition.UpdateContext(context.GetChild());
                    return updatedCond.IsStaticResult && updatedCond.StaticResult is Constant constCond && constCond.machineValues[0] != 0;
                });
            if (constTrue != null)
                updatedForks = updatedForks.Append(constTrue);
            updatedForks = updatedForks
                .Select(fork => UpdateContextFork(fork, context.GetChild()));
            return new ForkNode(updatedForks.ToArray());
        }

        private static Fork UpdateContextFork(Fork fork, Context context)
        {
            var condition = fork.condition.UpdateContext(context.GetChild());
            BodyNode body;

            if (!condition.IsStaticResult)
            {
                var conditionContext = context.GetChild();
                (_, var tempVariable) = conditionContext.memoryManager.CreateTempVariable(new Bool());
                condition = fork.condition.UpdateContext(conditionContext, tempVariable);
                body = fork.body.UpdateContext(context.GetChild());
                return new Fork(condition, body, tempVariable);
            }

            body = fork.body.UpdateContext(context.GetChild());
            return new Fork(condition, body);
        }

        public override AssemblerCommand[] ToCode()
        {
            var endLabel = "end_" + Guid.NewGuid().ToString();
            var commands = forks.SelectMany((fork, index) => ForkToCode(fork, endLabel, index == forks.Length - 1));
            if (forks.Length > 1)
                commands = commands.Append(AssemblerCommand.Label(endLabel));
            return commands.ToArray();
        }

        private static IEnumerable<AssemblerCommand> ForkToCode(Fork fork, string endLabel, bool isLast)
        {
            var skipLabel = "skip_" + Guid.NewGuid().ToString();

            var commands = Enumerable.Empty<AssemblerCommand>();
            if (!fork.condition.IsStaticResult)
            {
                commands = commands
                    .Concat(fork.condition.ToCode())
                    .Append(AssemblerCommand.JmpIfEq0(fork.tempVariable.address, skipLabel));
            }
            else if (fork.condition.StaticResult is Variable variableCond)
            {
                commands = commands.Append(AssemblerCommand.JmpIfEq0(variableCond.address, skipLabel));
            }
            commands = commands.Concat(fork.body.ToCode());
            if (!isLast)
                commands = commands.Append(AssemblerCommand.Jmp(endLabel));
            commands = commands.Append(AssemblerCommand.Label(skipLabel));
            return commands;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not ForkNode other) return false;
            if (forks.Length != other.forks.Length) return false;
            return forks.SequenceEqual(other.forks);
        }

        public override int GetHashCode()
        {
            return forks.Select(ch => ch.GetHashCode()).Aggregate((a, b) => a * 37 + b);
        }
    }
}
