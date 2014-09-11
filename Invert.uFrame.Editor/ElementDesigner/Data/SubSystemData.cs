using System;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor;
using UnityEngine;


public static class SubsystemExtensions
{
    public static IEnumerable<SubSystemData> GetAllImportedSubSystems(this SubSystemData subsystem,INodeRepository data)
    {
        var subSystem = data.NodeItems.OfType<SubSystemData>()
            .Where(p => subsystem.Imports.Contains(p.Identifier));

        foreach (var subSystemData in subSystem)
        {
            yield return subSystemData;
            foreach (var systemData in subSystemData.GetAllImportedSubSystems(subSystemData.Data))
            {
                yield return systemData;
            }
        }
    }
    public static IEnumerable<string> GetAllImports(this SubSystemData subsystem)
    {
        return subsystem.GetAllImportedSubSystems(subsystem.Data).SelectMany(p => p.Imports).Concat(subsystem.Imports);
    }
    public static IEnumerable<IDiagramNodeItem> GetIncludedItems(this SubSystemData subsystem)
    {
        foreach (var allDiagramItem in subsystem.Data.NodeItems.OfType<SubSystemData>())
        {
            if (subsystem.Imports.Contains(allDiagramItem.Identifier))
            {
                foreach (var item in allDiagramItem.GetSubItems())
                {
                    yield return item;
                }
            }
        }
    }
    public static IEnumerable<IDiagramNodeItem> GetSubItems(this SubSystemData subsystem)
    {
        var items =
            subsystem.Data.NodeItems.OfType<ElementData>()
                .Where(p => subsystem.Data.PositionData.HasPosition(subsystem,p))
                .Cast<IDiagramNodeItem>();

        foreach (var item in items)
            yield return item;
    }
    public static IEnumerable<ViewModelCommandData> GetIncludedCommands(this SubSystemData subsystem)
    {
        return subsystem.GetIncludedElements().Where(p => !p.IsMultiInstance).SelectMany(p => p.Commands);
    }

    public static IEnumerable<ElementData> GetIncludedElements(this SubSystemData subsystem)
    {
        var list = new List<ElementData>();
        foreach (var diagramSubItem in subsystem.GetSubItems().OfType<ElementData>())
        {
            list.Add(diagramSubItem);
        }
        foreach (var allDiagramItem in subsystem.Data.NodeItems.OfType<SubSystemData>())
        {
            if (subsystem.Imports.Contains(allDiagramItem.Identifier))
            {
                foreach (var nested in allDiagramItem.GetIncludedElements())
                {
                    list.Add(nested);
                }
            }
        }
        return list.Distinct();
    }
}
[Serializable]
public class SubSystemData : DiagramNode
{
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

    [SerializeField]
    private FilterCollapsedDictionary _collapsedValues = new FilterCollapsedDictionary();

    [SerializeField]
    private List<string> _imports = new List<string>();


    public override IEnumerable<IDiagramNodeItem> ContainedItems
    {
        get { yield break; }
        set { }
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
        get
        {
          yield break;
        }
    }

    public override string Label
    {
        get { return Name; }
    }


    public override void RemoveFromDiagram()
    {
        base.RemoveFromDiagram();
        Data.RemoveNode(this);
    }

}