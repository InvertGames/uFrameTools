using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor;

public class ClassNodeData : DiagramNode, IDesignerType
{
    private List<IDiagramNodeItem> _nodeItems;
 

    public List<IDiagramNodeItem> NodeItems
    {
        get { return _nodeItems ?? (_nodeItems= new List<IDiagramNodeItem>()); }
        set { _nodeItems = value; }
    }

    public override IEnumerable<IDiagramNodeItem> ContainedItems
    {
        get
        {
            return NodeItems;
        }
        set
        {
            NodeItems = value.ToList();
        }
    }

    public override void RemoveFromDiagram()
    {
        base.RemoveFromDiagram();
        Project.RemoveNode(this);
    }

    public override string Label
    {
        get
        {
            return Name;
        }
    }

    public IEnumerable<ClassPropertyData> Properties
    {
        get
        {
            return NodeItems.OfType<ClassPropertyData>();
        }
    }
    public IEnumerable<ClassCollectionData> Collections
    {
        get
        {
            return NodeItems.OfType<ClassCollectionData>();
        }
    }
    public string BaseIdentifier { get; set; }

    public ClassNodeData BaseClass
    {
        get
        {
            return Project.NodeItems.OfType<ClassNodeData>().FirstOrDefault(p => p.Identifier == BaseIdentifier);
        }
    }

    public IEnumerable<ClassNodeData> DerivedElements
    {
        get
        {
            var derived = Project.NodeItems.OfType<ClassNodeData>().Where(p => p.BaseIdentifier == Identifier);
            foreach (var derivedItem in derived)
            {
                yield return derivedItem;
                foreach (var another in derivedItem.DerivedElements)
                {
                    yield return another;
                }
            }
        }
    }

    public void SetBaseClass(ClassNodeData output)
    {
        BaseIdentifier = output.Identifier;
    }

    public void RemoveBaseClass()
    {
        BaseIdentifier = null;
    }

    public override void NodeItemRemoved(IDiagramNodeItem diagramNodeItem)
    {
        base.NodeItemRemoved(diagramNodeItem);
        NodeItems.Remove(diagramNodeItem);
    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        if (BaseIdentifier != null)
            cls.Add("BaseIdentifier", new JSONData(BaseIdentifier));
    }

    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        if (cls["BaseIdentifier"] != null)
            BaseIdentifier = cls["BaseIdentifier"].Value;
    }
}