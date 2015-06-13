using System;
using System.IO;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.IOC;
using Xipton.Razor;
using Xipton.Razor.Core;

namespace Invert.GraphDesigner.Documentation
{
    public class DocumentationPlugin : DiagramPlugin
    {

        public static string TemplatePath
        {
            get { return InvertGraphEditor.Prefs.GetString("RazorTemplatePath", "Templates\\"); }
            set
            {
                InvertGraphEditor.Prefs.SetString("RazorTemplatePath", value);
            }
        }

        public override decimal LoadPriority
        {
            get { return 1; }
        }

        public RazorMachine Machine { get; set; }
        public override void Initialize(UFrameContainer container)
        {
            Machine = new RazorMachine();
            TemplatePath = @"D:\Invert\uFrameGit\uFrameTools\WPF\uFrameWPF\Invert.GraphDesigner.Documentation\Templates\";
            if (TemplatePath == null)
            {
                return;
            }
            var templates = Directory.GetFiles(TemplatePath, "*.cshtml", SearchOption.AllDirectories);
            foreach (var template in templates)
            {
                try
                {
                    Machine.RegisterTemplate("~/" + Path.GetFileName(template), File.ReadAllText(template));
                }
                catch (Exception ex)
                {
                    InvertApplication.LogException(ex);
                    continue;
                }
            }
            foreach (var template in Machine.GetRegisteredInMemoryTemplates())
            {
                var t = Machine.Context.TemplateFactory.CreateTemplateInstance(template.Key);
                var nodeType = t.GetType().GetGenericParameter();
                var nodeConfig = container.GetNodeConfig(nodeType);
                if (nodeConfig != null)
                {
                    nodeConfig.AddOutputGenerator(() => new RazorOutputGenerator()
                    {
                        TemplateInstance = t,
                        AlwaysRegenerate = true,
                        GeneratorFor = nodeType,

                    });
                }
                else
                {
                    
                    container.RegisterInstance<DesignerGeneratorFactory>(
                        new GraphGeneratorFactory(
                            (data) => new RazorOutputGenerator()
                            {
                                
                                TemplateInstance = t,
                                AlwaysRegenerate = true,
                                GeneratorFor = nodeType,
                                ObjectData = data,
                            }),t.GetType().Name);
                }
                
                //nodeConfig.

            }
        }

    }

    public class RazorOutputGenerator : OutputGenerator
    {
        public ITemplate TemplateInstance { get; set; }
        public override Type GeneratorFor { get; set; }

        public override string Filename
        {
            get { return string.Format("Documentation/{0}_{1}.html", ObjectData.GetType().Name , TemplateInstance.GetType().Name); }
            set { base.Filename = value; }
        }

        public override string ToString()
        {
            var templateController = TemplateInstance as ITemplateController;
            return templateController.SetModel(ObjectData).Execute().Result;
            //return TemplateInstance.Result;
        }
    }
    public static class RazorExtensions
    {
        public static void AddDocumentationTemplate()
        {
            
        }
    }
}