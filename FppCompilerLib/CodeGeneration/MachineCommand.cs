namespace FppCompilerLib.CodeGeneration
{
    internal class MachineCommand
    {
        public readonly (int unitId, int cmdId) id;
        public readonly int[] args;

        public MachineCommand((int unitId, int cmdId) id, int[] args)
        {
            this.id = id;
            this.args = args;
        }

        public override string ToString() => $"{id} {string.Join(" ", args)}";
    }
}
