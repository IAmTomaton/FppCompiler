namespace FppCompilerLib.SyntacticalAnalysis
{
    internal class Terminal : Token
    {
        public Terminal(string value) : base(value) { }

        public static Terminal End => new("end");
        public static Terminal Type => new("type");
        public static Terminal Word => new("word");
        public static Terminal Const => new("const");
        public static Terminal BinaryOperator => new("binaryOperator");
        public static Terminal UnaryOperator => new("unaryOperator");
    }
}
