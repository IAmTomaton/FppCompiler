using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.FunctionManagement;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes
{
    internal class InitedFunctionCallNode : InitedResultableNode
    {
        private readonly string name;
        private readonly InitedResultableNode[] args;

        public InitedFunctionCallNode(string name, InitedResultableNode[] args)
        {
            this.name = name;
            this.args = args;
        }

        public static InitedFunctionCallNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var name = node.childs[0].AsTerminalNode.RealValue;
            var args = ParseArgs(node.childs[2].AsNonTerminalNode, parceTable);

            return new InitedFunctionCallNode(name, args);
        }

        private static InitedResultableNode[] ParseArgs(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            if (node.childs.Length == 0)
                return Array.Empty<InitedResultableNode>();

            var args = new List<InitedResultableNode>();

            while (node.childs.Length > 1)
            {
                args.Add(parceTable.Parse<InitedResultableNode>(node.childs[0].AsNonTerminalNode));
                node = node.childs[2].AsNonTerminalNode;
            }
            args.Add(parceTable.Parse<InitedResultableNode>(node.childs[0].AsNonTerminalNode));

            return args.ToArray();
        }

        public override TypedFunctionCallNode UpdateTypes(Context context)
        {
            var typedArgs = args.Select(arg => arg.UpdateTypes(context.GetChild())).ToArray();
            var argTypes = typedArgs.Select(arg => arg.ResultType).ToArray();
            var function = context.functionManager.ResolveFunction(name, argTypes);
            return new TypedFunctionCallNode(typedArgs, function);
        }
    }

    internal class TypedFunctionCallNode : TypedResultableNode
    {
        public override TypeInfo ResultType => function.resultType ?? throw new InvalidOperationException("Can't get return type from void function");

        private readonly TypedResultableNode[] args;
        private readonly Function function;

        public TypedFunctionCallNode(TypedResultableNode[] args, Function function)
        {
            this.args = args;
            this.function = function;
        }

        public override UpdatedFunctionCallNode UpdateContext(Context context, Variable? target)
        {
            var upadatedArgs = new UpdatedResultableNode?[args.Length];
            var argsData = new Data[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                (argsData[i], upadatedArgs[i]) = UpdateArg(args[i], context);
            }

            return new UpdatedFunctionCallNode(upadatedArgs, function, argsData);
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

    internal class UpdatedFunctionCallNode : UpdatedResultableNode
    {
        private readonly UpdatedResultableNode?[] args;
        private readonly Function function;
        private readonly Data[] argsData;

        public UpdatedFunctionCallNode(UpdatedResultableNode?[] args, Function function, Data[] argsData)
        {
            this.args = args;
            this.function = function;
            this.argsData = argsData;
        }

        public override AssemblerCommand[] ToCode()
        {
            var commands = args.SelectMany(arg => arg?.ToCode() ?? Enumerable.Empty<AssemblerCommand>())
                .Concat(function.Call(argsData))
                .ToArray();
            return commands;
        }
    }
}
