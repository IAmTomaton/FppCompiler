namespace FppCompilerLib.CodeGeneration
{
    internal class AdditionalCommand : AssemblerCommand
    {
        public readonly (int, int) id;

        public AdditionalCommand((int, int) id) : base("@")
        {
            this.id = id;
        }

        public AdditionalCommand((int, int) id, int arg0) : base("@", arg0)
        {
            this.id = id;
        }

        public AdditionalCommand((int, int) id, int arg0, int arg1) : base("@", arg0, arg1)
        {
            this.id = id;
        }

        public AdditionalCommand((int, int) id, int arg0, int arg1, int arg2) : base("@", arg0, arg1, arg2)
        {
            this.id = id;
        }

        public override MachineCommand ToMachineCommand()
        {
            return new MachineCommand(id, args.Select(arg => int.Parse(arg)).ToArray());
        }

        public override AdditionalCommand ReplaceStrToInt(Dictionary<string, int> dict)
        {
            return this;
        }

        public override string ToString() => $"{cmd} {id} {string.Join(" ", args)}";
    }
}
