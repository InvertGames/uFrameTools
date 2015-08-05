using System;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Data;
using Invert.uFrame.ECS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class CodeGeneratorTests
    {
        [TestInitialize]
        public void Init()
        {
            // From some reason we need to reference the type of the appdomain
            var t = typeof(ModuleGraph);
            InvertApplication.CachedAssemblies.Add(typeof(RepositoryTests).Assembly);

        }

        [TestMethod]
        public void TestGetAllCodeGenerators()
        {
            var Db = InvertGraphEditor.Container.Resolve<IRepository>() as TypeDatabase;
            var generators = InvertGraphEditor.GetAllCodeGenerators(
                new GraphConfiguration(@"D:\TestOutput", "MyNamespaceA"), Db.AllOf<IDataRecord>().ToArray(), true);

            Assert.AreNotEqual(0, generators.Count());
        }

        [TestMethod]
        public void TestGetAllFileGenerators()
        {
            var Db = InvertGraphEditor.Container.Resolve<IRepository>() as TypeDatabase;
            var generators = InvertGraphEditor.GetAllFileGenerators(
                new GraphConfiguration(@"D:\TestOutput", "MyNamespaceA"), Db.AllOf<IDataRecord>().ToArray(), true).ToArray();
            foreach (var item in generators)
            {
                Console.WriteLine(item.SystemPath);
            }
            Assert.AreNotEqual(0, generators.Length);
        }
        [TestMethod]
        public void TestSaveCommand()
        {
            InvertGraphEditor.Container.RegisterInstance<IGraphConfiguration>(
                new GraphConfiguration(@"D:\\TestOutputCode", "MyNamespaceTest"));
            var command = new SaveCommand();
            var enumerator = command.Generate();
            while (enumerator.MoveNext())
            {
                var progress = enumerator.Current as TaskProgress;
                if (progress != null)
                {
                    Console.WriteLine(progress.Message);
                }
            }
        }
    }
}
