using System;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor;
using UnityEngine;


[Serializable]
public class SubSystemData : DiagramNode
{
    public IEnumerable<RegisteredInstanceData> AllInstances
    {
        get { return this.GetAllImportedSubSystems().Concat(new[] {this}).SelectMany(p => p.Instances).Reverse(); }
    }

    public IEnumerable<RegisteredInstanceData> AllInstancesDistinct
    {
        get
        {
            var instancesAdded = new List<string>();
            foreach (var instance in AllInstances)
            {
                if (!instancesAdded.Contains(instance.Name))
                {
                    yield return instance;
                }
                instancesAdded.Add(instance.Name);
            }
        }
    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        cls.AddPrimitiveArray("Imports", _imports, i => new JSONData(i));
    }

    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);

        _imports = cls["Imports"].AsArray.DeserializePrimitiveArray(n => n.Value).ToList();
    }

    public override void NodeItemRemoved(IDiagramNodeItem item)
    {
        Instances.Remove(item as RegisteredInstanceData);
    }

    [SerializeField]
    private FilterCollapsedDictionary _collapsedValues = new FilterCollapsedDictionary();

    [SerializeField]
    private List<string> _imports = new List<string>();

    public List<RegisteredInstanceData> Instances
    {
        get { return _instances ?? (_instances = new List<RegisteredInstanceData>()); }
        set { _instances = value; }
    }
    private List<RegisteredInstanceData> _instances;
    
    public override IEnumerable<IDiagramNodeItem> ContainedItems
    {
        get { return Instances.Cast<IDiagramNodeItem>(); }
        set { Instances = value.OfType<RegisteredInstanceData>().ToList(); }
    }

    public virtual List<string> Imports
    {
        get { return _imports; }
        set { _imports = value; }
    }


    public override string InfoLabel
    {
        get { return string.Format("Items: [{0}]", Locations.Keys.Count - 1); }
    }

    public override IEnumerable<IDiagramNodeItem> Items
    {
        get { return ContainedItems; }
    }

    public override string Label
    {
        get { return Name; }
    }

    public override void NodeRemoved(IDiagramNode nodeData)
    {
        base.NodeRemoved(nodeData);
        if (Imports.Contains(nodeData.Identifier))
        {
            Imports.Remove(nodeData.Identifier);
        }
        Instances.RemoveAll(p=>p.RelatedType == nodeData.Identifier);

    }

    public override void RemoveFromDiagram()
    {
        base.RemoveFromDiagram();
        Project.RemoveNode(this);
    }

}