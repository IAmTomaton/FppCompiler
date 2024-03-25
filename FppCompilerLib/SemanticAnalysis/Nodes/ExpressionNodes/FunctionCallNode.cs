using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.FunctionManagement;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement.Types.NumericTypes;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes
{
    internal class FunctionCallNode : ResultableNode
    {
        private readonly string name;
        private readonly ResultableNode[] args;

        private readonly Function function;
        private readonly Variable?[] tempVariables;

        public FunctionCallNode(string name, ResultableNode[] args)
        {
            this.name = name;
            this.args = args;
        }

        public FunctionCallNode(string name, ResultableNode[] args, Function function) : this(name, args)
        {
            this.function = function;
        }

        public FunctionCallNode(string name, ResultableNode[] args, Function function, Variable?[] tempVariables): this(name, args, function)
        {
            this.tempVariables = tempVariables;
        }

        public static FunctionCallNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            var name = node.childs[0].AsTerminalNode.RealValue;
            var args = ParseArgs(node.childs[2].AsNonTerminalNode, parceTable);

            return new FunctionCallNode(name, args);
        }

        private static ResultableNode[] ParseArgs(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            if (node.childs.Length == 0)
                return new ResultableNode[0];

            var args = new List<ResultableNode>();

            while (node.childs.Length > 1)
            {
                args.Add(parceTable.Parse<ResultableNode>(node.childs[0].AsNonTerminalNode));
                node = node.childs[2].AsNonTerminalNode;
            }
            args.Add(parceTable.Parse<ResultableNode>(node.childs[0].AsNonTerminalNode));

            return args.ToArray();
        }

        public override FunctionCallNode UpdateTypes(Context context)
        {
            var typedArgs = args.Select(arg => arg.UpdateTypes(context.GetChild())).ToArray();
            var argTypes = typedArgs.Select(arg => arg.ResultType).ToArray();
            var function = context.functionManager.ResolveFunction(name, argTypes);
            return new FunctionCallNode(name, typedArgs, function);
        }

        public override FunctionCallNode UpdateContext(Context context, Variable? target)
        {
            var tempVariables = new Variable?[args.Length];
            var upadatedArgs = new ResultableNode[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var upadatedArg = args[i].UpdateContext(context.GetChild());

                if (!upadatedArg.IsStaticResult)
                {
                    (_, var tempVariable) = context.memoryManager.CreateTempVariable(upadatedArg.ResultType);
                    tempVariables[i] = tempVariable;
                    upadatedArg = args[i].UpdateContext(context.GetChild(), tempVariable);
                    upadatedArgs[i] = upadatedArg;
                }
                else
                {
                    tempVariables[i] = null;
                    upadatedArgs[i] = upadatedArg;
                }
            }

            return new FunctionCallNode(name, upadatedArgs, function, tempVariables);
        }

        public override AssemblerCommand[] ToCode()
        {
            var commands = args.SelectMany(arg => arg.ToCode());
            var argsData = Enumerable.Range(0, args.Length)
                .Select(i => tempVariables[i] ?? args[i].StaticResult)
                .ToArray();
            commands = commands.Concat(function.Call(argsData));
            return commands.ToArray();
        }

        public override bool Equals(object? obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
