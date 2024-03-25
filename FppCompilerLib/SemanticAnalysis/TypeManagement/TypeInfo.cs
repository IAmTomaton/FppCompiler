using FppCompilerLib.SemanticAnalysis.FunctionManagement;

namespace FppCompilerLib.SemanticAnalysis.TypeManagement
{
    internal abstract class TypeInfo
    {
        public abstract string Name { get; }
        public abstract int Size { get; }

        public virtual bool TryGetFunction(string name, TypeInfo[] argTypes, out Function function) =>
            throw new ArgumentException($"Type \"{Name}\" does not have function \"{name}\"");
        public virtual bool TryGetConversionFrom(TypeInfo source, out UnaryOperator conversionFunc) =>
            throw new ArgumentException($"Type \"{Name}\" cannot be converted from type \"{source.Name}\"");
        public virtual bool TryGetConversionTo(TypeInfo target, out UnaryOperator conversionFunc) =>
            throw new ArgumentException($"Type \"{Name}\" a cannot be converted to type \"{target.Name}\"");
        public virtual bool TryGetUnaryOperator(string operatorName, TypeInfo argType, out UnaryOperator operatorFunc) =>
            throw new ArgumentException($"Type \"{Name}\" does not have unary operator \"{operatorName}\"");
        public virtual bool TryGetBinaryOperator(string operatorName, TypeInfo arg0Type, TypeInfo arg1Type, out BinaryOperator operatorFunc) =>
            throw new ArgumentException($"Type \"{Name}\" does not have binary operator \"{operatorName}\"");

        public abstract override int GetHashCode();
        public abstract override bool Equals(object? obj);
    }
}
