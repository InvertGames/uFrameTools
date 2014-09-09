using System;
using System.Collections.Generic;
using Invert.uFrame.Editor;

public interface IProjectRepository : INodeRepository
{
    
    IElementDesignerData LoadDiagram(string path);
    void SaveDiagram(INodeRepository data);
    void RecordUndo(INodeRepository data, string title);
    void MarkDirty(INodeRepository data);
    Dictionary<string, string> GetProjectDiagrams();
    void CreateNewDiagram();
    Type RepositoryFor { get; }
    JsonElementDesignerData CurrentDiagram { get; set; }
    List<JsonElementDesignerData> Diagrams { get; set; }
    GeneratorSettings GeneratorSettings { get; set; }
    void Refresh();
}