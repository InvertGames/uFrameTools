namespace Invert.Core.GraphDesigner
{
    public interface INodeItemEvents
    {
        void Deleted(IDiagramNodeItem node);
        void Hidden(IDiagramNodeItem node);
        void Renamed(IDiagramNodeItem node, string previousName, string newName);
    }
}