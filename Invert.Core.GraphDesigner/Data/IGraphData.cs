using System.Collections.Generic;
using Invert.Core.GraphDesigner;


public interface IGraphData : IElementFileData
{
    string Identifier { get; set; }

    int RefactorCount { get; set; }
    
    string Version { get; set; }

    // Not Persisted
    FilterState FilterState { get; set; }

    // Filters
    IDiagramFilter RootFilter { get;  }
    ICodePathStrategy CodePathStrategy { get; set; }

    //IEnumerable<ConnectionData> Connections { get; }
    void AddConnection(IGraphItem output, IGraphItem input);
    void RemoveConnection(IGraphItem output, IGraphItem input);
    void ClearOutput(IGraphItem output);
    void ClearInput(IGraphItem input);
    void SetProject(IProjectRepository value);
}