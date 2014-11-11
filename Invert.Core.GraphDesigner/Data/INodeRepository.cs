using System.Collections.Generic;

public interface INodeRepository 
{
    // Basic Information
    string Name { get; }
    IEnumerable<IDiagramNode> NodeItems { get; }

    // Settings
    ElementDiagramSettings Settings { get; }
    void AddNode(IDiagramNode data);
    void RemoveNode(IDiagramNode enumData);
    IDiagramFilter CurrentFilter { get; }
    FilterPositionData PositionData { get; }
    void RemoveItem(IDiagramNodeItem nodeItem);
    void AddItem(IDiagramNodeItem item);
}