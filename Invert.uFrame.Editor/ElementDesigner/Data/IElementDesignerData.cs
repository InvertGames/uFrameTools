using System.Collections.Generic;
using Invert.uFrame.Editor.Refactoring;
using UnityEngine;

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
}


public interface IElementFileData : INodeRepository
{
       



    /// <summary>
    /// Should be called when first loaded.
    /// </summary>
    void Initialize();
}

public interface IElementDesignerData : IElementFileData
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