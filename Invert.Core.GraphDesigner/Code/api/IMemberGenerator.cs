using System.CodeDom;

namespace Invert.Core.GraphDesigner
{
    public interface IMemberGenerator
    {
        MemberGeneratorLocation MemberLocation { get; set; }
        
        CodeTypeMember Create(CodeTypeDeclaration decleration, object data, bool isDesignerFile);
    }
    public interface IMemberGenerator<in TData> : IMemberGenerator
    {


        CodeTypeMember Create(TData data, bool isDesignerFile);

    }
}