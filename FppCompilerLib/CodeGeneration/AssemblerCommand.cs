namespace FppCompilerLib.CodeGeneration
{
    internal class AssemblerCommand
    {
        public readonly string cmd;
        public readonly string[] args;

        public AssemblerCommand(string cmd)
        {
            this.cmd = cmd;
            args = Array.Empty<string>();
        }

        public AssemblerCommand(string cmd, string[] args)
        {
            this.cmd = cmd;
            this.args = args;
        }

        public AssemblerCommand(string cmd, int[] args)
        {
            this.cmd = cmd;
            this.args = args.Select(arg => arg.ToString()).ToArray();
        }

        public AssemblerCommand(string cmd, string arg)
        {
            this.cmd = cmd;
            args = new string[] { arg };
        }

        public AssemblerCommand(string cmd, int arg0, string arg1)
        {
            this.cmd = cmd;
            args = new string[] { arg0.ToString(), arg1 };
        }

        public AssemblerCommand(string cmd, int arg)
        {
            this.cmd = cmd;
            args = new string[] { arg.ToString() };
        }

        public AssemblerCommand(string cmd, int arg0, int arg1)
        {
            this.cmd = cmd;
            args = new string[] { arg0.ToString(), arg1.ToString() };
        }

        public AssemblerCommand(string cmd, int arg0, int arg1, int arg2)
        {
            this.cmd = cmd;
            args = new string[] { arg0.ToString(), arg1.ToString(), arg2.ToString() };
        }

        public virtual MachineCommand ToMachineCommand()
        {
            if (!cmdToId.ContainsKey(cmd))
                throw new InvalidOperationException($"Cannot convert an assembly instruction {cmd} into a machine instruction.");
            return new MachineCommand(cmdToId[cmd], args.Select(arg => int.Parse(arg)).ToArray());
        }

        public static AssemblerCommand Move(int addr0, int addr1) => new("mov", addr0, addr1);
        public static AssemblerCommand[] Move(int addr0, int addr1, int length)
        {
            return Enumerable.Range(0, length).Select(i => Move(addr0 + i, addr1 + i)).ToArray();
        }
        public static AssemblerCommand Put(int value, int addr) => new("put", value, addr);
        public static AssemblerCommand[] Put(int[] values, int addr1)
        {
            return Enumerable.Range(0, values.Length).Select(i => Put(values[i], addr1 + i)).ToArray();
        }

        public static AssemblerCommand Jmp(int index) => new("jmp", 0, index);
        public static AssemblerCommand Jmp(string label) => new("jmp", 0, label);
        public static AssemblerCommand JmpIfNotEq0(int addr, int index) => new("jmpne0", addr, index);
        public static AssemblerCommand JmpIfNotEq0(int addr, string label) => new("jmpne0", addr, label);
        public static AssemblerCommand JmpIfEq0(int addr, int index) => new("jmpe0", addr, index);
        public static AssemblerCommand JmpIfEq0(int addr, string label) => new("jmpe0", addr, label);

        public static AssemblerCommand Label(string label) => new("label", label);
        public static AssemblerCommand Empty() => new("empty", Array.Empty<string>());

        public virtual AssemblerCommand ReplaceStrToInt(Dictionary<string, int> dict)
        {
            return new AssemblerCommand(cmd, args.Select(arg => dict.ContainsKey(arg) ? dict[arg].ToString() : arg).ToArray());
        }

        public override string ToString() => $"{cmd} {string.Join(" ", args)}";
        public override int GetHashCode()
        {
            return cmd.GetHashCode() + args.Select(arg => arg.GetHashCode()).Aggregate((a, b) => a * 37 + b) * 37;
        }
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not AssemblerCommand other) return false;
            if (cmd != other.cmd || args.Length != other.args.Length) return false;
            return args.SequenceEqual(other.args);
        }

        private static readonly Dictionary<string, (int, int)> cmdToId = new()
        {
            { "empty", (0, 0) },  // empty command, stop programm

            { "put", (1, 1) },  // put
            { "mov", (1, 2) },  // move
            { "put2i", (1, 3) }, // put to pointer
            { "move2i", (1, 4) },  // move to pointer
            { "move4i", (1, 5) },  // move from pointer

            { "add", (2, 1) },  // +
            { "sub", (2, 2) },  // -
            { "mul", (2, 3) },  // *
            { "div", (2, 4) },  // /
            { "rem", (2, 5) },  // %
            { "pow", (2, 6) },  // **
            { "shl", (2, 7) },  // <<
            { "shr", (2, 8) },  // >>
            { "and", (2, 9) },  // & &&
            { "or", (2, 10) },  // | ||
            { "xor", (2, 11) },  // ^

            { "lth", (2, 12) },  // <
            { "gth", (2, 13) },  // >
            { "eq", (2, 14) },  // ==
            { "geth", (2, 15) },  // >=
            { "leth", (2, 16) },  // <=
            { "neq", (2, 17) },  // !=
            { "not", (2, 18) },  // !

            { "inc", (2, 19) },  // + 1
            { "dec", (2, 20) },  // - 1

            { "abs", (2, 21) },  // abs
            { "rnd", (2, 22) },  // round
            { "max", (2, 23) },  // max
            { "min", (2, 24) },  // min

            { "jmp", (3, 1) },  // jmp
            { "jmpne0", (3, 2) },  // jmp if != 0
            { "jmpe0", (3, 3) },  // jmp if == 0
        };
    }
}
