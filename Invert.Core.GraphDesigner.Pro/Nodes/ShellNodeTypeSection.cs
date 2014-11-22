using Invert.Core.GraphDesigner;

public class ShellNodeTypeSection : GenericInheritableNode, IShellReferenceType
{


    [JsonProperty]
    public bool AllowAdding { get; set; }


    public string ClassName
    {
        get { return this.Name + "Reference"; }
    }

    public IShellReferenceType ReferenceType
    {
        get { return GetConnectionReference<ReferenceItemType>().InputFrom<IShellReferenceType>(); }
    }

    public bool IsCustom { get { return this["Custom"]; } }
}