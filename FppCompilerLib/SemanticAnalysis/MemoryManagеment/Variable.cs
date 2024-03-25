using FppCompilerLib.SemanticAnalysis.TypeManagement;

namespace FppCompilerLib.SemanticAnalysis.MemoryManagement
{
    internal class Variable : Data
    {
        public readonly int address;
        public override TypeInfo TypeInfo => typeInfo;

        private readonly TypeInfo typeInfo;

        public Variable(int address, TypeInfo typeInfo)
        {
            this.address = address;
            this.typeInfo = typeInfo;
        }

        public Variable Copy() => new(address, typeInfo);
    }
}
