using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement.Types.NumericTypes;

namespace FppCompilerLib.SemanticAnalysis.TypeManagement
{
    internal class TypeManager
    {
        public TypeManager() { }

        public TypeInfo ResolveType(string name)
        {
            return name switch
            {
                "int" => new Int(),
                "bool" => new Bool(),
                _ => throw new ArgumentException($"Unrecognized type {name}"),
            };
        }

        public Constant Parse(string value)
        {
            int[] machineValues;
            if (Int.TryParse(value, out machineValues))
                return new Constant(machineValues, new Int());
            if (Bool.TryParse(value, out machineValues))
                return new Constant(machineValues, new Bool());
            throw new ArgumentException($"Cannot parse constant {value}");
        }

        public TypeManager GetChild()
        {
            return this;
        }
    }
}
