//using System;
//using System.CodeDom;
//using System.CodeDom.Compiler;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using Invert.Core;
//using Invert.Core.GraphDesigner;
//using Microsoft.AspNet.Razor;
//using Microsoft.CSharp;

//namespace Invert.GraphDesigner.Documentation
//{
//    public class RazorEnginePlugin : DiagramPlugin
//    {
//        public static string TemplatePath
//        {
//            get { return InvertGraphEditor.Prefs.GetString("RazorTemplatePath", "Templates\\"); }
//            set
//            {
//                InvertGraphEditor.Prefs.SetString("RazorTemplatePath", value);
//            }
//        }

//        public override decimal LoadPriority
//        {
//            get { return 1; }
//        }

//        public static RazorEngineHost Host { get; set; }
//        public static RazorTemplateEngine Engine { get; set; }

//        public static Assembly TemplatesAssembly { get; set; }

//        [Inject]
//        public static PluginDesigner PluginDesigner { get; set; }

//        static RazorEnginePlugin()
//        {
//            InvertApplication.CachedAssemblies.Add(typeof(RazorEnginePlugin).Assembly);
//        }
//        public override void Initialize(uFrameContainer container)
//        {
//            var language = new CSharpRazorCodeLanguage();
//            Host = new RazorEngineHost(language)
//            {
//                DefaultBaseClass = "Invert.GraphDesigner.Documentation.uFrameRazorTemplateBase<TModel>",
//                DefaultClassName = "uFrameRazorTemplate",
//                DefaultNamespace = "CompiledRazorTemplates",
//            };
//            Engine =  new RazorTemplateEngine(Host);

//            TemplatePath = @"D:\Invert\uFrameGit\uFrameTools\WPF\uFrameWPF\Invert.GraphDesigner.Documentation\Templates\";
//            if (TemplatePath == null)
//            {
//                return;
//            }

//            var csharpCodeProvider = new CSharpCodeProvider();
//            var compileUnits = new List<GeneratorResults>();
//            var templates = Directory.GetFiles(TemplatePath, "*.cshtml", SearchOption.AllDirectories);
//            foreach (var template in templates)
//            {
//                try
//                {
//                    GeneratorResults razorResult = Engine.GenerateCode(new StringReader(File.ReadAllText(template)),Path.GetFileNameWithoutExtension(template),null,null);
//                    Debug.WriteLine(razorResult.GeneratedCode);
//                    compileUnits.Add(razorResult);
//                }
//                catch (Exception ex)
//                {
//                    InvertApplication.LogException(ex);
//                    continue;
//                }
//            }

//            var assemblyNames = InvertApplication.CachedAssemblies.Where(p=>p.FullName.StartsWith("Invert") || p.FullName.StartsWith("System")).Select(p => p.FullName).ToArray();
//            CompilerResults compilerResults =
//                new CSharpCodeProvider()
//                    .CompileAssemblyFromSource(new CompilerParameters()
//                    {
//                        ReferencedAssemblies = { "System.dll",
//                            "System.Xml.dll", "System.Xml.Linq.dll", "Invert.Core.dll", "Invert.GraphDesigner.WPF.dll", "Invert.GraphDesigner.Documentation.dll"},
//                    }, compileUnits.Select(p=>p.GeneratedCode).ToArray());

//            foreach (var error in compilerResults.Errors)
//            {
//                Debug.WriteLine(error);
//            }

//            var assembly = compilerResults.CompiledAssembly;
//            TemplatesAssembly = assembly;
//            foreach (var type in assembly.DefinedTypes)
//            {
//                InvertGraphEditor.Platform.MessageBox("loaded template", type.FullName, "ok");
                
//            }
//        }
//    }
//}