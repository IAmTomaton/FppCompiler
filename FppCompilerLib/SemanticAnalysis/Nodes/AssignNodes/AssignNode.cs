using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes;
using FppCompilerLib.SyntacticalAnalysis;
using System;

namespace FppCompilerLib.SemanticAnalysis.Nodes.AssignNodes
{
    internal class AssignNode : SemanticNode
    {
        private readonly AssignableNode target;
        private readonly ResultableNode expression;
        private readonly Variable? tempVariableTarget;
        private readonly Variable? tempVariableExp;

        public AssignNode(AssignableNode target, ResultableNode expression)
        {
            this.target = target;
            this.expression = expression;
        }

        public AssignNode(AssignableNode target, ResultableNode expression, Variable? tempVariableTarget = null, Variable? tempVariableExp = null) : this(target, expression)
        {
            this.tempVariableTarget = tempVariableTarget;
            this.tempVariableExp = tempVariableExp;
        }

        public static AssignNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var target = parceTable.Parse<AssignableNode>(node.childs[0].AsNonTerminalNode);
            var expression = parceTable.Parse<ResultableNode>(node.childs[2].AsNonTerminalNode);
            return new AssignNode(target, expression);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not AssignNode other) return false;
            return target.Equals(other.target) && expression.Equals(other.expression);
        }

        public override int GetHashCode()
        {
            return target.GetHashCode() + expression.GetHashCode() * 37;
        }

        public override AssignNode UpdateTypes(Context context)
        {
            var typedTarget = target.UpdateTypes(context.GetChild());
            var typedExpression = expression.UpdateTypes(context.GetChild());
            if (!typedTarget.TargetType.Equals(typedExpression.ResultType))
                throw new InvalidOperationException();
            return new AssignNode(typedTarget, typedExpression);
        }

        public override AssignNode UpdateContext(Context context)
        {
            Variable? tempVariableTarget = null;
            Variable? tempVariableExp = null;

            var updatedTarget = target.UpdateContext(context.GetChild());
            //if (!updatedTarget.IsStaticTarget)
            //{
            //    (_, tempVariableTarget) = context.memoryManager.CreateTempVariable(updatedTarget.TargetType);
            //    updatedTarget = target.UpdateContext(context.GetChild(), tempVariableTarget);
            //}

            var updatedExp = expression.UpdateContext(context.GetChild(), (Variable)updatedTarget.StaticTarget);
            //if (updatedTarget.IsStaticTarget && updatedTarget.StaticTarget is Variable varTarget)
            //{
            //    updatedExp = expression.UpdateContext(context.GetChild(), varTarget);
            //}
            //else if (!updatedExp.IsStaticResult)
            //{
            //    (_, tempVariableExp) = context.memoryManager.CreateTempVariable(updatedExp.ResultType);
            //    updatedExp = expression.UpdateContext(context.GetChild(), tempVariableExp);
            //}

            return new AssignNode(updatedTarget, updatedExp, tempVariableTarget, tempVariableExp);
        }

        public override AssemblerCommand[] ToCode()
        {
            // TODO
            // Сделать проверку на присвоение по указателю. Присвоение по константе только по указателю.

            //Data targetResult;
            //if (target.IsStaticTarget)
            //    targetResult = target.StaticTarget;
            //else
            //    targetResult = tempVariableTarget;

            //Data expResult;
            //if (expression.IsStaticResult)
            //    expResult = expression.StaticResult;
            //else
            //    expResult = tempVariableExp;

            //IEnumerable<AssemblerCommand> commands = target.ToCode();
            IEnumerable<AssemblerCommand> commands = Array.Empty<AssemblerCommand>();

            //if (expression.IsStaticResult && expression.StaticResult is Constant constResult && target.IsStaticTarget && target.StaticTarget is Variable varTarget)
            //{
            //    commands = commands.Concat(MemoryManager.MoveOrPut(constResult, varTarget));
            //}

            commands = commands.Concat(expression.ToCode());
            return commands.ToArray();
        }
    }
}
