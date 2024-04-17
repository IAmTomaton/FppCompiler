using FppCompilerLib.CodeGeneration;

namespace FppCompilerLib.SemanticAnalysis.Nodes
{
    internal class InitedAssemblerInsertNode : InitedSemanticNode
    {
        private readonly AssemblerCommand[] commands;

        public InitedAssemblerInsertNode(AssemblerCommand[] commands)
        {
            this.commands = commands;
        }

        public override TypedAssemblerInsertNode UpdateTypes(Context context)
        {
            return new TypedAssemblerInsertNode(commands);
        }
    }

    internal class TypedAssemblerInsertNode : TypedSemanticNode
    {
        private readonly AssemblerCommand[] commands;

        public TypedAssemblerInsertNode(AssemblerCommand[] commands)
        {
            this.commands = commands;
        }

        public override UpdatedAssemblerInsertNode UpdateContext(Context context)
        {
            return new UpdatedAssemblerInsertNode(commands);
        }
    }

    internal class UpdatedAssemblerInsertNode : UpdatedSemanticNode
    {
        private readonly AssemblerCommand[] commands;

        public UpdatedAssemblerInsertNode(AssemblerCommand[] commands)
        {
            this.commands = commands;
        }

        public override AssemblerCommand[] ToCode()
        {
            return commands;
        }
    }
}
