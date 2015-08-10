using System.Collections.Generic;

namespace Invert.Core.GraphDesigner
{

    public interface IClassTypeNode : IDiagramNodeItem
    {
        string ClassName { get; }
    }

    public interface IClassRefactorable 
    {
        IEnumerable<string> ClassNameFormats { get; }
    }

    public interface IIdentifierRefactorable 
    {
        IEnumerable<string> IdentifierFormats { get;  }
    }

    public interface IMethodRefactorable 
    {
        IEnumerable<string> MethodFormats { get; }
    }

    public interface IMethodParameterRefactorable 
    {
        
    }

    public interface IPropertyRefactorable : ITypedItem
    {
        IEnumerable<string> PropertyFormats { get; }
    }
}