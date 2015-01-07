using System;
using System.Linq;
using Invert.Core.GraphDesigner;
using Invert.GraphDesigner.Documentation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xipton.Razor;
using Xipton.Razor.Config;

namespace Invert.Core.Tests
{
    [TestClass]
    public class RazorTemplateTest : InvertTest
    {
        public RazorTemplateTest()
        {
            InvertApplication.CachedAssemblies.Add(typeof(DocumentationPlugin).Assembly);
        }

        [Inject]
        public ProjectService ProjectService { get; set; }

        [Inject]
        public IAssetManager AssetManager { get; set; }

        [TestMethod]
        public void Test1()
        {
            var docPlugin = Container.Resolve<DocumentationPlugin>();
            var project = ProjectService.Projects[0];
            project.CreateNewDiagram(typeof(PluginGraphData), new ShellPluginNode());
            project.AddNode(new ShellNodeTypeNode());
            var node = project.NodeItems.OfType<ShellNodeTypeNode>().First();
            foreach (var template in docPlugin.Machine.GetRegisteredInMemoryTemplates())
            {
                Console.WriteLine(template.Key);
                Console.WriteLine(template.Value);
                
            }
            var temp = docPlugin.Machine.Context.TemplateFactory.CreateTemplateInstance("~/Page1.cshtml", false);
            
            var result = (TemplateBase)docPlugin.Machine.Execute("~/Page1.cshtml",node, null, true,true );
            var genericType = result.GetType().GetGenericParameter();
            Console.WriteLine(genericType.Name);
            Console.WriteLine(result);
        }
    }
}
