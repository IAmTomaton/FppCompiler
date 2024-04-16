namespace FppCompilerLib.SemanticAnalysis.TypeManagement.Types.NumericTypes
{
    /// <summary>
    /// Base class for numeric types
    /// </summary>
    internal abstract class Numeric : TypeInfo
    {
        public abstract int Priority { get; }
        public abstract Numeric BaseType { get; }

        /// <summary>
        /// Determines which general type to convert
        /// </summary>
        /// <param name="argTypes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Numeric ResolveNumericCast(Numeric[] argTypes)
        {
            if (argTypes.Length == 0) throw new ArgumentException("Empty sequence");
            return argTypes.Select(n => n.BaseType).MaxBy(n => n.Priority);
        }

        /// <summary>
        /// Determines which general type to convert
        /// </summary>
        /// <param name="arg0Type"></param>
        /// <param name="arg1Type"></param>
        /// <returns></returns>
        public static Numeric ResolveNumericCast(Numeric arg0Type, Numeric arg1Type)
        {
            return ResolveNumericCast(new Numeric[] { arg0Type, arg1Type });
        }
    }
}
