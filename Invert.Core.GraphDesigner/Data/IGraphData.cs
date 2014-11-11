using Invert.uFrame.Editor.Refactoring;
using UnityEngine;


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
}