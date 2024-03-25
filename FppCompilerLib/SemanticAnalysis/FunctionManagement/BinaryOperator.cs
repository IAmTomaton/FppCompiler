using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement;

namespace FppCompilerLib.SemanticAnalysis.FunctionManagement
{
    internal class BinaryOperator
    {
        public readonly TypeInfo resultType;
        public readonly TypeInfo arg0Type;
        public readonly TypeInfo arg1Type;

        private readonly Func<Constant, Constant, Constant> calculateConstant;
        private readonly Func<Data, Data, Variable?, AssemblerCommand[]> toCode;

        public BinaryOperator(TypeInfo resultType, TypeInfo arg0Type, TypeInfo arg1Type,
            Func<Constant, Constant, Constant> calculateConstant,
            Func<Data, Data, Variable?, AssemblerCommand[]> toCode)
        {
            this.resultType = resultType;
            this.arg0Type = arg0Type;
            this.arg1Type = arg1Type;
            this.calculateConstant = calculateConstant;
            this.toCode = toCode;
        }

        public Constant CalculateConstant(Constant arg0, Constant arg1)
        {
            CheckType(arg0.TypeInfo, arg1.TypeInfo);
            return calculateConstant(arg0, arg1);
        }

        public AssemblerCommand[] ToCode(Data arg0, Data arg1, Variable? target)
        {
            CheckType(arg0.TypeInfo, arg1.TypeInfo);
            return toCode(arg0, arg1, target);
        }

        private void CheckType(TypeInfo typeInfo0, TypeInfo typeInfo1)
        {
            if (!arg0Type.Equals(typeInfo0) && !arg1Type.Equals(typeInfo1))
                throw new ArgumentException("Bad types");
        }
    }
}
