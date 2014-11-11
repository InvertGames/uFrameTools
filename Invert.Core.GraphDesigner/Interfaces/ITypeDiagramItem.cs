using System.CodeDom;
using System.Collections.Generic;
using Invert.uFrame.Editor;

public interface ITypedItem : IDiagramNodeItem
{
    string RelatedType { get; set; }
    string RelatedTypeName { get; }
    CodeTypeReference GetFieldType();
    CodeTypeReference GetPropertyType();
}

public interface IBindableTypedItem : IRefactorable, ITypedItem
{
    
   
    
    bool AllowEmptyRelatedType { get;  }
    string FieldName { get; }
    string NameAsChangedMethod { get;  }
    string ViewFieldName { get; }

    void SetType(IDesignerType input);
    void RemoveType();
}