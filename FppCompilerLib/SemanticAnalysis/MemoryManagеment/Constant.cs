using FppCompilerLib.SemanticAnalysis.TypeManagement;

namespace FppCompilerLib.SemanticAnalysis.MemoryManagement
{
    internal class Constant : Data
    {
        public readonly int[] machineValues;
        public override TypeInfo TypeInfo => typeInfo;

        private readonly TypeInfo typeInfo;

        public Constant(int[] machineValues, TypeInfo typeInfo)
        {
            this.machineValues = machineValues;
            this.typeInfo = typeInfo;
        }

        public Constant(int machineValue, TypeInfo typeInfo) : this(new int[] { machineValue }, typeInfo) { }

        public Constant Copy() => new(machineValues, typeInfo);
    }
}
