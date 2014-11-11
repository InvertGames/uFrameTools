using System.CodeDom;

namespace Invert.uFrame.Editor
{
    public abstract class MemberGenerator : IMemberGenerator
    {
        public CodeTypeDeclaration Decleration { get; set; }
        public MemberGeneratorLocation Location { get; set; }
        public object DataObject { get; set; }
        public abstract CodeTypeMember Create(bool isDesignerFile);
    }
}