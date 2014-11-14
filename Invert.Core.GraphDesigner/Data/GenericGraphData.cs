public class GenericGraphData<T> : GraphData where T : IDiagramFilter, new()
{
    public T FilterNode
    {
        get { return (T)RootFilter; }
    }
    protected override IDiagramFilter CreateDefaultFilter()
    {
        return new T()
        {
            Name = name
        };
    }
}