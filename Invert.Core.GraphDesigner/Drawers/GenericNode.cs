using Invert.Core;
using Invert.uFrame.Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class GenericConnectionReference : GenericNodeChildItem
{
    public string InputName { get; set; }

    public override string Name
    {
        get { return InputName; }
        set { base.Name = value; }
    }
}

public class GenericNode : DiagramNode, IConnectable
{
    private List<IDiagramNodeItem> _childItems = new List<IDiagramNodeItem>();
    private List<string> _connectedGraphItemIds = new List<string>();

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

    //public List<string> ConnectedGraphItemIds
    //{
    //    get { return _connectedGraphItemIds; }
    //    set { _connectedGraphItemIds = value; }
    //}

    //public IEnumerable<IGraphItem> ConnectedGraphItems
    //{
    //    get
    //    {
    //        foreach (var item in Project.NodeItems)
    //        {
    //            if (ConnectedGraphItemIds.Contains(item.Identifier))
    //                yield return item;

    //            foreach (var child in item.ContainedItems)
    //            {
    //                if (ConnectedGraphItemIds.Contains(child.Identifier))
    //                {
    //                    yield return child;
    //                }
    //            }
    //        }
    //    }
    //}

    public override IEnumerable<IDiagramNodeItem> ContainedItems
    {
        get { return ChildItems; }
        set
        {
            ChildItems = value.ToList();
        }
    }

    public IEnumerable<ConnectionData> Inputs
    {
        get
        {
           
            foreach (var connectionData in Project.Connections)
            {
                if (connectionData.InputIdentifier == this.Identifier)
                {
                    yield return connectionData;
                }
            }
        }
    }
    public IEnumerable<ConnectionData> Outputs
    {
        get
        {
            foreach (var connectionData in Project.Connections)
            {
                if (connectionData.InputIdentifier == this.Identifier)
                {
                    yield return connectionData;
                }
            }
        }
    }
    //public IEnumerable<IGraphItem> OutputGraphItems
    //{
    //    get
    //    {
    //        foreach (var connectionData in Project.Connections)
    //        {
    //            if (connectionData.InputIdentifier == this.Identifier)
    //            {
    //                var item = Project.AllGraphItems.FirstOrDefault(p => p.Identifier == connectionData.OutputIdentifier);
    //                if (item != null)
    //                    yield return item;
    //            }
    //        }
    //    }
    //}

    public override IEnumerable<IDiagramNodeItem> Items
    {
        get
        {
            return ChildItems;
        }
    }

    public override string Label
    {
        get { return Name; }
    }

    

    public void AddReferenceItem(IGraphItem item, NodeConfigSection mirrorSection)
    {
        AddReferenceItem(ChildItems.Where(p => p.GetType() == mirrorSection.ReferenceType).Cast<GenericReferenceItem>().ToArray(), item, mirrorSection);
    }

    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
     
    }

    //public TItem GetConnection<TConnectionType, TItem>() where TConnectionType : GenericConnectionReference, new()
    //{
    //    return (TItem)GetConnectionReference<TConnectionType>().ConnectedGraphItems.FirstOrDefault();
    //}

    public TType GetConnectionReference<TType>()
        where TType : GenericConnectionReference, new()
    {
        return (TType)GetConnectionReference(typeof(TType));
    }

    public GenericConnectionReference GetConnectionReference(Type inputType)
    {
        var item = ChildItems.FirstOrDefault(p => inputType.IsAssignableFrom(p.GetType()));
        if (item == null)
        {
            var input = Activator.CreateInstance(inputType) as GenericConnectionReference;
            input.Node = this;
            ChildItems.Add(input);
            return input;
        }

        return item as GenericConnectionReference;
    }

    //public IEnumerable<TItem> GetConnections<TConnectionType, TItem>() where TConnectionType : GenericConnectionReference, new()
    //{
    //    return GetConnectionReference<TConnectionType>().ConnectedGraphItems.Cast<TItem>();
    //}

    //public IEnumerable<TChildItem> GetInputChildItems<TSourceNode, TChildItem>()
    //    where TSourceNode : GenericNode
    //{
    //    return InputGraphItems.OfType<TSourceNode>().SelectMany(p => p.ContainedItems.OfType<TChildItem>());
    //}

    //public IEnumerable<TChildItem> GetInputInheritedChildItems<TSourceNode, TChildItem>()
    //  where TSourceNode : GenericInheritableNode
    //{
    //    return InputGraphItems.OfType<TSourceNode>().SelectMany(p => p.ChildItemsWithInherited.OfType<TChildItem>());
    //}

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

    public override void NodeRemoved(IDiagramNode nodeData)
    {
        base.NodeRemoved(nodeData);
        
    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        
    }

    private void UpdateReferences()
    {
        foreach (var mirrorSection in Config.Sections.Where(p => p.ReferenceType != null && !p.AllowAdding))
        {
            NodeConfigSection section = mirrorSection;
            var mirrorItems = ChildItems.Where(p => p.GetType() == section.ReferenceType).Cast<GenericReferenceItem>().ToArray();
            var newItems = mirrorSection.GenericSelector(this).ToArray();
            var newItemIds = newItems.Select(p => p.Identifier);

            foreach (var item in newItems)
            {
                if (ChildItems.OfType<GenericReferenceItem>().Any(p => p.SourceIdentifier == item.Identifier)) continue;
                AddReferenceItem(mirrorItems, item, mirrorSection);
            }
            ChildItems.RemoveAll(p => p.GetType() == section.ChildType && p is GenericReferenceItem && !newItemIds.Contains(((GenericReferenceItem)p).SourceIdentifier));
        }
    }

    internal void AddReferenceItem(GenericReferenceItem[] mirrorItems, IGraphItem item, NodeConfigSection mirrorSection)
    {
        var current = mirrorItems.FirstOrDefault(p => p.SourceIdentifier == item.Identifier);
        if (current != null && !mirrorSection.AllowDuplicates) return;

        var newMirror = Activator.CreateInstance(mirrorSection.ChildType) as GenericReferenceItem;
        newMirror.SourceIdentifier = item.Identifier;
        newMirror.Node = this;
        UnityEngine.Debug.Log("Added referenceitem" + newMirror.Name);
        ChildItems.Add(newMirror);
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

    public override string Name
    {
        get
        {
            if (SourceItemObject == null)
            {
                return "Missing";
            }
            return SourceItemObject.Name;
        }
    }

    public string SourceIdentifier
    {
        get { return _sourceIdentifier; }
        set { _sourceIdentifier = value; }
    }

    public IDiagramNodeItem SourceItemObject
    {
        get
        {
            return Node.Project.NodeItems.Cast<IDiagramNodeItem>().Concat(Node.Project.NodeItems.SelectMany(p => p.ContainedItems))

                .FirstOrDefault(p => p.Identifier == SourceIdentifier);
        }
    }

    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);

        SourceIdentifier = cls["SourceIdentifier"].Value;
    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        if (!string.IsNullOrEmpty(SourceIdentifier))
            cls.Add("SourceIdentifier", SourceIdentifier);
    }
}

//public class GenericInheritanceReference : GenericNodeChildItem
//{
//    public string InputName { get; set; }

//    public override string Name
//    {
//        get { return InputName; }
//        set { base.Name = value; }
//    }
//}