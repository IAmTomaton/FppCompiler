namespace FppCompilerLib.SyntacticalAnalysis
{
    internal class TerminalWithValue : Terminal
    {
        public readonly string realValue;

        public TerminalWithValue(string value, string real_value) : base(value)
        {
            this.realValue = real_value;
        }

        public static new TerminalWithValue Type(string real_value) => new("type", real_value);
        public static new TerminalWithValue Word(string real_value) => new("word", real_value);
        public static new TerminalWithValue Const(string real_value) => new("const", real_value);
        public static new TerminalWithValue BinaryOperator(string real_value) => new("binaryOperator", real_value);
        public static new TerminalWithValue UnaryOperator(string real_value) => new("unaryOperator", real_value);

        public override string ToString()
        {
            return $"{value} {realValue}";
        }
    }
}
