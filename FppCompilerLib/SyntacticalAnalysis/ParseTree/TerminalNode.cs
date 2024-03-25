namespace FppCompilerLib.SyntacticalAnalysis
{
    internal class TerminalNode : ParseNode
    {
        public string RealValue => terminal.realValue;

        private readonly TerminalWithValue terminal;

        public TerminalNode(TerminalWithValue terminal)
        {
            this.terminal = terminal;
        }
    }
}
