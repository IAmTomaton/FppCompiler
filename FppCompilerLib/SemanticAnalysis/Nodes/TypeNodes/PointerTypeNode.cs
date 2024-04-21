using FppCompilerLib.SemanticAnalysis.TypeManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement.Types;

namespace FppCompilerLib.SemanticAnalysis.Nodes.TypeNodes
{
    internal class InitedPointerTypeNode : InitedTypeNode
    {
        private readonly InitedTypeNode child;

        public InitedPointerTypeNode(InitedTypeNode child)
        {
            this.child = child;
        }

        public override TypedPointerTypeNode UpdateTypes(Context context)
        {
            var typedChild = child.UpdateTypes(context);
            return new TypedPointerTypeNode(typedChild);
        }
    }

    internal class TypedPointerTypeNode : TypedTypeNode
    {
        public override TypeInfo TypeInfo => new Pointer(child.TypeInfo);

        private readonly TypedTypeNode child;

        public TypedPointerTypeNode(TypedTypeNode child)
        {
            this.child = child;
        }
    }
}
