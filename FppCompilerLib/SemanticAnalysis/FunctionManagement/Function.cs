using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement;

namespace FppCompilerLib.SemanticAnalysis.FunctionManagement
{
    internal class Function
    {
        public readonly TypeInfo? resultType;
        public readonly TypeInfo[] argTypes;

        private readonly Func<Data[], AssemblerCommand[]> call;

        public Function(TypeInfo resultType, TypeInfo[] argTypes, Func<Data[], AssemblerCommand[]> call)
        {
            this.resultType = resultType;
            this.argTypes = argTypes;
            this.call = call;
        }

        public Function(TypeInfo[] argTypes, Func<Data[], AssemblerCommand[]> call) : this(null, argTypes, call) { }

        public AssemblerCommand[] Call(Data[] args)
        {
            if (!CheckTypes(args.Select(arg => arg.TypeInfo).ToArray()))
                throw new ArgumentException("Bad types");
            return call(args);
        }

        public bool CheckTypes(TypeInfo[] typeInfos)
        {
            return argTypes.Zip(typeInfos).All(tup => tup.First.Equals(tup.Second));
        }
    }
}
