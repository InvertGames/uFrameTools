using System.CodeDom;

namespace Invert.uFrame.Editor
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