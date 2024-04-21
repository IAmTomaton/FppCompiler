using FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes;
using FppCompilerLib.SemanticAnalysis.TypeManagement;
using FppCompilerLib.SemanticAnalysis.TypeManagement.Types;

namespace FppCompilerLib.SemanticAnalysis.Nodes.TypeNodes
{
    internal class InitedArrayTypeNode : InitedTypeNode
    {
        private readonly InitedTypeNode child;
        private readonly InitedResultableNode sizeExp;

        public InitedArrayTypeNode(InitedResultableNode sizeExp, InitedTypeNode child)
        {
            this.child = child;
            this.sizeExp = sizeExp;
        }

        public override TypedArrayTypeNode UpdateTypes(Context context)
        {
            var typedSizeExp = sizeExp.UpdateTypes(context);
            if (!typedSizeExp.IsConstantResult)
                throw new ArgumentException("array size must be constant");
            if (typedSizeExp.ResultType is not Int)
                throw new ArgumentException("array size must be int");

            var typedChild = child.UpdateTypes(context);
            return new TypedArrayTypeNode(typedSizeExp.GetConstantResult.machineValues[0], typedChild);
        }
    }

    internal class TypedArrayTypeNode : TypedTypeNode
    {
        public override TypeInfo TypeInfo => new ArrayFpp(child.TypeInfo, size);

        private readonly TypedTypeNode child;
        private readonly int size;

        public TypedArrayTypeNode(int size, TypedTypeNode child)
        {
            this.size = size;
            this.child = child;
        }
    }
}
