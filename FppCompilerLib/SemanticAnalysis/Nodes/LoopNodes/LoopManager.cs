namespace FppCompilerLib.SemanticAnalysis.Nodes.LoopNodes
{
    internal class LoopManager
    {
        private readonly LoopManager? parent;

        private string? continueLabel;
        public string ContinueLabel
        {
            get
            {
                if (continueLabel == null && parent != null) return parent.ContinueLabel;
                if (continueLabel == null) throw new InvalidOperationException();
                return continueLabel;
            }
        }
        private string? breakLabel;
        public string BreakLabel
        {
            get
            {
                if (breakLabel == null && parent != null) return parent.BreakLabel;
                if (breakLabel == null) throw new InvalidOperationException();
                return breakLabel;
            }
        }

        public LoopManager() { }

        public LoopManager(LoopManager parent)
        {
            this.parent = parent;
        }

        public LoopManager GetChild()
        {
            return new LoopManager(this);
        }

        public void SetLoopLabels(string continueLabel, string breakLabel)
        {
            this.continueLabel = continueLabel;
            this.breakLabel = breakLabel;
        }
    }
}
