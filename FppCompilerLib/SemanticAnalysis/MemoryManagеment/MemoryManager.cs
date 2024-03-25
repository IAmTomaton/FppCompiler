using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.TypeManagement;

namespace FppCompilerLib.SemanticAnalysis.MemoryManagement
{
    internal class MemoryManager
    {
        private readonly Dictionary<string, Variable> variables = new();
        private readonly Dictionary<string, Constant> constants = new();
        private readonly RAMManager ramManager;

        private readonly MemoryManager? parent;

        public MemoryManager(RAMManager ramManager)
        {
            this.ramManager = ramManager;
        }

        public MemoryManager(MemoryManager parent, RAMManager ramManager) : this(ramManager)
        {
            this.parent = parent;
        }

        public bool IsVariable(string name)
        {
            if (variables.ContainsKey(name)) return true;
            if (parent != null) return parent.IsVariable(name);
            return false;
        }

        public Variable AddVariable(string name, TypeInfo typeInfo)
        {
            if (IsVariable(name)) throw new ArgumentException($"Variable {name} already exist");
            var address = ramManager.Allocate(typeInfo.Size);
            var variable = new Variable(address, typeInfo);
            variables.Add(name, variable);
            return variable;
        }

        public void DelVariable(string name)
        {
            if (!variables.ContainsKey(name)) throw VariableNotExist(name);
            ramManager.FreeUp(variables[name].address);
            variables.Remove(name);
        }

        public Variable GetVariable(string name)
        {
            if (!IsVariable(name)) throw VariableNotExist(name);
            if (variables.ContainsKey(name)) return variables[name];
            return parent.GetVariable(name);
        }

        public bool IsConstant(string name)
        {
            if (constants.ContainsKey(name)) return true;
            if (parent != null) return parent.IsConstant(name);
            return false;
        }

        public void AddConstant(string name, Constant constant)
        {
            if (IsConstant(name)) throw new ArgumentException($"Constant \"{name}\" already exist");
            constants.Add(name, constant);
        }

        public void DelConstant(string name)
        {
            if (!constants.ContainsKey(name)) throw ConstantNotExist(name);
            constants.Remove(name);
        }

        public Constant GetConstant(string name)
        {
            if (!IsConstant(name)) throw ConstantNotExist(name);
            if (constants.ContainsKey(name)) return constants[name];
            return parent.GetConstant(name);
        }

        public Data GetData(string name)
        {
            if (!IsConstant(name) && !IsVariable(name)) throw ConstantNotExist(name);
            if (IsConstant(name))
                return GetConstant(name);
            return GetVariable(name);
        }

        public (string, Variable) CreateTempVariable(TypeInfo typeInfo)
        {
            var tempVarName = Guid.NewGuid().ToString();
            var address = ramManager.Allocate(typeInfo.Size);
            var variable = new Variable(address, typeInfo);
            variables.Add(tempVarName, variable);
            return (tempVarName, variable);
        }

        public static AssemblerCommand[] MoveOrPut(Data data, Variable target)
        {
            if (data.TypeInfo.Size != target.TypeInfo.Size)
                throw new ArgumentException("data sizes do not match");
            if (data is Constant constant)
                return AssemblerCommand.Put(constant.machineValues, target.address);
            if (data is Variable variable)
                return AssemblerCommand.Move(variable.address, target.address, variable.TypeInfo.Size);
            throw new ArgumentException();
        }

        public static AssemblerCommand[] MoveOrPut(Data data, int address)
        {
            if (data.TypeInfo.Size != 1)
                throw new ArgumentException("data sizes do not match");
            if (data is Constant constant)
                return AssemblerCommand.Put(constant.machineValues, address);
            if (data is Variable variable)
                return AssemblerCommand.Move(variable.address, address, variable.TypeInfo.Size);
            throw new ArgumentException();
        }

        public MemoryManager GetChild()
        {
            return new MemoryManager(this, ramManager.GetChild());
        }

        private static ArgumentException ConstantNotExist(string name) => new($"Constant \"{name}\" does not exist");
        private static ArgumentException VariableNotExist(string name) => new($"Variable \"{name}\" does not exist");
    }
}
