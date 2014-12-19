using Invert.Core.GraphDesigner;

public class ShellInheritableNode : GenericInheritableNode, IShellNode
{

    [InspectorProperty]
    public bool IsCustom
    {
        get { return this["Custom"]; }
        set { this["Custom"] = value; }
    }


    public virtual string ClassName
    {
        get { return string.Format("{0}", Name); }
    }
}