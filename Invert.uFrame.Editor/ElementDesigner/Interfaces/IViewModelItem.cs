using System.Collections.Generic;
using Invert.uFrame.Editor;

public interface IViewModelItem : IDiagramNodeItem, IRefactorable,IItem
{
    
    string RelatedType { get; set; }
    string RelatedTypeName { get; }
    bool AllowEmptyRelatedType { get;  }
    IEnumerable<string> BindingMethodNames { get; }
    void SetType(IDesignerType input);
    void RemoveType();
}