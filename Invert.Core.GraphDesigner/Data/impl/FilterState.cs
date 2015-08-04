using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Json;

[Serializable]
public class FilterState : IJsonObject {
    [NonSerialized]
    private Stack<IDiagramFilter> _filterStack = new Stack<IDiagramFilter>();

 
    //// Filters
    //public IDiagramFilter CurrentFilter
    //{
    //    get
    //    {
            
    //        return FilterStack.Peek();
    //    }
    //}

    public Stack<IDiagramFilter> FilterStack
    {
        get
        {
            return _filterStack ?? (_filterStack = new Stack<IDiagramFilter>());
        }
        set { _filterStack = value; }
    }

    public List<string> _persistedFilterStack = new List<string>();

    public void FilterPushed(IDiagramFilter filter)
    {
        if (!_persistedFilterStack.Contains(filter.Identifier))
            _persistedFilterStack.Add(filter.Identifier);
    }

    public void FilterPoped(IDiagramFilter pop)
    {
        _persistedFilterStack.Remove(pop.Identifier);
    }

    public void Reload(IGraphData graphData)
    {
        // TODO 2.0: Filter Stacks?
        //if (_persistedFilterStack.Count < 1) return;
        //if (_persistedFilterStack.Count != (FilterStack.Count))
        //{
        //    foreach (var filterName in _persistedFilterStack)
        //    {
        //        var filter = graphData.Repository.GetFilters().FirstOrDefault(p => p.Identifier == filterName);
        //        if (filter == null)
        //        {
        //            _persistedFilterStack.Clear();
        //            FilterStack.Clear();
        //            break;
        //        }
                
        //        //FilterStack.Push(filter);
        //        graphData.PushFilter(filter);
        //    }
        //}
    }

    public void Serialize(JSONClass cls)
    {
        cls.AddPrimitiveArray("FilterStack",_persistedFilterStack, i=>new JSONData(i));
    }

    public void Deserialize(JSONClass cls)
    {
        
        _persistedFilterStack = cls["FilterStack"].DeserializePrimitiveArray(n=>n.Value).ToList();
        
    }
}