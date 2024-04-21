using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.AssignNodes
{
    internal class InitedAssignVariableNode : InitedSemanticNode
    {
        private readonly InitedVariableNode target;
        private readonly InitedResultableNode expression;

        public InitedAssignVariableNode(InitedVariableNode target, InitedResultableNode expression)
        {
            this.target = target;
            this.expression = expression;
        }

        public static InitedAssignVariableNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var target = parceTable.Parse<InitedVariableNode>(node.childs[0].AsNonTerminalNode);
            var expression = parceTable.Parse<InitedResultableNode>(node.childs[2].AsNonTerminalNode);
            return new InitedAssignVariableNode(target, expression);
        }

        public override TypedAssignVariableNode UpdateTypes(Context context)
        {
            var typedTarget = target.UpdateTypes(context.GetChild());
            var typedExpression = expression.UpdateTypes(context.GetChild());

            if (!typedTarget.ResultType.Equals(typedExpression.ResultType))
                throw new ArgumentException($"An attempt to assign a variable of type {typedTarget.ResultType.Name} to a value of type {typedExpression.ResultType.Name}");

            return new TypedAssignVariableNode(typedTarget, typedExpression);
        }
    }

    internal class TypedAssignVariableNode : TypedSemanticNode
    {
        private readonly TypedVariableNode target;
        private readonly TypedResultableNode expression;

        public TypedAssignVariableNode(TypedVariableNode target, TypedResultableNode expression)
        {
            this.target = target;
            this.expression = expression;
        }

        public override UpdatedAssignVariableNode UpdateContext(Context context)
        {
            var updatedTarget = target.UpdateContext(context.GetChild(), null);
            var updatedExpression = expression.UpdateContext(context.GetChild(), updatedTarget.GetVariableResult);

            return new UpdatedAssignVariableNode(updatedTarget, updatedExpression);
        }
    }

    internal class UpdatedAssignVariableNode : UpdatedSemanticNode
    {
        private readonly UpdatedVariableNode target;
        private readonly UpdatedResultableNode expression;

        public UpdatedAssignVariableNode(UpdatedVariableNode target, UpdatedResultableNode expression)
        {
            this.target = target;
            this.expression = expression;
        }

        public override AssemblerCommand[] ToCode()
        {
            var commands = expression.ToCode().ToArray();
            return commands;
        }
    }
}
