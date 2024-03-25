using FppCompilerLib.SemanticAnalysis.Nodes;

namespace FppCompilerLib.CodeGeneration
{
    internal class CodeGenerator
    {
        public CodeGenerator()
        {

        }

        public AssemblerCommand[] ToAssemblerCommands(SemanticNode rootNode)
        {
            return rootNode.ToCode();
        }

        public MachineCommand[] ToMachineCommands(AssemblerCommand[] program)
        {
            var labels = new Dictionary<string, int>();

            var cmdIndex = 0;

            foreach (var cmd in program)
            {
                if (cmd.cmd == "label")
                    labels.Add(cmd.args[0], cmdIndex);
                else
                    cmdIndex++;
            }

            program = program.Where(cmd => cmd.cmd != "label")
                .Select(cmd => cmd.ReplaceStrToInt(labels))
                .Append(AssemblerCommand.Empty())
                .ToArray();

            var machineCommands = program.Select(cmd => cmd.ToMachineCommand()).ToArray();
            return machineCommands;
        }

        public MachineCommand[] ToMachineCommands(SemanticNode rootNode)
        {
            return ToMachineCommands(ToAssemblerCommands(rootNode));
        }
    }
}
