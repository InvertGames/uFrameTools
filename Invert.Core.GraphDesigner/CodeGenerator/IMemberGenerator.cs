using System.CodeDom;

namespace Invert.uFrame.Editor
{
    public interface IMemberGenerator
    {
        CodeTypeDeclaration Decleration { get; set; }
        MemberGeneratorLocation Location { get; set; }
        object DataObject { get; set; }
        CodeTypeMember Create(bool isDesignerFile);
    }
}