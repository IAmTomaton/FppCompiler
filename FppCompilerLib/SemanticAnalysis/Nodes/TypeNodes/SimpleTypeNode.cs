using FppCompilerLib.CodeGeneration;
using FppCompilerLib.SemanticAnalysis.TypeManagement;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib.SemanticAnalysis.Nodes.TypeNodes
{
    internal class SimpleTypeNode : TypeNode
    {
        private readonly string name;

        private TypeInfo? typeInfo;

        public SimpleTypeNode(string name, TypeInfo? typeInfo = null)
        {
            this.name = name;
            this.typeInfo = typeInfo;
        }

        public static new SimpleTypeNode Parse(NonTerminalNode node, RuleToNodeParseTable parceTable)
        {
            return new SimpleTypeNode(((TerminalNode)node.childs[0]).RealValue);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not SimpleTypeNode other) return false;
            return name == other.name;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override TypeInfo GetTypeInfo()
        {
            if (typeInfo is null) throw new InvalidOperationException($"Call UpdateTypes before calling GetTypeInfo");
            return typeInfo;
        }

        public override SimpleTypeNode UpdateTypes(Context context)
        {
            var typeInfo = context.typeManager.ResolveType(name);
            return new SimpleTypeNode(name, typeInfo);
        }

        public override TypeNode UpdateContext(Context context)
        {
            throw new InvalidOperationException();
        }

        public override AssemblerCommand[] ToCode() => Array.Empty<AssemblerCommand>();
    }
}
