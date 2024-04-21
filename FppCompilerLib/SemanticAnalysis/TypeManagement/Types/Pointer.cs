using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.FunctionManagement;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;

namespace FppCompilerLib.SemanticAnalysis.TypeManagement.Types
{
    internal class Pointer : TypeInfo
    {
        public override string Name => "pointerType";
        public override int Size => 1;
        public TypeInfo ChildType => childType;

        private readonly TypeInfo childType;

        public Pointer(TypeInfo childType)
        {
            this.childType = childType;
        }

        public override BinaryOperator GetBinaryOperator(string operatorName, TypeInfo arg0Type, TypeInfo arg1Type)
        {
            if (arg0Type is not Pointer arg0Int || arg1Type is not Int arg1Int) throw new ArgumentException($"Type int dose not have binary operator for {arg0Type.Name} and {arg1Type.Name}");
            if (!operators.Contains(operatorName))
                throw new ArgumentException($"Type {Name} dose not have binary operator {operatorName}");

            var operatorFunc = new BinaryOperator(this, this, new Int(),
                (arg0, arg1) => BinaryOperatorCalculateConstant(arg0, arg1, operatorName),
                (arg0, arg1, target) => BinaryOperatorToCode(operatorName, arg0, arg1, target));
            return operatorFunc;
        }

        private Constant BinaryOperatorCalculateConstant(Constant arg0, Constant arg1, string op)
        {
            var funcs = new Dictionary<string, Func<int, int, int>>()
            {
                { "+", (arg0, arg1) => arg0 + arg1 * ChildType.Size },
                { "-", (arg0, arg1) => arg0 - arg1 * ChildType.Size }
            };

            return new Constant(new int[] { funcs[op](arg0.machineValues[0], arg1.machineValues[0]) }, new Int());
        }

        private AssemblerCommand[] BinaryOperatorToCode(string op, Data arg0, Data arg1, Variable? target)
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
                { "+", "add" },
                { "-", "sub" }
            };

            var commands = Enumerable.Empty<AssemblerCommand>();
            int arg0Addr;
            int arg1Addr = 2;

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
                var newConst = new Constant(constArg1.machineValues[0] * ChildType.Size, new Int());
                commands = commands.Concat(MemoryManager.MoveOrPut(newConst, 2));
            }
            else if (arg1 is Variable variable1)
            {
                commands = commands.Concat(MemoryManager.MoveOrPut(new Constant(ChildType.Size, new Int()), 2))
                    .Append(new AssemblerCommand("mul", variable1.address, 2, 2));
            }
            else
                throw new ArgumentException();

            if (target is null)
                commands = commands.Append(new AssemblerCommand(operatorToCmd[op], arg0Addr, arg1Addr));
            else
                commands = commands.Append(new AssemblerCommand(operatorToCmd[op], arg0Addr, arg1Addr, target.address));

            return commands.ToArray();
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() + ChildType.GetHashCode() * 37;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not Pointer other) return false;
            return ChildType.Equals(other.ChildType);
        }

        private static string[] operators = new string[] { "+", "-" };
    }
}
