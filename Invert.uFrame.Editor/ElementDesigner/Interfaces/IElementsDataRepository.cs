using System;
using System.Collections.Generic;
using Invert.uFrame.Editor.ElementDesigner;
using UnityEditor;

public interface IElementsDataRepository : INodeRepository
{
    
    IElementDesignerData LoadDiagram(string path);
    void SaveDiagram(INodeRepository data);
    void RecordUndo(INodeRepository data, string title);
    void MarkDirty(INodeRepository data);
    Dictionary<string, string> GetProjectDiagrams();
    void CreateNewDiagram();
    Type RepositoryFor { get; }
    
}