using System;
using System.Collections.Generic;
using System.IO;
using Invert.Core.GraphDesigner;
using Invert.GraphDesigner.WPF;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Invert.Core.Tests
{


    public class InvertTest
    {
        public IUFrameContainer Container
        {
            get { return InvertApplication.Container; }
        }
        [TestInitialize]
        public virtual void Setup()
        {
            InvertGraphEditor.Prefs = new WindowsPrefs();
            InvertApplication.Container = null;
            InvertApplication.IsTestMode = true;
            InvertApplication.Container.Inject(this);
        }

    }

    [TestClass]
    public class EventListenerTests : InvertTest
    {
        static EventListenerTests()
        {
            InvertApplication.CachedAssemblies.Add(typeof(EventListenerTests).Assembly);
        }

        [Inject]
        public ProjectService ProjectService { get; set; }

        [Inject]
        public IAssetManager AssetManager { get; set; }

        
        public override void Setup()
        {
            base.Setup();
          

            PluginTest.ProjectLoadedInvokeCount = 0;
            PluginTest.ProjectRemovedInvokeCount = 0;
            PluginTest.ProjectUnloadedInvokeCount = 0;

     

        }
        [TestMethod]
        public void TestProjectEvents()
        {
//            ProjectService.RefreshProjects();

        }


    }

    [TestClass]
    public class GraphEventsTest : InvertTest
    {
        [Inject]
        public ProjectService ProjectService { get; set; }

        [Inject]
        public IAssetManager AssetManager { get; set; }

        [TestMethod]
        public void TestGraphEvents()
        {

            PluginTest.GraphCreatedInvokeCount = 0;
            PluginTest.GraphLoadedInvokeCount = 0;
            PluginTest.GraphRemovedInvokeCount = 0;
            var project = ProjectService.Projects[0];
            project.CreateNewDiagram(typeof(PluginGraphData), new ShellPluginNode());
            ProjectService.RefreshProjects();
            Assert.AreEqual(1, PluginTest.ProjectLoadedInvokeCount);

            TestAssetManager._projects = new IProjectRepository[0];

            ProjectService.RefreshProjects(); 
            ProjectService.RefreshProjects();
            ProjectService.RefreshProjects();

            Assert.AreEqual(1, PluginTest.ProjectRemovedInvokeCount);
            Assert.AreEqual(1, PluginTest.GraphCreatedInvokeCount);
            Assert.AreEqual(1, PluginTest.GraphLoadedInvokeCount);

        }

    }
    public class PluginTest : DiagramPlugin, IProjectEvents, IGraphEvents
    {

        public static int GraphCreatedInvokeCount { get; set; }
        public static int GraphLoadedInvokeCount { get; set; }
        public static int GraphRemovedInvokeCount { get; set; }
        public static int ProjectLoadedInvokeCount { get; set; }
        public static int ProjectRemovedInvokeCount { get; set; }
        public static int ProjectUnloadedInvokeCount { get; set; }

        [Inject]
        public ProjectService ProjectService { get; set; }

        public override void Initialize(uFrameContainer container)
        {
            container.RegisterInstance<IAssetManager>(new TestAssetManager(new IProjectRepository[] { new JsonProjectRepository(new FileInfo("Test.json"), null, null), }));
            //container.ListenToProjectEvents(this);
        }

        public override void Loaded(uFrameContainer container)
        {
            base.Loaded(container);
            ProjectService.Subscribe(this);
        }

        public void ProjectLoaded(IProjectRepository project)
        {
            Console.WriteLine("Project Loaded - " + project.Name);
            ProjectLoadedInvokeCount++;
            project.Subscribe(this);
        }

        public void ProjectUnloaded(IProjectRepository project)
        {
            ProjectUnloadedInvokeCount++;
            project.Unsubscribe(this);
        }

        public void ProjectRemoved(IProjectRepository project)
        {
            ProjectRemovedInvokeCount++;
            project.Unsubscribe(this);
        }

        public void GraphCreated(IProjectRepository project, IGraphData graph)
        {
            GraphCreatedInvokeCount++;
        }

        public void GraphLoaded(IProjectRepository project, IGraphData graph)
        {
            GraphLoadedInvokeCount++;
        }

        public void GraphDeleted(IProjectRepository project, IGraphData graph)
        {
            GraphRemovedInvokeCount++;
            project.Unsubscribe(this);
        }
    }

    public class TestAssetManager : IAssetManager
    {
        public static IProjectRepository[] _projects;

        public TestAssetManager(IProjectRepository[] projects)
        {
            _projects = projects;
        }

        public object CreateAsset(Type type)
        {
            throw new NotImplementedException();
        }

        public object LoadAssetAtPath(string path, Type repositoryFor)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetAssets(Type type)
        {
            return _projects;
        }
    }
}
