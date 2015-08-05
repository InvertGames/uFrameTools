using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Data;
using Invert.Json;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class Workspace : IDataRecord
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
            set { _name = value;
                Changed = true;
            }
        }

        public IRepository Repository { get; set; }

        [JsonProperty]
        public string CurrentGraphId
        {
            get { return _currentGraphId; }
            set { _currentGraphId = value;
                Changed = true;
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
    }

    public class WorkspaceGraph : IDataRecord, IDataRecordRemoved
    {
        private string _workspaceId;
        private string _graphId;

        [JsonProperty]
        public string GraphId
        {
            get { return _graphId; }
            set { _graphId = value;
                Changed = true;
            }
        }

        [JsonProperty]
        public string WorkspaceId
        {
            get { return _workspaceId; }
            set { _workspaceId = value;
                Changed = true;
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
            set { _collapsed = value;
                Changed = true;
            }
        }

        [JsonProperty]
        public string NodeId
        {
            get { return _nodeId; }
            set { _nodeId = value;
                Changed = true;
            }
        }

        [JsonProperty]
        public string FilterId
        {
            get { return _filterId; }
            set { _filterId = value;
                Changed = true;
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
            set { _position = value;
                Changed = true;
            }
        }

        public void RecordRemoved(IDataRecord record)
        {
            //if (NodeId == record.Identifier || FilterId == record.Identifier)
            //    Repository.Remove(this);
            
        }
    }


}
