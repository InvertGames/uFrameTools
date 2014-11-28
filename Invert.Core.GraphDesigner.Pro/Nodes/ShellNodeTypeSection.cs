using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

public class ShellNodeTypeSection : GenericNode, IShellReferenceType
{
    [JsonProperty]
    public bool AllowAdding { get; set; }


    public string ClassName
    {
        get { return this.Name + "Reference"; }
    }
    [JsonProperty, NodeProperty]
    public SectionVisibility Visibility { get; set; }



    [NodeProperty]
    public bool IsCustom
    {
        get
        {
            return this["Custom"];
        }
        set { this["Custom"] = value; }
    }


    public virtual string ReferenceClassName
    {
        get { return "I" + this.Name; }
    }
}

public class ShellSectionNode : ShellNodeTypeSection
{
    [InputSlot("Reference Type")]
    public SingleInputSlot<ShellChildItemTypeNode> ReferenceSlot { get; set; }

    public IShellReferenceType ReferenceType
    {
        get
        {
            if (ReferenceSlot == null) return null;
            return ReferenceSlot.Item;
        }
    }
    public override string ReferenceClassName
    {
        get
        {
            if (ReferenceType == null) return null;
            return ReferenceType.ClassName;
        }
    }
}