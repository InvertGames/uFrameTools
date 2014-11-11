using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Invert.Core;
using Invert.uFrame.Editor;

public class GenericNode : DiagramNode, IConnectable
{
    private List<string> _connectedGraphItemIds = new List<string>();
    private List<IDiagramNodeItem> _childItems = new List<IDiagramNodeItem>();

    public override string Label
    {
        get { return Name; }
    }
    public IEnumerable<IGraphItem> InputGraphItems
    {
        get
        {
            foreach (var item in Project.NodeItems.OfType<GenericNode>())
            {
                if (item.ConnectedGraphItems.Contains(this))
                {
                    yield return item;
                }
                foreach (var containedItem in item.ContainedItems.OfType<GenericNodeChildItem>())
                {
                    if (containedItem.ConnectedGraphItems.Contains(this))
                    {
                        yield return containedItem;
                    }
                }
            }
        }
    }
    public IEnumerable<IGraphItem> ConnectedGraphItems
    {
        get
        {
            foreach (var item in Project.NodeItems)
            {
                if (ConnectedGraphItemIds.Contains(item.Identifier))
                    yield return item;

                foreach (var child in item.ContainedItems)
                {
                    if (ConnectedGraphItemIds.Contains(child.Identifier))
                    {
                        yield return child;
                    }
                }
            }
        }
    }

    public List<string> ConnectedGraphItemIds
    {
        get { return _connectedGraphItemIds; }
        set { _connectedGraphItemIds = value; }
    }

    public List<IDiagramNodeItem> ChildItems
    {
        get { return _childItems; }
        set { _childItems = value; }
    }

    public NodeConfig Config
    {
        get
        {
            return InvertApplication.Container.Resolve<NodeConfig>(this.GetType().Name);
        }
    }

    public override IEnumerable<IDiagramNodeItem> Items
    {
        get
        {

            return ChildItems;
        }
    }

    public void UpdateReferences()
    {
        foreach (var mirrorSection in Config.Sections.Where(p => p.ReferenceType != null && !p.AllowAdding))
        {
            NodeConfigSection section = mirrorSection;
            var mirrorItems = ChildItems.Where(p => p.GetType() == section.ReferenceType).Cast<GenericReferenceItem>().ToArray();
            var newItems = mirrorSection.GenericSelector(this).ToArray();
            foreach (var item in newItems)
            {
                AddReferenceItem(mirrorItems, item, mirrorSection);
            }
            ChildItems.RemoveAll(p => p.GetType() == section.ChildType && !newItems.Contains(p));
        }


    }

    public void AddReferenceItem(IGraphItem item, NodeConfigSection mirrorSection)
    {
        AddReferenceItem(ChildItems.Where(p => p.GetType() == mirrorSection.ReferenceType).Cast<GenericReferenceItem>().ToArray(), item, mirrorSection);
    }

    internal void AddReferenceItem(GenericReferenceItem[] mirrorItems, IGraphItem item, NodeConfigSection mirrorSection)
    {
        var current = mirrorItems.FirstOrDefault(p => p.SourceIdentifier == item.Identifier);
        if (current != null && !mirrorSection.AllowDuplicates) return;

        var newMirror = Activator.CreateInstance(mirrorSection.ChildType) as GenericReferenceItem;
        newMirror.SourceIdentifier = item.Identifier;
        newMirror.Node = this;
        ChildItems.Add(newMirror);
    }

    public override IEnumerable<IDiagramNodeItem> ContainedItems
    {
        get { return ChildItems; }
        set { ChildItems = value.ToList(); }
    }

    public override void NodeRemoved(IDiagramNode nodeData)
    {
        base.NodeRemoved(nodeData);
        ConnectedGraphItemIds.Remove(nodeData.Identifier);
    }

    public override void NodeAddedInFilter(IDiagramNode newNodeData)
    {
        base.NodeAddedInFilter(newNodeData);


    }

    public override void NodeItemAdded(IDiagramNodeItem data)
    {
        base.NodeItemAdded(data);
        UpdateReferences();
    }

    public override void NodeItemRemoved(IDiagramNodeItem diagramNodeItem)
    {
        base.NodeItemRemoved(diagramNodeItem);
        //UpdateReferences();
        ChildItems.RemoveAll(
            p =>
                p.Identifier == diagramNodeItem.Identifier ||
                (p is GenericReferenceItem && ((GenericReferenceItem)p).SourceIdentifier == diagramNodeItem.Identifier));

    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        cls.AddPrimitiveArray("ConnectedGraphItems", ConnectedGraphItemIds, i => new JSONData(i));
    }

    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        if (cls["ConnectedGraphItems"] != null)
        {
            ConnectedGraphItemIds = cls["ConnectedGraphItems"].DeserializePrimitiveArray(n => n.Value).ToList();
        }
    }

    public IEnumerable<TChildItem> GetInputChildItems<TSourceNode, TChildItem>()
        where TSourceNode : GenericNode
    {
        return InputGraphItems.OfType<TSourceNode>().SelectMany(p => p.ContainedItems.OfType<TChildItem>());
    }

    public IEnumerable<TChildItem> GetInputInheritedChildItems<TSourceNode, TChildItem>()
      where TSourceNode : GenericInheritableNode
    {
        return InputGraphItems.OfType<TSourceNode>().SelectMany(p => p.ChildItemsWithInherited.OfType<TChildItem>());
    }
}

public class GenericReferenceItem<TSourceType> : GenericReferenceItem
{
    public TSourceType SourceItem
    {
        get { return (TSourceType)SourceItemObject; }
    }
}
public class GenericReferenceItem : GenericNodeChildItem
{
    private string _sourceIdentifier;

    public string SourceIdentifier
    {
        get { return _sourceIdentifier; }
        set { _sourceIdentifier = value; }
    }

    public IDiagramNodeItem SourceItemObject
    {
        get
        {
            return Node.Project.NodeItems.SelectMany(p => p.ContainedItems)
                .FirstOrDefault(p => p.Identifier == SourceIdentifier);
        }
    }

    public override string Name
    {
        get
        {

            return SourceItemObject.Name;
        }
    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        if (!string.IsNullOrEmpty(SourceIdentifier))
            cls.Add("SourceIdentifier", SourceIdentifier);
    }


    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);

        SourceIdentifier = cls["SourceIdentifier"].Value;

    }
}