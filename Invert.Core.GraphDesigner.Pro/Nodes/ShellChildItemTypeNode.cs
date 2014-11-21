public class ShellChildItemTypeNode : GenericInheritableNode, IShellReferenceType {
    public IShellReferenceType ReferenceType
    {
        get { return GetConnectionReference<ReferenceItemType>().InputFrom<IShellReferenceType>(); }
    }

    public bool IsCustom { get { return this["Custom"]; } }

    public string ClassName
    {
        get { return this.Name + "ChildItem"; }
    }
}