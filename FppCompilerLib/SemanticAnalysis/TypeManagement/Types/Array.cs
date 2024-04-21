using FppCompilerLib.SemanticAnalysis.MemoryManagement;

namespace FppCompilerLib.SemanticAnalysis.TypeManagement.Types
{
    internal class ArrayFpp : TypeInfo
    {
        public override string Name => "array";
        public override int Size => ChildType.Size * Length;
        public int Length => length;
        public TypeInfo ChildType => childType;

        private readonly int length;
        private readonly TypeInfo childType;

        public ArrayFpp(TypeInfo childType, int length)
        {
            this.length = length;
            this.childType = childType;
        }

        public override int GetHashCode()
        {
            var hash = ChildType.GetHashCode();
            hash = hash * 37 + Length;
            hash = hash * 37 + Name.GetHashCode();
            return hash;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not ArrayFpp other) return false;
            return Length == other.Length && ChildType.Equals(other.ChildType);
        }
    }
}
