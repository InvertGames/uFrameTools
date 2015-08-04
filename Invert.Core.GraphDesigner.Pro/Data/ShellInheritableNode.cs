using System.Linq;
using Invert.Core.GraphDesigner;
using Invert.Json;

public class ShellInheritableNode : GenericInheritableNode, IShellNode
{

    [InspectorProperty]
    public bool IsCustom
    {
        get { return this["Custom"]; }
        set { this["Custom"] = value; }
    }
    [JsonProperty]
    public string BaseIdentifier { get; set; }

    [InspectorProperty(InspectorType.GraphItems)]
    public override GenericInheritableNode BaseNode
    {
        get
        {
            return Repository.GetById<GenericInheritableNode>(BaseIdentifier);
            //return this.Project.NodeItems.FirstOrDefault(p => p.Identifier == BaseIdentifier) as GenericInheritableNode;
        }
        set
        {
            if (value != null)
            BaseIdentifier = value.Identifier;
            else
            {
                BaseIdentifier = null;
            }
        }
    }

    public virtual string ClassName
    {
        get { return string.Format("{0}", Name); }
    }
    

}