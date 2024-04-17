using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.AssignNodes
{
    internal class InitedAssignNode : InitedSemanticNode
    {
        private readonly InitedAssignableNode target;
        private readonly InitedResultableNode expression;

        public InitedAssignNode(InitedAssignableNode target, InitedResultableNode expression)
        {
            this.target = target;
            this.expression = expression;
        }

        public static InitedAssignNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var target = parceTable.Parse<InitedAssignableNode>(node.childs[0].AsNonTerminalNode);
            var expression = parceTable.Parse<InitedResultableNode>(node.childs[2].AsNonTerminalNode);
            return new InitedAssignNode(target, expression);
        }

        public override TypedAssignNode UpdateTypes(Context context)
        {
            var typedTarget = target.UpdateTypes(context.GetChild());
            var typedExpression = expression.UpdateTypes(context.GetChild());

            if (!typedTarget.TargetType.Equals(typedExpression.ResultType))
                throw new InvalidOperationException($"An attempt to assign a variable of type {typedTarget.TargetType.Name} to a value of type {typedExpression.ResultType.Name}");

            return new TypedAssignNode(typedTarget, typedExpression);
        }
    }

    internal class TypedAssignNode : TypedSemanticNode
    {
        private readonly TypedAssignableNode target;
        private readonly TypedResultableNode expression;

        public TypedAssignNode(TypedAssignableNode target, TypedResultableNode expression)
        {
            this.target = target;
            this.expression = expression;
        }

        public override UpdatedAssignNode UpdateContext(Context context)
        {
            UpdatedAssignableNode updatedTarget;
            UpdatedResultableNode updatedExpression;

            if (target.AssignableNodeType == AssignableNodeType.Variable)
            {
                updatedTarget = target.UpdateContext(context.GetChild());
                updatedExpression = expression.UpdateContext(context.GetChild(), updatedTarget.VariableTraget);
            }
            else if (target.AssignableNodeType != AssignableNodeType.ConstantPointer)
            {
                Variable pointer;

                if (target.AssignableNodeType == AssignableNodeType.VariablePointer)
                {
                    updatedTarget = target.UpdateContext(context.GetChild());
                    pointer = updatedTarget.VariableTraget;
                }
                else if (target.AssignableNodeType == AssignableNodeType.CalculatedPointer)
                {
                    (_, pointer) = context.memoryManager.CreateTempVariable(target.TargetType);
                    updatedTarget = target.UpdateContext(context.GetChild(), pointer);
                }

                if (expression.IsConstantResult)
                {
                    // TODO
                    throw new NotImplementedException();
                }
                else if (expression.IsVariableResult)
                {
                    // TODO
                    throw new NotImplementedException();
                }
                else
                {
                    // TODO
                    throw new NotImplementedException();
                }
            }
            else
            {
                if (expression.IsConstantResult)
                {
                    // TODO
                    throw new NotImplementedException();
                }
                else if (expression.IsVariableResult)
                {
                    // TODO
                    throw new NotImplementedException();
                }
                else
                {
                    // TODO
                    throw new NotImplementedException();
                }
            }

            return new UpdatedAssignNode(updatedTarget, updatedExpression);
        }
    }

    internal class UpdatedAssignNode : UpdatedSemanticNode
    {
        private readonly UpdatedAssignableNode target;
        private readonly UpdatedResultableNode expression;

        public UpdatedAssignNode(UpdatedAssignableNode target, UpdatedResultableNode expression)
        {
            this.target = target;
            this.expression = expression;
        }

        public override AssemblerCommand[] ToCode()
        {
            // TODO
            // Сделать проверку на присвоение по указателю. Присвоение по константе только по указателю.

            var commands = target.ToCode()
                .Concat(expression.ToCode())
                .ToArray();
            return commands;
        }
    }
}
