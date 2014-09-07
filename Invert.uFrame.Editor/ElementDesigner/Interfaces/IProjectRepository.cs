using System;
using System.Collections.Generic;

public interface IProjectRepository : INodeRepository
{
    
    IElementDesignerData LoadDiagram(string path);
    void SaveDiagram(INodeRepository data);
    void RecordUndo(INodeRepository data, string title);
    void MarkDirty(INodeRepository data);
    Dictionary<string, string> GetProjectDiagrams();
    void CreateNewDiagram();
    Type RepositoryFor { get; }
    INodeRepository CurrentDiagram { get; set; }
    List<JsonElementDesignerData> Diagrams { get; set; }
    void Refresh();
}