using System.Collections.Generic;
using Invert.uFrame.Editor.Refactoring;

public interface IElementFileData
{
    string Identifier { get; set; }
    int RefactorCount { get; set; }
    IDiagramFilter CurrentFilter { get; }
    
    IEnumerable<IDiagramNode> AllDiagramItems { get; }
    

    /// <summary>
    /// Should be called when first loaded.
    /// </summary>
    void Initialize();

    void AddNode(IDiagramNode data);
    void RemoveNode(IDiagramNode enumData);

    // TemporaryData
    List<IDiagramLink> Links { get; }
}

public interface IElementDesignerData : IElementFileData
{
    // External Diagram support
    List<string> ExternalReferences { get; set; }
    // Settings
    ElementDiagramSettings Settings { get; }

    // Basic Information
    string Name { get; }
    string Version { get; set; }

    IEnumerable<IDiagramNode> LocalNodes { get; } 
    // Not Persisted
    FilterState FilterState { get; set; }

    // Filters
    SceneFlowFilter SceneFlowFilter { get;  }

}