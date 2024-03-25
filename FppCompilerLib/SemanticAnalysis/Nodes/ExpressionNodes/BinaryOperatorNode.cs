using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.FunctionManagement;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement.Types.NumericTypes;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes
{
    internal class BinaryOperatorNode : ResultableNode
    {
        private readonly ResultableNode arg0;
        private readonly ResultableNode arg1;
        private readonly string binaryOperator;
        private readonly BinaryOperator? operatorFunc;
        private readonly Variable? tempVariable0;
        private readonly Variable? tempVariable1;

        public BinaryOperatorNode(ResultableNode arg0, ResultableNode arg1, string binaryOperator)
        {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.binaryOperator = binaryOperator;
        }

        public BinaryOperatorNode(ResultableNode arg0, ResultableNode arg1, string binaryOperator, BinaryOperator operatorFunc) :
            this(arg0, arg1, binaryOperator)
        {
            resultType = operatorFunc.resultType;
            this.operatorFunc = operatorFunc;
        }

        private BinaryOperatorNode(ResultableNode arg0, ResultableNode arg1, string binaryOperator, BinaryOperator operatorFunc, Constant constantResult, Variable target) :
            this(arg0, arg1, binaryOperator, operatorFunc)
        {
            isStaticResult = true;
            this.staticResult = constantResult;
            this.target = target;
        }

        public BinaryOperatorNode(ResultableNode arg0, ResultableNode arg1, string binaryOperator, BinaryOperator operatorFunc, Variable target,
            Variable? tempVariable0 = null, Variable? tempVariable1 = null) :
            this(arg0, arg1, binaryOperator, operatorFunc)
        {
            isStaticResult = false;
            this.tempVariable0 = tempVariable0;
            this.tempVariable1 = tempVariable1;
            this.target = target;
        }

        public static ResultableNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            return ExpressionToNode(InfixToPostfix(ConcatExpression(node)), parceTable);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not BinaryOperatorNode other) return false;
            return binaryOperator == other.binaryOperator && arg0.Equals(other.arg0) && arg1.Equals(other.arg1);
        }

        public override int GetHashCode()
        {
            return binaryOperator.GetHashCode() + arg0.GetHashCode() * 37 + arg1.GetHashCode() * 37 * 37;
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

        private static ResultableNode ExpressionToNode(IEnumerable<ParseNode> expression, RuleToNodeParseTable parceTable)
        {
            var stack = new Stack<ResultableNode>();

            foreach (var node in expression)
            {
                if (node is NonTerminalNode nonTerminalNode)
                    stack.Push(parceTable.Parse<ResultableNode>(nonTerminalNode));
                else if (node is TerminalNode op)
                {
                    var arg1 = stack.Pop();
                    var arg0 = stack.Pop();
                    stack.Push(new BinaryOperatorNode(arg0, arg1, op.RealValue));
                }
            }

            return stack.Pop();
        }

        public override BinaryOperatorNode UpdateTypes(Context context)
        {
            var typedArg0 = arg0.UpdateTypes(context.GetChild());
            var typedArg1 = arg1.UpdateTypes(context.GetChild());
            BinaryOperator operatorFunc;
            if (typedArg0.ResultType.TryGetBinaryOperator(binaryOperator, typedArg0.ResultType, typedArg1.ResultType, out operatorFunc))
            {

            }
            else if (typedArg0.ResultType is Numeric arg0Num && typedArg1.ResultType is Numeric arg1Num)
            {
                var targetType = Numeric.ResolveNumericCast(arg0Num, arg1Num);
                if (!targetType.TryGetBinaryOperator(binaryOperator, arg0Num, arg1Num, out operatorFunc))
                    throw new ArgumentException();
            }
            else
                throw new NotImplementedException();

            return new BinaryOperatorNode(typedArg0, typedArg1, binaryOperator, operatorFunc);
        }

        public override ResultableNode UpdateContext(Context context, Variable? target)
        {
            if (operatorFunc is null)
                throw new InvalidOperationException("Call UpdateTypes before call UpdateContext.");

            var updatedArg0 = arg0.UpdateContext(context.GetChild());
            var updatedArg1 = arg1.UpdateContext(context.GetChild());

            if (updatedArg0.IsStaticResult && updatedArg0.StaticResult is Constant constArg0 && 
                updatedArg1.IsStaticResult && updatedArg1.StaticResult is Constant constArg1)
            {
                var result = operatorFunc.CalculateConstant(constArg0, constArg1);
                return new BinaryOperatorNode(updatedArg0, updatedArg1, binaryOperator, operatorFunc, result, target);
            }

            Variable? tempVariable0 = null;
            Variable? tempVariable1 = null;

            if (!updatedArg0.IsStaticResult)
            {
                (_, tempVariable0) = context.memoryManager.CreateTempVariable(updatedArg0.ResultType);
                updatedArg0 = arg0.UpdateContext(context.GetChild(), tempVariable0);
            }

            if (!updatedArg1.IsStaticResult)
            {
                (_, tempVariable1) = context.memoryManager.CreateTempVariable(updatedArg1.ResultType);
                updatedArg1 = arg1.UpdateContext(context.GetChild(), tempVariable1);
            }

            return new BinaryOperatorNode(updatedArg0, updatedArg1, binaryOperator, operatorFunc, target, tempVariable0, tempVariable1);
        }

        public override AssemblerCommand[] ToCode()
        {
            if (IsStaticResult)
            {
                return target == null ? Array.Empty<AssemblerCommand>() : MemoryManager.MoveOrPut(StaticResult, target);
            }

            Data arg0Result;
            if (arg0.IsStaticResult)
                arg0Result = arg0.StaticResult;
            else
                arg0Result = tempVariable0;

            Data arg1Result;
            if (arg1.IsStaticResult)
                arg1Result = arg1.StaticResult;
            else
                arg1Result = tempVariable1;

            IEnumerable<AssemblerCommand> commands = arg0.ToCode();
            commands = commands.Concat(arg1.ToCode());
            commands = commands.Concat(operatorFunc.ToCode(arg0Result, arg1Result, target));
            return commands.ToArray();
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
}
