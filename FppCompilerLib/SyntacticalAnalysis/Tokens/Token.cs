namespace FppCompilerLib.SyntacticalAnalysis
{
    internal abstract class Token
    {
        public readonly string value;

        public Token(string value)
        {
            this.value = value;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not Token other) return false;
            return other.value == value;
        }

        public static bool operator ==(Token a, Token b) => a.Equals(b);
        public static bool operator !=(Token a, Token b) => !a.Equals(b);

        public override string ToString()
        {
            return $"{value}";
        }
    }
}
