using System.Linq;
using Invert.Core.GraphDesigner;
using Invert.Json;
using Invert.uFrame.Code.Bindings;
using Invert.uFrame.Editor;

public class ViewBindingData : DiagramNodeItem
{
    public override string FullLabel
    {
        get { return Name; }
    }

    public override void Remove(IDiagramNode diagramNode)
    {
        var viewData = Node as ViewData;
        if (viewData != null)
        {
            viewData.Bindings.Remove(this);
        }
    }

    public override string Label
    {
        get { return Name; }
    }
    public string GeneratorType { get; set; }

    public IBindingGenerator Generator { get; set; }
    public string PropertyIdentifier { get; set; }

    public ViewData View
    {
        get { return Node as ViewData; }
    }

    public IBindableTypedItem Property
    {
        get
        {
            if (string.IsNullOrEmpty(PropertyIdentifier)) return null;
            return
                Node.Project.NodeItems.OfType<ElementData>()
                    .SelectMany(p => p.ViewModelItems)
                    .FirstOrDefault(p => p.Identifier == PropertyIdentifier);
        }
    }

    public override void NodeRemoved(IDiagramNode nodeData)
    {
        base.NodeRemoved(nodeData);
        if (nodeData.Identifier == PropertyIdentifier)
        {
            PropertyIdentifier = string.Empty;
        }
    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        if (GeneratorType != null)
            cls.Add("GeneratorType", new JSONData(GeneratorType));

        cls.Add("PropertyIdentifier", new JSONData(PropertyIdentifier ?? string.Empty));
    }

    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        if (cls["GeneratorType"] != null)
        {
            GeneratorType = cls["GeneratorType"].Value;
        }
        if (cls["PropertyIdentifier"] != null)
        {
            PropertyIdentifier = cls["PropertyIdentifier"].Value;
        }
        else
        {
            
        }
    }
}