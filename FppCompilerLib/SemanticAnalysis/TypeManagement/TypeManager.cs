using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement.Types.NumericTypes;

namespace FppCompilerLib.SemanticAnalysis.TypeManagement
{
    internal class TypeManager
    {
        private readonly TypeInfo[] typesForResolve = new TypeInfo[] { new Int(), new Bool() };

        public TypeManager() { }

        /// <summary>
        /// Returns a type by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public TypeInfo ResolveType(string name)
        {
            var targetType = typesForResolve.FirstOrDefault(t => t.Name == name);
            if (targetType != null)
                return targetType;

            throw new ArgumentException($"Unrecognized type {name}");
        }

        /// <summary>
        /// Converts a string to a constant with the desired type
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
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
