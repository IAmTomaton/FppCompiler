using FppCompilerLib.SemanticAnalysis.FunctionManagement;

namespace FppCompilerLib.SemanticAnalysis.TypeManagement
{
    internal abstract class TypeInfo
    {
        public abstract string Name { get; }
        public abstract int Size { get; }

        // Placeholders for functions
        public virtual Function GetFunction(string name, TypeInfo[] argTypes) =>
            throw new ArgumentException($"Type \"{Name}\" does not have function \"{name}\"");

        public virtual UnaryOperator GetConversionFrom(TypeInfo source) =>
            throw new ArgumentException($"Type \"{Name}\" cannot be converted from type \"{source.Name}\"");

        public virtual UnaryOperator GetConversionTo(TypeInfo target) =>
            throw new ArgumentException($"Type \"{Name}\" a cannot be converted to type \"{target.Name}\"");

        public virtual UnaryOperator GetUnaryOperator(string operatorName, TypeInfo argType) =>
            throw new ArgumentException($"Type \"{Name}\" does not have unary operator \"{operatorName}\"");

        public virtual BinaryOperator GetBinaryOperator(string operatorName, TypeInfo arg0Type, TypeInfo arg1Type) =>
            throw new ArgumentException($"Type \"{Name}\" does not have binary operator \"{operatorName}\"");

        public abstract override int GetHashCode();
        public abstract override bool Equals(object? obj);
    }
}
