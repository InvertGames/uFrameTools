using System;
using System.Collections.Generic;
using Invert.uFrame.Editor;
using UnityEngine;

public interface IProjectRepository : INodeRepository
{
    Vector2 GetItemLocation(IDiagramNode node);
    void SetItemLocation(IDiagramNode node,Vector2 position);
    IElementDesignerData LoadDiagram(string path);
    void SaveDiagram(INodeRepository data);
    void RecordUndo(INodeRepository data, string title);
    void MarkDirty(INodeRepository data);
    Dictionary<string, string> GetProjectDiagrams();
    IElementDesignerData CreateNewDiagram();
    Type RepositoryFor { get; }
    JsonElementDesignerData CurrentDiagram { get; set; }
    List<JsonElementDesignerData> Diagrams { get; set; }
    GeneratorSettings GeneratorSettings { get; set; }
    void Refresh();
    void HideNode(string identifier);
}