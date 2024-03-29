using System;
using System.Collections.Generic;
using Invert.uFrame.Editor;
using UnityEngine;

public interface IProjectRepository : INodeRepository
{
    bool GetSetting(string key, bool def = true);
    bool SetSetting(string key, bool value);

    Vector2 GetItemLocation(IDiagramNode node);
    void SetItemLocation(IDiagramNode node,Vector2 position);
    IGraphData LoadDiagram(string path);
    void SaveDiagram(INodeRepository data);
    void RecordUndo(INodeRepository data, string title);
    void MarkDirty(INodeRepository data);
    Dictionary<string, string> GetProjectDiagrams();
    IGraphData CreateNewDiagram(Type diagramType = null, IDiagramFilter defaultFilter = null);
    Type RepositoryFor { get; }
    GraphData CurrentGraph { get; set; }
    List<GraphData> Diagrams { get; set; }
    GeneratorSettings GeneratorSettings { get; set; }
    void Refresh();
    void HideNode(string identifier);
}