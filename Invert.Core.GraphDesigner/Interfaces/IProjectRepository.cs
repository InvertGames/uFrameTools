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
        string ProjectNamespace { get; set; }
        void Refresh();
        void HideNode(string identifier);
        IEnumerable<OpenGraph> OpenGraphs { get;  }

        void CloseGraph(OpenGraph tab);
        void Save();
    }

    [Serializable]
    public class OpenGraph
    {
        [SerializeField]
        private string[] _path;

        [SerializeField]
        private string _graphName;

        [SerializeField]
        private string _graphIdentifier;

        public string GraphName
        {
            get { return _graphName; }
            set { _graphName = value; }
        }

        public string[] Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public string GraphIdentifier
        {
            get { return _graphIdentifier; }
            set { _graphIdentifier = value; }
        }
    }
}