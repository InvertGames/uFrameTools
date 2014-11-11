using System.Collections.Generic;

public interface IConnectable : IGraphItem
{

    IEnumerable<IGraphItem> ConnectedGraphItems { get; }
    List<string> ConnectedGraphItemIds { get; set; }
}