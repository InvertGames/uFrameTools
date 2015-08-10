using System.Linq;
using Invert.Core.GraphDesigner;
using Invert.Data;
using Invert.Json;

public class ShellInheritableNode : GenericInheritableNode, IShellNode
{
    private string _baseIdentifier;

    [JsonProperty]
    public string BaseIdentifier
    {
        get { return _baseIdentifier; }
        set {
            _baseIdentifier = value;
            this.Changed("BaseIdentifier", _baseIdentifier, value);
        }
    }

    [InspectorProperty(InspectorType.GraphItems)]
    public override GenericInheritableNode BaseNode
    {
        get
        {
            if (string.IsNullOrEmpty(BaseIdentifier) || Repository == null)
                return null;
            return Repository.GetById<GenericInheritableNode>(BaseIdentifier);
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