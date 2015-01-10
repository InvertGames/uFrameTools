using System.Collections.Generic;
using Invert.Core.GraphDesigner;
using UnityEngine;

public interface INodeRepository 
{
    // Basic Information
    string Name { get; set; }
    IEnumerable<IDiagramNode> NodeItems { get; }
    IEnumerable<IGraphItem> AllGraphItems { get; }
    IEnumerable<ConnectionData> Connections { get; }

    // Settings
    ElementDiagramSettings Settings { get; }

    void AddNode(IDiagramNode data);
    void RemoveNode(IDiagramNode enumData);
    IDiagramFilter CurrentFilter { get; }
    FilterPositionData PositionData { get; set; }
    string Namespace { get; set; }
    void RemoveItem(IDiagramNodeItem nodeItem);
    void AddItem(IDiagramNodeItem item);

    void Save();
    void RecordUndo(INodeRepository data, string title);
    void MarkDirty(INodeRepository data);
    void SetItemLocation(IDiagramNode node, Vector2 position);
    Vector2 GetItemLocation(IDiagramNode node);
    void HideNode(string identifier);
}