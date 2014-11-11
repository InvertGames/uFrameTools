using System.CodeDom;

namespace Invert.uFrame.Code.Bindings
{
    public interface IBindingGenerator
    {
        string Title { get; }
        string Description { get; }
        string MethodName { get; }
        IBindableTypedItem Item { get; set; }
        bool IsApplicable { get; }
        bool IsOverride { get; set; }
        bool CallBase { get; set; }
        string BindingConditionFieldName { get; }
        bool GenerateDefaultImplementation { get; set; }
        ElementData Element { get; set; }
        bool IsBase { get; set; }
        void CreateMembers(CodeTypeMemberCollection collection);
        void CreateBindingStatement(CodeTypeMemberCollection collection, CodeConditionStatement bindingCondition);
    }

}