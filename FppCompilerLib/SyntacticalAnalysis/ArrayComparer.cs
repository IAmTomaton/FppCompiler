using System.Diagnostics.CodeAnalysis;

namespace FppCompilerLib.SyntacticalAnalysis
{
    internal class ArrayComparer<T> : IEqualityComparer<T[]>
    {
        public bool Equals(T[]? x, T[]? y)
        {
            if (x == null) return false;
            if (y == null) return false;
            if (x.Length != y.Length) return false;
            return x.SequenceEqual(y);
        }

        public int GetHashCode([DisallowNull] T[] array)
        {
            if (array.Length == 0) return 0;
            return array.Select(t => t.GetHashCode()).Aggregate((a, b) => a * 37 + b);
        }
    }
}
