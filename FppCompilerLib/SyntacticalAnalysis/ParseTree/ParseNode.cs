namespace FppCompilerLib.SyntacticalAnalysis
{
    internal abstract class ParseNode
    {
        public NonTerminalNode AsNonTerminalNode => (NonTerminalNode)this;
        public TerminalNode AsTerminalNode => (TerminalNode)this;
    }
}
