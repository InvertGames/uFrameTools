using System.Collections.Generic;

namespace Invert.Core.GraphDesigner
{

    public interface IClassTypeNode : IDiagramNodeItem
    {
        string ClassName { get; }
    }

    public interface IClassRefactorable : IDiagramNodeItem
    {
        IEnumerable<string> ClassNameFormats { get; }
    }

    public interface IIdentifierRefactorable : IDiagramNodeItem
    {
        IEnumerable<string> IdentifierFormats { get;  }
    }

    public interface IMethodRefactorable : IDiagramNodeItem
    {
        IEnumerable<string> MethodFormats { get; }
    }

    public interface IMethodParameterRefactorable : ITypedItem
    {
        
    }

    public interface IPropertyRefactorable : ITypedItem
    {
        IEnumerable<string> PropertyFormats { get; }
    }
}