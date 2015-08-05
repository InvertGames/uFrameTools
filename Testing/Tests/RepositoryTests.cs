using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Invert.Core.GraphDesigner;
using Invert.Data;
using Invert.Json;
using Invert.uFrame.ECS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class RepositoryTests
    {
        private TypeDatabase _db;

        public TypeDatabase Db
        {
            get { return _db ?? (_db = new TypeDatabase(new JsonRepositoryFactory("TestDatabase2"))); }
            set { _db = value; }
        }
        [TestMethod]
        public void TestBulkInsert()
        {
            using (new Benchmark())
            {
                for (var i = 0; i < 500; i++)
                {
                    var workspace = Db.Create<Workspace>();
                    workspace.Name = "Test " + i;
                }
                Db.Commit();
            }
        }
        [TestMethod]
        public void TestInsert()
        {
            using (new Benchmark())
            {
                try
                {
                    var workspace = Db.Create<Workspace>();
                    workspace.Name = "Micah";

                    var scrollerModule = Db.Create<InvertGraph>();
                    scrollerModule.Name = "Scroller Module";

                    var gameModule = Db.Create<InvertGraph>();
                    gameModule.Name = "Game Module";

                    workspace.AddGraph(gameModule);
                    workspace.AddGraph(scrollerModule);

                    Db.Commit();
                }
                catch (Exception ex)
                {
                    Db.Reset();
                }
            }

        }
        [TestMethod]
        public void TestRead()
        {
            using (new Benchmark())
            {
                foreach (var item in Db.All<Workspace>())
                {
                    Console.WriteLine(item.Name);
                    foreach (var graph in item.Graphs)
                    {
                        Console.WriteLine("---- {0}", graph.Name);
                    }
                }
            }
            using (new Benchmark())
            {
                foreach (var item in Db.All<Workspace>())
                {
                    Console.WriteLine(item.Name);
                    foreach (var graph in item.Graphs)
                    {
                        Console.WriteLine("---- {0}", graph.Name);
                    }
                }
            }
        }
        [TestMethod]
        public void TestInheritance()
        {
            var derived = Db.Create<DerivedTest>();
            Db.Commit();
            var result = Db.GetById<BaseTest>(derived.Identifier);
            
            Assert.IsTrue(Db.AllOf<BaseTest>().Any());
            Assert.IsNotNull(result);
            
            Db = null;
            Assert.IsTrue(Db.AllOf<BaseTest>().Any());
            Assert.IsNotNull(Db.GetById<BaseTest>(derived.Identifier));
            TestRemove();
        }

        [TestMethod]
        public void TestFilterItems()
        {
            var t = typeof (ModuleGraph);
            Db = new TypeDatabase(new JsonRepositoryFactory(@"D:\Invert\uFrameGit\uFrameDB"));
            foreach (var item in Db.AllOf<InvertGraph>())
            {
                Console.WriteLine(item.Name);
                Console.WriteLine(item.RootFilterId);
            }
            //foreach (var item in Db.AllOf<IDiagramFilter>())
            //{
            //    Console.WriteLine(item.Name);
            //    foreach (var child in item.FilterItems())
            //    {
            //        Console.WriteLine(child.Identifier);
            //        Assert.IsNotNull(child.Node);
            //    }
            //}
       



        }
        [TestMethod]
        public void TestRemove()
        {
            TestInsert();
            Db = null;
            Db.RemoveAll<BaseTest>();
            Db.RemoveAll<DerivedTest>();
            Db.RemoveAll<Workspace>();
            Db.RemoveAll<InvertGraph>();
            Db.RemoveAll<WorkspaceGraph>();
            Db.Commit();
            Db = null;
            var count = Db.All<Workspace>().Count();
            Assert.AreEqual(0,count);
        }

    }

    public class BaseTest : IDataRecord
    {
        public IRepository Repository { get; set; }
        [JsonProperty]
        public string Identifier { get; set; }

        public bool Changed { get; set; }
    }

    public class DerivedTest : BaseTest
    {
        
    }
    public class Benchmark : IDisposable
    {
        public DateTime Start;
        public Benchmark()
        {
            Start = DateTime.Now;

        }

        public void Dispose()
        {
            var ts = DateTime.Now.Subtract(Start);
            Console.WriteLine("Elapsed Time {0}", ts.ToString());
        }
    }


}
