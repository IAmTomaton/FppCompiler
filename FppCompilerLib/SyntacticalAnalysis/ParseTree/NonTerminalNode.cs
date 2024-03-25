namespace FppCompilerLib.SyntacticalAnalysis
{
    internal class NonTerminalNode : ParseNode
    {
        public readonly ParseNode[] childs;
        public readonly Rule rule;

        private readonly NonTerminal nonTerminal;

        public NonTerminalNode(NonTerminal nonTerminal, ParseNode[] childs, Rule rule)
        {
            this.nonTerminal = nonTerminal;
            this.childs = childs;
            this.rule = rule;
        }
    }
}
