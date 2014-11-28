using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
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
        //Dictionary<string, string> GetProjectDiagrams();
        IGraphData CreateNewDiagram(Type diagramType,IDiagramFilter defaultFilter = null);
        Type RepositoryFor { get; }
        IGraphData CurrentGraph { get; set; }
        IEnumerable<IGraphData> Graphs { get; set; }
        GeneratorSettings GeneratorSettings { get; set; }
        void Refresh();
        void HideNode(string identifier);
    }
}