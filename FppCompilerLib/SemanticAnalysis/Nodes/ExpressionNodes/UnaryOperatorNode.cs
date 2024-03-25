using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.FunctionManagement;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes
{
    internal class UnaryOperatorNode : ResultableNode
    {
        private readonly ResultableNode arg;
        private readonly string unaryOperator;
        private readonly Variable? tempVariable;
        private readonly UnaryOperator? operatorFunc;

        public UnaryOperatorNode(ResultableNode arg, string unaryOperator)
        {
            this.arg = arg;
            this.unaryOperator = unaryOperator;
        }

        private UnaryOperatorNode(ResultableNode arg, string unaryOperator, UnaryOperator operatorFunc) : this(arg, unaryOperator)
        {
            resultType = operatorFunc.resultType;
            this.operatorFunc = operatorFunc;
        }

        private UnaryOperatorNode(ResultableNode arg, string unaryOperator, UnaryOperator operatorFunc, Data staticResult) :
            this(arg, unaryOperator, operatorFunc)
        {
            isStaticResult = true;
            this.staticResult = staticResult;
        }

        private UnaryOperatorNode(ResultableNode arg, string unaryOperator, UnaryOperator operatorFunc, Variable target, Variable? tempVariable=null) :
            this(arg, unaryOperator, operatorFunc)
        {
            isStaticResult = false;
            this.target = target;
            this.tempVariable = tempVariable;
        }

        public static ResultableNode ParsePrefix(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var unaryOperator = ((TerminalNode)node.childs[0]).RealValue;
            var specialOperators = new string[] { "-", "&", "*", "++", "--" };
            if (specialOperators.Contains(unaryOperator))
                unaryOperator += "unpre";
            var arg = parceTable.Parse<ResultableNode>((NonTerminalNode)node.childs[1]);
            return new UnaryOperatorNode(arg, unaryOperator);
        }

        public static ResultableNode ParsePostfix(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var arg = parceTable.Parse<ResultableNode>((NonTerminalNode)node.childs[0]);

            var operatorChilds = ((NonTerminalNode)node.childs[1]).childs;
            if (operatorChilds.Length == 0)
                return arg;

            var unaryOperator = ((TerminalNode)operatorChilds[0]).RealValue;
            unaryOperator += "unpost";
            return new UnaryOperatorNode(arg, unaryOperator);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not UnaryOperatorNode other) return false;
            return unaryOperator == other.unaryOperator && arg.Equals(other.arg);
        }

        public override int GetHashCode()
        {
            return unaryOperator.GetHashCode() + arg.GetHashCode() * 37;
        }

        public override UnaryOperatorNode UpdateTypes(Context context)
        {
            var typedArg = arg.UpdateTypes(context.GetChild());
            typedArg.ResultType.TryGetUnaryOperator(unaryOperator, typedArg.ResultType, out var operatorFunc);
            return new UnaryOperatorNode(typedArg, unaryOperator, operatorFunc);
        }

        public override UnaryOperatorNode UpdateContext(Context context, Variable? target)
        {
            if (operatorFunc is null)
                throw new InvalidOperationException("Call UpdateTypes before call UpdateContext.");

            var updatedArg = arg.UpdateContext(context.GetChild());
            Variable? tempVariable = null;

            if (!updatedArg.IsStaticResult)
            {
                (_, tempVariable) = context.memoryManager.CreateTempVariable(updatedArg.ResultType);
                updatedArg = arg.UpdateContext(context.GetChild(), tempVariable);
            }

            if (updatedArg.IsStaticResult && updatedArg.StaticResult is Constant constant)
            {
                var result = operatorFunc.CalculateConstant(constant);
                return new UnaryOperatorNode(updatedArg, unaryOperator, operatorFunc, result);
            }
            else
            {
                return new UnaryOperatorNode(updatedArg, unaryOperator, operatorFunc, target, tempVariable);
            }
        }

        public override AssemblerCommand[] ToCode()
        {
            if (IsStaticResult) return Array.Empty<AssemblerCommand>();

            Data argResult;
            if (arg.IsStaticResult)
                argResult = arg.StaticResult;
            else
                argResult = tempVariable;

            IEnumerable<AssemblerCommand> commands = arg.ToCode();
            commands = commands.Concat(operatorFunc.ToCode(argResult, target));
            return commands.ToArray();
        }
    }
}
