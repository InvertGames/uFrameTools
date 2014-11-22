using System.Collections.Generic;
using Invert.Core.GraphDesigner;

public interface INodeRepository 
{
    // Basic Information
    string Name { get; }
    IEnumerable<IDiagramNode> NodeItems { get; }
    IEnumerable<IGraphItem> AllGraphItems { get; }
    IEnumerable<ConnectionData> Connections { get; }

    // Settings
    ElementDiagramSettings Settings { get; }
    void AddNode(IDiagramNode data);
    void RemoveNode(IDiagramNode enumData);
    IDiagramFilter CurrentFilter { get; }
    FilterPositionData PositionData { get; }
    void RemoveItem(IDiagramNodeItem nodeItem);
    void AddItem(IDiagramNodeItem item);
}