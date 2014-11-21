public interface IShellReferenceType : IDiagramNodeItem, IConnectable {
    string ClassName { get;  }
    IShellReferenceType ReferenceType { get; }
    bool IsCustom { get; }

}