using Invert.Core.GraphDesigner;
using Invert.Core.GraphDesigner.Two;

public class GenericGraphData<T> : InvertGraph where T : IDiagramFilter, new()
{
    public T FilterNode
    {
        get { return (T)RootFilter; }
    }

    public override IDiagramFilter CreateDefaultFilter()
    {
        var filterItem = new T()
        {
            
        };
        Repository.Add(filterItem);
        var item = Repository.Create<FilterItem>();
        item.NodeId = filterItem.Identifier;
        item.FilterId = filterItem.Identifier;
        Repository.Commit();
        return filterItem;
    }
}