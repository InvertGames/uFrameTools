using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Data;
using Invert.Json;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Two
{
    public class Workspace : IDataRecord
    {
        [JsonProperty]
        public string Identifier { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        public IRepository Repository { get; set; }

        [JsonProperty]
        public string CurrentGraphId { get; set; }

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

    public class WorkspaceGraph : IDataRecord
    {
        [JsonProperty]
        public string GraphId { get; set; }

        [JsonProperty]
        public string WorkspaceId { get; set; }

        public IRepository Repository { get; set; }

        [JsonProperty]
        public string Identifier { get; set; }
    }

    public class FilterItem : IDataRecord , IDataRecordRemoved
    {
        public IRepository Repository { get; set; }

        [JsonProperty]
        public string Identifier { get; set; }

        [JsonProperty]
        public bool Collapsed { get; set; }
        
        [JsonProperty]
        public string NodeId { get; set; }

        [JsonProperty]
        public string FilterId { get; set; }

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
        public Vector2 Position { get; set; }

        public void RecordRemoved(IDataRecord record)
        {
            if (NodeId == record.Identifier)
                Repository.Remove(this);
            if (FilterId == record.Identifier)
                Repository.Remove(this);
        }
    }

    public class Connection : IDataRecord
    {
        public IRepository Repository { get; set; }
        public string Identifier { get; set; }
        [JsonProperty]
        public string SourceId { get; set; }
        [JsonProperty]
        public string TargetId { get; set; }
    }
}
