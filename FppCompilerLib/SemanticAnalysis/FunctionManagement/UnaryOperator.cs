using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement;

namespace FppCompilerLib.SemanticAnalysis.FunctionManagement
{
    internal class UnaryOperator
    {
        public readonly TypeInfo resultType;
        public readonly TypeInfo argType;

        private readonly Func<Constant, Constant> calculateConstant;
        private readonly Func<Data, Variable?, AssemblerCommand[]> toCode;

        public UnaryOperator(TypeInfo resultType, TypeInfo argType,
            Func<Constant, Constant> calculateConstant,
            Func<Data, Variable?, AssemblerCommand[]> toCode)
        {
            this.resultType = resultType;
            this.argType = argType;
            this.calculateConstant = calculateConstant;
            this.toCode = toCode;
        }

        public Constant CalculateConstant(Constant arg)
        {
            CheckType(arg.TypeInfo);
            return calculateConstant(arg);
        }

        public AssemblerCommand[] ToCode(Data arg, Variable? target)
        {
            CheckType(arg.TypeInfo);
            return toCode(arg, target);
        }

        private void CheckType(TypeInfo typeInfo)
        {
            if (!argType.Equals(typeInfo))
                throw new ArgumentException("Bad types");
        }
    }
}
