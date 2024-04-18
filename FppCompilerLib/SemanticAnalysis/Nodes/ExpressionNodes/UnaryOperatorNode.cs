using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.FunctionManagement;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes
{
    internal class InitedUnaryOperatorNode : InitedResultableNode
    {
        private readonly InitedResultableNode arg;
        private readonly string unaryOperator;

        public InitedUnaryOperatorNode(InitedResultableNode arg, string unaryOperator)
        {
            this.arg = arg;
            this.unaryOperator = unaryOperator;
        }

        public static InitedResultableNode ParsePrefix(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var unaryOperator = ((TerminalNode)node.childs[0]).RealValue;
            var specialOperators = new string[] { "-", "&", "*", "++", "--" };
            if (specialOperators.Contains(unaryOperator))
                unaryOperator += "unpre";
            var arg = parceTable.Parse<InitedResultableNode>((NonTerminalNode)node.childs[1]);
            return new InitedUnaryOperatorNode(arg, unaryOperator);
        }

        public static InitedResultableNode ParsePostfix(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var arg = parceTable.Parse<InitedResultableNode>((NonTerminalNode)node.childs[0]);

            var operatorChilds = ((NonTerminalNode)node.childs[1]).childs;
            if (operatorChilds.Length == 0)
                return arg;

            var unaryOperator = ((TerminalNode)operatorChilds[0]).RealValue;
            unaryOperator += "unpost";
            return new InitedUnaryOperatorNode(arg, unaryOperator);
        }

        public override TypedResultableNode UpdateTypes(Context context)
        {
            var typedArg = arg.UpdateTypes(context.GetChild());
            var operatorFunc = typedArg.ResultType.GetUnaryOperator(unaryOperator, typedArg.ResultType);
            if (typedArg.IsConstantResult)
            {
                var constResult = operatorFunc.CalculateConstant(typedArg.GetConstantResult);
                return new TypedConstantNode(constResult);
            }
            return new TypedUnaryOperatorNode(typedArg, operatorFunc);
        }
    }

    internal class TypedUnaryOperatorNode : TypedResultableNode
    {
        public override TypeInfo ResultType => operatorFunc.resultType;

        private readonly TypedResultableNode arg;
        private readonly UnaryOperator operatorFunc;

        public TypedUnaryOperatorNode(TypedResultableNode arg, UnaryOperator operatorFunc)
        {
            this.arg = arg;
            this.operatorFunc = operatorFunc;
        }

        public override UpdatedResultableNode UpdateContext(Context context, Variable? target)
        {
            if (arg.IsVariableResult)
            {
                var updatedArg = arg.UpdateContext(context.GetChild(), null);
                return new UpdatedUnaryOperatorNode(updatedArg, operatorFunc, target, updatedArg.GetVariableResult);
            }
            else
            {
                (_, var argData) = context.memoryManager.CreateTempVariable(arg.ResultType);
                var updatedArg = arg.UpdateContext(context.GetChild(), argData);
                return new UpdatedUnaryOperatorNode(updatedArg, operatorFunc, target, argData);
            }
        }
    }

    internal class UpdatedUnaryOperatorNode : UpdatedResultableNode
    {
        private readonly UpdatedResultableNode arg;
        private readonly UnaryOperator operatorFunc;
        private readonly Variable? target;
        private readonly Data argData;

        public UpdatedUnaryOperatorNode(UpdatedResultableNode arg, UnaryOperator operatorFunc, Variable? target, Data argData)
        {
            this.arg = arg;
            this.operatorFunc = operatorFunc;
            this.target = target;
            this.argData = argData;
        }

        public override AssemblerCommand[] ToCode()
        {
            var commands = arg.ToCode()
                .Concat(operatorFunc.ToCode(argData, target))
                .ToArray();
            return commands;
        }
    }
}
