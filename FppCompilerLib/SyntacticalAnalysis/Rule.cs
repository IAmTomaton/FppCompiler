namespace FppCompilerLib.SyntacticalAnalysis
{
    internal class Rule
    {
        public readonly NonTerminal source;
        public readonly Token[] tokens;

        public Rule(NonTerminal source, Token[] tokens)
        {
            this.source = source;
            this.tokens = tokens;
        }

        public Rule(NonTerminal source, Token token) : this(source, new Token[] { token }) { }
        public Rule(NonTerminal source) : this(source, Array.Empty<Token>()) { }

        public override int GetHashCode()
        {
            return source.GetHashCode() + (tokens.Length > 0 ? tokens.Select(t => t.GetHashCode()).Aggregate((a, b) => a * 37 + b) : 0);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not Rule other) return false;
            if (tokens.Length != other.tokens.Length) return false;
            return source == other.source && tokens.SequenceEqual(other.tokens);
        }

        public override string ToString()
        {
            return $"{source} -> {string.Join(" ", tokens.Select(t => t.ToString()))}";
        }
    }
}
