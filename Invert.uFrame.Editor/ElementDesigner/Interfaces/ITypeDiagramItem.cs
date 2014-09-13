using System.Collections.Generic;
using Invert.uFrame.Editor;

public interface ITypeDiagramItem : IDiagramNodeItem, IRefactorable,IItem
{
    
    string RelatedType { get; set; }
    string RelatedTypeName { get; }
    bool AllowEmptyRelatedType { get;  }

    void SetType(IDesignerType input);
    void RemoveType();
}