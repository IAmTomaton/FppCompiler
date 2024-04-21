using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes;
using FppCompilerLib.SemanticAnalysis.TypeManagement.Types;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.AssignNodes
{
    internal class InitedAssignPointerNode : InitedSemanticNode
    {
        private readonly InitedResultableNode target;
        private readonly InitedResultableNode expression;

        public InitedAssignPointerNode(InitedResultableNode target, InitedResultableNode expression)
        {
            this.target = target;
            this.expression = expression;
        }

        public static InitedAssignPointerNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var target = parceTable.Parse<InitedResultableNode>(node.childs[1].AsNonTerminalNode);
            var expression = parceTable.Parse<InitedResultableNode>(node.childs[3].AsNonTerminalNode);
            return new InitedAssignPointerNode(target, expression);
        }

        public override TypedAssignPointerNode UpdateTypes(Context context)
        {
            var typedTarget = target.UpdateTypes(context.GetChild());
            var typedExpression = expression.UpdateTypes(context.GetChild());

            if (typedTarget.ResultType is not Pointer pointer)
                throw new ArgumentException("The result must be of type pointerType");

            if (!pointer.ChildType.Equals(typedExpression.ResultType))
                throw new ArgumentException($"An attempt to assign a pointerType of type {pointer.ChildType.Name} to a value of type {typedExpression.ResultType.Name}");

            return new TypedAssignPointerNode(typedTarget, typedExpression, pointer);
        }
    }

    internal class TypedAssignPointerNode : TypedSemanticNode
    {
        private readonly TypedResultableNode target;
        private readonly TypedResultableNode expression;
        private readonly Pointer pointerType;

        public TypedAssignPointerNode(TypedResultableNode target, TypedResultableNode expression, Pointer pointer)
        {
            this.target = target;
            this.expression = expression;
            this.pointerType = pointer;
        }

        public override UpdatedSemanticNode UpdateContext(Context context)
        {
            if (target.IsConstantResult)
            {
                var constantResult = target.GetConstantResult;
                var variable = new Variable(constantResult.machineValues[0], pointerType.ChildType);
                var updatedExpression = expression.UpdateContext(context.GetChild(), variable);
                return new UpdatedAssignVariableNode(new UpdatedVariableNode(variable, null), updatedExpression);
            }
            else
            {
                Variable pointer;
                UpdatedResultableNode updatedTarget;
                Data expData;
                UpdatedResultableNode updatedExpression;


                if (target.IsVariableResult)
                {
                    updatedTarget = target.UpdateContext(context.GetChild());
                    pointer = updatedTarget.GetVariableResult;
                }
                else
                {
                    (_, pointer) = context.memoryManager.CreateTempVariable(target.ResultType);
                    updatedTarget = target.UpdateContext(context.GetChild(), pointer);
                }

                if (expression.IsConstantResult)
                {
                    updatedExpression = expression.UpdateContext(context.GetChild());
                    expData = expression.GetConstantResult;
                }
                else if (expression.IsVariableResult)
                {
                    updatedExpression = expression.UpdateContext(context.GetChild());
                    expData = updatedExpression.GetVariableResult;
                }
                else
                {
                    (_, var tempVariable) = context.memoryManager.CreateTempVariable(target.ResultType);
                    expData = tempVariable;
                    updatedExpression = expression.UpdateContext(context.GetChild(), tempVariable);
                }

                return new UpdatedAssignPointerNode(updatedTarget, pointer, updatedExpression, expData);
            }
        }
    }

    internal class UpdatedAssignPointerNode : UpdatedSemanticNode
    {
        private readonly UpdatedResultableNode target;
        private readonly Variable pointer;
        private readonly UpdatedResultableNode expression;
        private readonly Data expData;

        public UpdatedAssignPointerNode(UpdatedResultableNode target, Variable pointer, UpdatedResultableNode expression, Data expData)
        {
            this.target = target;
            this.pointer = pointer;
            this.expression = expression;
            this.expData = expData;
        }

        public override AssemblerCommand[] ToCode()
        {
            var commands = target.ToCode()
                .Concat(expression.ToCode())
                .Concat(MemoryManager.MoveOrPutToPointer(expData, pointer))
                .ToArray();

            return commands;
        }
    }
}
