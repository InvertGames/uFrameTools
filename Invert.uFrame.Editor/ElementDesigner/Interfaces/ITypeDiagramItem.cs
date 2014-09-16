using System.CodeDom;
using System.Collections.Generic;
using Invert.uFrame.Editor;

public interface ITypeDiagramItem : IDiagramNodeItem, IRefactorable,IItem
{
    
    string RelatedType { get; set; }
    string RelatedTypeName { get; }
    bool AllowEmptyRelatedType { get;  }
    string FieldName { get; }
    string NameAsChangedMethod { get;  }
    string ViewFieldName { get; }

    void SetType(IDesignerType input);
    void RemoveType();
    CodeTypeReference GetFieldType();
    CodeTypeReference GetPropertyType();
}