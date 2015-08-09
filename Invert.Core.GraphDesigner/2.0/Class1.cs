using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Data;
using Invert.Json;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class Workspace : IDataRecord, IDataRecordRemoved
    {
        private string _name;
        private string _currentGraphId;

        [JsonProperty]
        public string Identifier { get; set; }

        public bool Changed { get; set; }

        [JsonProperty]
        public string Name
        {
            get { return _name; }
            set {
                this.Changed("Name", _name, value);
                _name = value;
            }
        }

        public IRepository Repository { get; set; }

        [JsonProperty]
        public string CurrentGraphId
        {
            get { return _currentGraphId; }
            set {
                this.Changed("CurrentGraphId", _currentGraphId, value);
                _currentGraphId = value;
                
            }
        }

        public IGraphData CurrentGraph
        {
            get { return Repository.GetById<IGraphData>(CurrentGraphId); }
            set { CurrentGraphId = value.Identifier; }
        }

        public IEnumerable<IGraphData> Graphs
        {
            get
            {
                return Repository.All<WorkspaceGraph>()
                  .Where(_ => _.WorkspaceId == Identifier)
                  .Select(x => Repository.GetById<IGraphData>(x.GraphId));
            }
        }

        public void AddGraph(IGraphData data)
        {
            var workspaceGraph = Repository.Create<WorkspaceGraph>();
            workspaceGraph.GraphId = data.Identifier;
            workspaceGraph.WorkspaceId = Identifier;

        }

        public void Save()
        {
            Repository.Commit();
        }

        public IGraphData CreateGraph(Type to)
        {
            var graph = Activator.CreateInstance(to) as IGraphData;
            graph.Name = string.Format("{0}Graph", to.Name);
            Repository.Add(graph);

            var workspaceGraph = Repository.Create<WorkspaceGraph>();
            workspaceGraph.GraphId = graph.Identifier;
            workspaceGraph.WorkspaceId = this.Identifier;

            Repository.Commit();
            return graph;
        }

        public void RecordRemoved(IDataRecord record)
        {
            if (CurrentGraphId == record.Identifier)
                CurrentGraphId = Graphs.Select(p => p.Identifier).FirstOrDefault();
        }
    }

    public class WorkspaceGraph : IDataRecord, IDataRecordRemoved
    {
        private string _workspaceId;
        private string _graphId;

        [JsonProperty]
        public string GraphId
        {
            get { return _graphId; }
            set {
                this.Changed("GraphId", _graphId, value);
                _graphId = value;

            }
        }

        [JsonProperty]
        public string WorkspaceId
        {
            get { return _workspaceId; }
            set {
                this.Changed("WorkspaceId", _workspaceId, value);
                _workspaceId = value;
              
            }
        }

        public IRepository Repository { get; set; }

        [JsonProperty]
        public string Identifier { get; set; }

        public bool Changed { get; set; }
        public void RecordRemoved(IDataRecord record)
        {
            if (record.Identifier == GraphId || record.Identifier == WorkspaceId)
                Repository.Remove(this);
            
        }
    }

    public class FilterItem : IDataRecord , IDataRecordRemoved
    {
        private bool _collapsed;
        private string _nodeId;
        private string _filterId;
        private Vector2 _position;
        public IRepository Repository { get; set; }

        [JsonProperty]
        public string Identifier { get; set; }

        public bool Changed { get; set; }

        [JsonProperty]
        public bool Collapsed
        {
            get { return _collapsed; }
            set { this.Changed("Collapsed", _collapsed, value);
                _collapsed = value;
                
            }
        }

        [JsonProperty]
        public string NodeId
        {
            get { return _nodeId; }
            set {
                this.Changed("NodeId", _nodeId, value);
                _nodeId = value;
           
            }
        }

        [JsonProperty]
        public string FilterId
        {
            get { return _filterId; }
            set {
                this.Changed("FilterId", _filterId, value);
                _filterId = value;
            }
        }

        public IDiagramNode Node
        {
            get
            {
                return Repository.GetById<IDiagramNode>(NodeId);
            }
        }
        public IDiagramFilter Filter
        {
            get
            {
                return Repository.GetById<IDiagramFilter>(FilterId);
            }
        }

        [JsonProperty]
        public Vector2 Position
        {
            get { return _position; }
            set {
               
                _position = value;
                Changed = true;
            }
        }

        public void RecordRemoved(IDataRecord record)
        {
            if (NodeId == record.Identifier || FilterId == record.Identifier)
                Repository.Remove(this);
            
        }
    }


}
