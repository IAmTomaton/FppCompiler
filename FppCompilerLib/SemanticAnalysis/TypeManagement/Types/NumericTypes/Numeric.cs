namespace FppCompilerLib.SemanticAnalysis.TypeManagement.Types.NumericTypes
{
    internal abstract class Numeric : TypeInfo
    {
        public abstract int Priority { get; }
        public abstract Numeric BaseType { get; }

        public static Numeric ResolveNumericCast(Numeric[] argTypes)
        {
            if (argTypes.Length == 0) throw new ArgumentException("Empty sequence");
            return argTypes.Select(n => n.BaseType).MaxBy(n => n.Priority);
        }

        public static Numeric ResolveNumericCast(Numeric arg0Type, Numeric arg1Type)
        {
            return ResolveNumericCast(new Numeric[] { arg0Type, arg1Type });
        }
    }
}
