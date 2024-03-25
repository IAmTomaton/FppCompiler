using FppCompilerLib.CodeGeneration;

namespace FppCompilerLib.SemanticAnalysis.Nodes
{
    internal class AssemblerInsertNode : SemanticNode
    {
        private readonly AssemblerCommand[] commands;

        public AssemblerInsertNode(AssemblerCommand[] commands)
        {
            this.commands = commands;
        }

        public override SemanticNode UpdateTypes(Context context)
        {
            return this;
        }

        public override SemanticNode UpdateContext(Context context)
        {
            return this;
        }

        public override AssemblerCommand[] ToCode()
        {
            return commands;
        }

        public override bool Equals(object? obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
