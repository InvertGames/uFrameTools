using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        private TypeDatabase _db;

        public TypeDatabase Db
        {
            get { return _db ?? (_db = new TypeDatabase(new JsonRepositoryFactory() { RootPath = "TestDatabase" })); }
            set { _db = value; }
        }

        [TestMethod]
        public void TestInsert()
        {
            var workspace = Db.Create<Workspace>();
            workspace.Name = "Micah";
            
            var scrollerModule = Db.Create<GraphData>();
            scrollerModule.Name = "Scroller Module";
            
            var gameModule = Db.Create<GraphData>();
            gameModule.Name = "Game Module";

            var workspaceGraph = Db.Create<WorkspaceGraphData>();
            workspaceGraph.GraphId = scrollerModule.Id;
            workspaceGraph.WorkspaceId = workspace.Id;

            var workspaceGraph2 = Db.Create<WorkspaceGraphData>();
            workspaceGraph2.GraphId = gameModule.Id;
            workspaceGraph2.WorkspaceId = workspace.Id;

            Db.Commit();
        }
        [TestMethod]
        public void TestRead()
        {
            foreach (var item in Db.GetAll<Workspace>())
            {
                Console.WriteLine(item.Name);
                foreach (var graph in item.Graphs)
                {
                    Console.WriteLine("---- {0}",graph.Name);
                }
            }
        }
    }
    
    public class Workspace : IDataRecord
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IRepository Repository { get; set; }

        public IEnumerable<GraphData> Graphs
        {
            get
            {
                return Repository.GetAll<WorkspaceGraphData>()
                  .Where(_ => _.WorkspaceId == Id)
                  .Select(x => Repository.GetSingle<GraphData>(x.GraphId));
            }
        }
    }

    public class WorkspaceGraphData : IDataRecord
    {
        public string GraphId { get; set; }
        public string WorkspaceId { get; set; }
        public IRepository Repository { get; set; }
        public string Id { get; set; }
    }

    public class GraphData : IDataRecord
    {
        public IRepository Repository { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
    }

}
