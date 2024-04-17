using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.FunctionManagement;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes
{
    internal class InitedBinaryOperatorNode : InitedResultableNode
    {
        private readonly InitedResultableNode arg0;
        private readonly InitedResultableNode arg1;
        private readonly string binaryOperator;

        public InitedBinaryOperatorNode(InitedResultableNode arg0, InitedResultableNode arg1, string binaryOperator)
        {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.binaryOperator = binaryOperator;
        }

        public static InitedResultableNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            return ExpressionToNode(InfixToPostfix(ConcatExpression(node)), parceTable);
        }

        private static IEnumerable<ParseNode> ConcatExpression(NonTerminalNode node)
        {
            yield return node.childs[0];
            node = (NonTerminalNode)node.childs[1];
            while (node.childs.Length > 0)
            {
                yield return node.childs[0];
                yield return node.childs[1];
                node = (NonTerminalNode)node.childs[2];
            }
        }

        private static IEnumerable<ParseNode> InfixToPostfix(IEnumerable<ParseNode> expression)
        {
            var stack = new Stack<TerminalNode>();

            foreach (var node in expression)
            {
                if (node is NonTerminalNode)
                    yield return node;
                else if (node is TerminalNode op)
                {
                    while (stack.Count > 0 &&
                        precedence[op.RealValue] >= precedence[stack.Peek().RealValue])
                        yield return stack.Pop();
                    stack.Push(op);
                }
            }

            while (stack.Count > 0)
                yield return stack.Pop();
        }

        private static InitedResultableNode ExpressionToNode(IEnumerable<ParseNode> expression, RuleToNodeParseTable parceTable)
        {
            var stack = new Stack<InitedResultableNode>();

            foreach (var node in expression)
            {
                if (node is NonTerminalNode nonTerminalNode)
                    stack.Push(parceTable.Parse<InitedResultableNode>(nonTerminalNode));
                else if (node is TerminalNode op)
                {
                    var arg1 = stack.Pop();
                    var arg0 = stack.Pop();
                    stack.Push(new InitedBinaryOperatorNode(arg0, arg1, op.RealValue));
                }
            }

            return stack.Pop();
        }

        public override TypedResultableNode UpdateTypes(Context context)
        {
            var typedArg0 = arg0.UpdateTypes(context.GetChild());
            var typedArg1 = arg1.UpdateTypes(context.GetChild());
            typedArg0.ResultType.TryGetBinaryOperator(binaryOperator, typedArg0.ResultType, typedArg1.ResultType, out var operatorFunc);

            if (typedArg0.IsConstantResult && typedArg1.IsConstantResult)
            {
                var constResult = operatorFunc.CalculateConstant(typedArg0.GetConstantResult, typedArg1.GetConstantResult);
                return new TypedConstantNode(constResult);
            }

            return new TypedBinaryOperatorNode(typedArg0, typedArg1, operatorFunc);
        }

        private static readonly Dictionary<string, int> precedence = new()
        {
            { "**", 3 },
            { "*", 4 },
            { "/", 4 },
            { "%", 4 },
            { "+", 5 },
            { "-", 5 },
            { "<<", 6 },
            { ">>", 6 },
            { "<", 7 },
            { "<=", 7 },
            { ">", 7 },
            { ">=", 7 },
            { "==", 8 },
            { "!=", 8 },
            { "&", 9 },
            { "^", 10 },
            { "|", 11 },
            { "&&", 12 },
            { "||", 13 },
        };
    }

    internal class TypedBinaryOperatorNode : TypedResultableNode
    {
        public override TypeInfo ResultType => operatorFunc.resultType;

        private readonly TypedResultableNode arg0;
        private readonly TypedResultableNode arg1;
        private readonly BinaryOperator operatorFunc;

        public TypedBinaryOperatorNode(TypedResultableNode arg0, TypedResultableNode arg1, BinaryOperator operatorFunc)
        {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.operatorFunc = operatorFunc;
        }

        public override UpdatedBinaryOperatorNode UpdateContext(Context context, Variable? target)
        {
            var (arg0Data, updatedArg0) = UpdateArg(arg0, context);
            var (arg1Data, updatedArg1) = UpdateArg(arg1, context);

            return new UpdatedBinaryOperatorNode(updatedArg0, updatedArg1, operatorFunc, target, arg0Data, arg1Data);
        }

        private static (Data argData, UpdatedResultableNode? updatedArg) UpdateArg(TypedResultableNode arg, Context context)
        {
            if (arg.IsConstantResult)
            {
                return (arg.GetConstantResult, null);
            }
            else if (arg.IsVariableResult)
            {
                var updatedArg = arg.UpdateContext(context.GetChild(), null);
                return (updatedArg.GetVariableResult, updatedArg);
            }
            else
            {
                (_, var tempVariable) = context.memoryManager.CreateTempVariable(arg.ResultType);
                var updatedArg = arg.UpdateContext(context.GetChild(), tempVariable);
                return (tempVariable, updatedArg);
            }
        }
    }

    internal class UpdatedBinaryOperatorNode : UpdatedResultableNode
    {
        private readonly UpdatedResultableNode? arg0;
        private readonly UpdatedResultableNode? arg1;
        private readonly BinaryOperator operatorFunc;
        private readonly Variable? target;
        private readonly Data arg0Data;
        private readonly Data arg1Data;

        public UpdatedBinaryOperatorNode(UpdatedResultableNode? arg0, UpdatedResultableNode? arg1, BinaryOperator operatorFunc, Variable? target, Data arg0Data, Data arg1Data)
        {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.operatorFunc = operatorFunc;
            this.target = target;
            this.arg0Data = arg0Data;
            this.arg1Data = arg1Data;
        }

        public override AssemblerCommand[] ToCode()
        {
            var commands = (arg0?.ToCode() ?? Enumerable.Empty<AssemblerCommand>())
                .Concat(arg1?.ToCode() ?? Enumerable.Empty<AssemblerCommand>())
                .Concat(operatorFunc.ToCode(arg0Data, arg1Data, target))
                .ToArray();
            return commands;
        }
    }
}
