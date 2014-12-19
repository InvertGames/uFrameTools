using System;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Json;


public interface IGraphData : IElementFileData,IItem
{
    string Identifier { get; set; }

    int RefactorCount { get; set; }
    
    string Version { get; set; }

    // Not Persisted
    FilterState FilterState { get; set; }

    // Filters
    IDiagramFilter RootFilter { get; set; }
    ICodePathStrategy CodePathStrategy { get; set; }
    bool Errors { get; set; }
    Exception Error { get; set; }
    string Path { get; set; }
    IProjectRepository Project { get; }

    //IEnumerable<ConnectionData> Connections { get; }
    void AddConnection(IGraphItem output, IGraphItem input);
    void RemoveConnection(IGraphItem output, IGraphItem input);
    void ClearOutput(IGraphItem output);
    void ClearInput(IGraphItem input);
    void SetProject(IProjectRepository value);
    void DeserializeFromJson(JSONNode graphData);
}