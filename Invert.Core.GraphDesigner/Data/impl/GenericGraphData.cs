using Invert.Core.GraphDesigner;

public class GenericGraphData<T> : InvertGraph where T : IDiagramFilter, new()
{
    public T FilterNode
    {
        get { return (T)RootFilter; }
    }

    public override IDiagramFilter CreateDefaultFilter()
    {
        return new T()
        {
            Name = Name,
        };
    }
}