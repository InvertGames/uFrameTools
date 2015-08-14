﻿using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Data;
using Invert.Json;

namespace Invert.Core.GraphDesigner
{
    public class Workspace : IDataRecord, IDataRecordRemoved, IItem
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
                _name = value;
                this.Changed("Name", _name, value);
            }
        }

        public IRepository Repository { get; set; }

        [JsonProperty]
        public string CurrentGraphId
        {
            get { return _currentGraphId; }
            set {
                _currentGraphId = value;
                this.Changed("CurrentGraphId", _currentGraphId, value);
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

        public string Title
        {
            get { return Name; }
        }

        public string Group {
            get { return "Workspaces"; }
        }

        public string SearchTag
        {
            get { return Name; }
        }
    }
}