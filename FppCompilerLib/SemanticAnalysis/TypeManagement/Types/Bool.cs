using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.FunctionManagement;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;

namespace FppCompilerLib.SemanticAnalysis.TypeManagement.Types
{
    internal class Bool : TypeInfo
    {
        public override string Name => "bool";
        public override int Size => 1;

        public static bool TryParse(string value, out int[] result)
        {
            if (!bool.TryParse(value, out bool resultInt))
            {
                result = Array.Empty<int>();
                return false;
            }
            result = new int[] { resultInt ? 1 : 0 };
            return true;
        }

        public override BinaryOperator GetBinaryOperator(string operatorName, TypeInfo arg0Type, TypeInfo arg1Type)
        {
            if (arg0Type is not Bool arg0Int || arg1Type is not Bool arg1Int) throw new ArgumentException($"Type Bool dose not have binary operator for {arg0Type.Name} and {arg1Type.Name}");
            var operatorFunc = new BinaryOperator(new Bool(), new Bool(), new Bool(),
                (arg0, arg1) => BinaryOperatorCalculateConstant(arg0, arg1, operatorName),
                (arg0, arg1, target) => BinaryOperatorToCode(operatorName, arg0, arg1, target));
            return operatorFunc;
        }

        private static Constant BinaryOperatorCalculateConstant(Constant arg0, Constant arg1, string op)
        {
            var funcs = new Dictionary<string, Func<int, int, int>>()
            {
                { "&", (arg0, arg1) => arg0 != 0 && arg1 != 0 ? 1 : 0},
                { "&&", (arg0, arg1) => arg0 != 0 && arg1 != 0 ? 1 : 0 },
                { "|", (arg0, arg1) => arg0 != 0 || arg1 != 0 ? 1 : 0 },
                { "||", (arg0, arg1) => arg0 != 0 || arg1 != 0 ? 1 : 0 },
                { "^", (arg0, arg1) => arg0 != 0 ^ arg1 != 0 ? 1 : 0 }
            };

            return new Constant(new int[] { funcs[op](arg0.machineValues[0], arg1.machineValues[1]) }, new Int());
        }

        private static AssemblerCommand[] BinaryOperatorToCode(string op, Data arg0, Data arg1, Variable? target)
        {
            if (arg0 is Constant constant0 && arg1 is Constant constant1)
            {
                if (target is not null)
                {
                    var result = BinaryOperatorCalculateConstant(constant0, constant1, op);
                    return MemoryManager.MoveOrPut(result, target);
                }
                else
                    return Array.Empty<AssemblerCommand>();
            }

            var operatorToCmd = new Dictionary<string, string>()
            {
                { "&", "and" },
                { "&&", "and" },
                { "|", "or" },
                { "||", "or" },
                { "^", "xor" }
            };

            var commands = Enumerable.Empty<AssemblerCommand>();
            int arg0Addr;
            int arg1Addr;

            if (arg0 is Constant constArg0)
            {
                commands = commands.Concat(MemoryManager.MoveOrPut(constArg0, 1));
                arg0Addr = 1;
            }
            else if (arg0 is Variable variable0)
                arg0Addr = variable0.address;
            else
                throw new ArgumentException();

            if (arg1 is Constant constArg1)
            {
                commands = commands.Concat(MemoryManager.MoveOrPut(constArg1, 2));
                arg1Addr = 2;
            }
            else if (arg1 is Variable variable1)
                arg1Addr = variable1.address;
            else
                throw new ArgumentException();

            if (target is null)
                commands = commands.Append(new AssemblerCommand(operatorToCmd[op], arg0Addr, arg1Addr));
            else
                commands = commands.Append(new AssemblerCommand(operatorToCmd[op], arg0Addr, arg1Addr, target.address));

            return commands.ToArray();
        }

        public override UnaryOperator GetUnaryOperator(string operatorName, TypeInfo argType)
        {
            if (argType is not Bool argInt) throw new ArgumentException($"Type bool dose not have unary operator for {argType.Name}");
            var operatorFunc = new UnaryOperator(new Bool(), new Bool(),
                (arg) => UnaryOperatorCalculateConstant(arg, operatorName),
                (arg, target) => UnaryOperatorToCode(operatorName, arg, target));
            return operatorFunc;
        }

        private static Constant UnaryOperatorCalculateConstant(Constant arg, string op)
        {
            var funcs = new Dictionary<string, Func<int, int>>()
            {
                { "!", (arg) => arg == 0 ? 1 : 0 }
            };

            return new Constant(new int[] { funcs[op](arg.machineValues[0]) }, new Bool());
        }

        private AssemblerCommand[] UnaryOperatorToCode(string op, Data arg, Variable? target)
        {
            if (arg is Constant constant)
            {
                if (target is not null)
                {
                    var result = UnaryOperatorCalculateConstant(constant, op);
                    return MemoryManager.MoveOrPut(result, target);
                }
                else
                    return Array.Empty<AssemblerCommand>();
            }

            var handlers = new Dictionary<string, Func<Variable, Variable?, AssemblerCommand[]>>()
            {
                { "!", (variable, target) =>
                    {
                        if (target is null)
                            return new[] { new AssemblerCommand("not", variable.address) };
                        else
                            return new[] { new AssemblerCommand("not", variable.address, 0, target.address) };
                    }
                }
            };

            if (arg is Variable variable)
            {
                return handlers[op](variable, target);
            }

            throw new ArgumentException();
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not Bool) return false;
            return true;
        }
    }
}
