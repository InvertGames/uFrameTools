using System.Collections.Generic;
using Invert.Core.GraphDesigner;

public interface IShellReferenceType : IDiagramNodeItem, IConnectable, IClassTypeNode {
    
    //IShellReferenceType ReferenceType { get; }
    bool IsCustom { get; }
    

}