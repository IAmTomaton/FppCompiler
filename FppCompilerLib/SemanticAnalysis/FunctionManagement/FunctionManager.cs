using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.MemoryManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement.Types;

namespace FppCompilerLib.SemanticAnalysis.FunctionManagement
{
    internal class FunctionManager
    {
        private readonly Dictionary<string, List<Function>> functions;

        public FunctionManager()
        {
            functions = new Dictionary<string, List<Function>>();
            Init();
        }

        public Function ResolveFunction(string name, TypeInfo[] argTypes)
        {
            return functions[name].First(func => func.CheckTypes(argTypes));
        }

        public void AddFunction(string name, Function function)
        {
            if (!functions.ContainsKey(name))
                functions.Add(name, new List<Function>());
            functions[name].Add(function);
        }

        private void Init()
        {
            AddFunction("LoadTile", new Function(new TypeInfo[] { new Int(), new Int() }, LoadTile));
            AddFunction("Swap", new Function(new TypeInfo[0], Swap));
            AddFunction("Clear", new Function(new TypeInfo[0], Clear));
            AddFunction("LoadAudio", new Function(new TypeInfo[] { new Int() }, LoadAudioPackage));
            AddFunction("AudioTick", new Function(new TypeInfo[0], AudioTick));
            AddFunction("Wait", new Function(new TypeInfo[0], Wait));
        }

        public FunctionManager GetChild()
        {
            return this;
        }

        private static AssemblerCommand[] Swap(Data[] args)
        {
            return new AssemblerCommand[] { new AdditionalCommand((6, 3)) };
        }

        private static AssemblerCommand[] Clear(Data[] args)
        {
            return new AssemblerCommand[] { new AdditionalCommand((6, 2)) };
        }

        private static AssemblerCommand[] LoadTile(Data[] args)
        {
            var commands = Enumerable.Empty<AssemblerCommand>();

            int arg0_addr;
            if (args[0] is Constant constArg0)
            {
                arg0_addr = 1;
                commands = commands.Concat(MemoryManager.MoveOrPut(constArg0, 1));
            }
            else if (args[0] is Variable varArg0)
                arg0_addr = varArg0.address;
            else
                throw new ArgumentException();

            commands = commands.Append(new AdditionalCommand((4, 1), arg0_addr, 2));

            int arg1_addr;
            if (args[1] is Constant constArg1)
            {
                arg1_addr = 1;
                commands = commands.Concat(MemoryManager.MoveOrPut(constArg1, 1));
            }
            else if (args[1] is Variable varArg1)
                arg1_addr = varArg1.address;
            else
                throw new ArgumentException();

            commands = commands.Append(new AdditionalCommand((6, 4), 2, arg1_addr));

            return commands.ToArray();
        }

        private static AssemblerCommand[] LoadAudioPackage(Data[] args)
        {
            var commands = Enumerable.Empty<AssemblerCommand>();

            int arg0_addr;
            if (args[0] is Constant constArg0)
            {
                arg0_addr = 1;
                commands = commands.Concat(MemoryManager.MoveOrPut(constArg0, 1));
            }
            else if (args[0] is Variable varArg0)
                arg0_addr = varArg0.address;
            else
                throw new ArgumentException();

            commands = commands.Append(new AdditionalCommand((4, 1), arg0_addr, 2));

            commands = commands.Append(new AdditionalCommand((5, 1), 2));

            return commands.ToArray();
        }

        private static AssemblerCommand[] AudioTick(Data[] args)
        {
            return new AssemblerCommand[] { new AdditionalCommand((5, 2)) };
        }

        private static AssemblerCommand[] Wait(Data[] args)
        {
            return new AssemblerCommand[] { new AdditionalCommand((7, 1)) };
        }
    }
}
