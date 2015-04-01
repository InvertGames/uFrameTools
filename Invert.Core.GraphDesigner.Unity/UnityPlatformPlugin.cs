using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PrettyPrinter;
using Invert.Core.GraphDesigner.Unity.Refactoring;
using Invert.Core.GraphDesigner.UnitySpecific;
using Invert.uFrame;
using Invert.uFrame.Editor;
using UnityEditor;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Unity
{
    public class Test : ElementsDiagramToolbarCommand
    {
        public override string Name
        {
            get { return "Test Command"; }
        }

        public override void Perform(DiagramViewModel node)
        {

            var selectedNode = node.SelectedNode.DataObject as IDiagramNode;

            var codeTypeMethod = new CodeMemberMethod()
            {
                Name = "HelloWorld",
                ReturnType = typeof(string).ToCodeReference()
            };
            var codeTypeMethod2 = new CodeMemberMethod()
            {
                Name = "HelloWorld2",
                ReturnType = typeof(string).ToCodeReference()
            };
            var text = CodeDomHelpers.GenerateCodeFromMembers(codeTypeMethod, codeTypeMethod2);
            var editableFileGenerator = selectedNode.GetAllEditableFilesForNode().FirstOrDefault();
            
            var parser =ParserFactory.CreateParser(SupportedLanguage.CSharp,
                new StringReader(File.ReadAllText(editableFileGenerator.FullPathName)));
            parser.Parse();

            var insertTextAtBottom = new InsertTextAtBottomRefactorer()
            {
                Text = text
            };
            parser.CompilationUnit.AcceptVisitor(insertTextAtBottom, null);
            var csharpOutput = new CSharpOutputVisitor();
            csharpOutput.BeforeNodeVisit += node1 => insertTextAtBottom.OutputNodeVisiting(node1, csharpOutput.OutputFormatter as CSharpOutputFormatter);
            csharpOutput.AfterNodeVisit += node1 => insertTextAtBottom.OutputNodeVisited(node1, csharpOutput.OutputFormatter as CSharpOutputFormatter);
            parser.CompilationUnit.AcceptVisitor(csharpOutput, null);
            InvertApplication.Log(csharpOutput.Text);

        }
    }
    public class UnityPlatformPlugin : DiagramPlugin, INodeItemEvents, IProjectEvents
    {
        public override decimal LoadPriority
        {
            get { return -95; }
        }

        public override bool Required
        {
            get { return true; }
        }

        static UnityPlatformPlugin()
        {
            InvertApplication.CachedAssemblies.Add(typeof(Vector3).Assembly);
            InvertGraphEditor.Prefs = new UnityPlatformPreferences();
            InvertApplication.Logger = new UnityPlatform();
            InvertGraphEditor.Platform = new UnityPlatform();
            InvertGraphEditor.PlatformDrawer = new UnityDrawer();
        }
        public override bool Enabled { get { return true; } set { } }
        public override void Initialize(uFrameContainer container)
        {
            Undo.undoRedoPerformed = delegate
            {
                container.Resolve<ProjectService>().RefreshProjects();
                InvertGraphEditor.DesignerWindow.RefreshContent();
            };
            InvertApplication.ListenFor<INodeItemEvents>(this);
            InvertApplication.ListenFor<IProjectEvents>(this);
            container.RegisterInstance<IPlatformDrawer>(InvertGraphEditor.PlatformDrawer);
            container.RegisterInstance<IStyleProvider>(new UnityStyleProvider());
#if DOCS
            container.RegisterToolbarCommand<GenerateDocsCommand>();
            container.RegisterToolbarCommand<DocsModeCommand>();
#endif
            container.RegisterInstance<IToolbarCommand>(new Test(), "Test");
            container.RegisterToolbarCommand<ExportDiagramCommand>();

            container.RegisterInstance<IAssetManager>(new UnityAssetManager());


            // Default Graph Item Drawers
            container.RegisterDrawer<EnumNodeViewModel, DiagramEnumDrawer>();
            container.RegisterDrawer<EnumItemViewModel, EnumItemDrawer>();
            container.RegisterDrawer<ClassPropertyItemViewModel, TypedItemDrawer>();
            container.RegisterDrawer<ClassCollectionItemViewModel, TypedItemDrawer>();
            container.RegisterDrawer<ClassNodeViewModel, ClassNodeDrawer>();


            // Command Drawers
            container.Register<ToolbarUI, UnityToolbar>();
            container.Register<ContextMenuUI, UnityContextMenu>();

            container.RegisterInstance<IGraphEditorSettings>(new UFrameSettings());
            // Where the generated code files are placed
            container.Register<ICodePathStrategy, DefaultCodePathStrategy>("Default");
            container.RegisterInstance<IToolbarCommand>(new DiagramSettingsCommand() { Title = "Settings" }, "SettingsCommand");
            container.RegisterInstance<IWindowManager>(new UnityWindowManager());


        }

        public override void Loaded(uFrameContainer container)
        {

        }


        public void Deleted(IDiagramNodeItem node)
        {
            
        }

        public void Hidden(IDiagramNodeItem node)
        {
      
        }

        public void Renamed(IDiagramNodeItem node, string previousName, string newName)
        {
            var n = node as DiagramNode;
            if (n == null) return;

            if (n == n.Graph.RootFilter)
            {
                var graph = n.Graph.Project.Graphs.FirstOrDefault(p=>p.Identifier == n.Graph.Identifier) as UnityEngine.Object;
                if (graph != null)
                {
                    graph.name = newName;
                    
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(graph), newName);
                    AssetDatabase.SaveAssets();
                    var openGraph = n.Graph.Project.OpenGraphs.FirstOrDefault(p => p.GraphIdentifier == n.Graph.Identifier);
                    if (openGraph != null)
                    openGraph.GraphName = newName;
                }
            }
        }

        public void ProjectLoaded(IProjectRepository project)
        {
            
        }

        public void ProjectUnloaded(IProjectRepository project)
        {
            
        }

        public void ProjectRemoved(IProjectRepository project)
        {
            
        }

        public void ProjectChanged(IProjectRepository project)
        {
            if (InvertGraphEditor.DesignerWindow != null)
            {
                InvertGraphEditor.DesignerWindow.ProjectChanged(project);
            }
        }

        public void ProjectsRefreshed(ProjectService service)
        {
            
        }
    }

    public class ExportDiagramCommand : ToolbarCommand<DiagramViewModel>
    {
        public override string Name
        {
            get { return "Export Graph"; }
        }

        public override void Perform(DiagramViewModel node)
        {
            var graph = node.GraphData as INodeRepository;
            var files = InvertGraphEditor.GetAllFileGenerators(null, graph, false).Select(p=>p.AssetPath).ToList();
            files.Add(AssetDatabase.GetAssetPath(graph as UnityEngine.Object));

            var path = EditorUtility.SaveFilePanelInProject("Export Graph Unity Package",
								graph.Name + ".unitypackage",
								"unitypackage",
								"Please enter a file name to export to.");
            if (path.Length != 0)
            {
                AssetDatabase.ExportPackage(files.Distinct().ToArray(), path, ExportPackageOptions.Default | ExportPackageOptions.Interactive);
            }
            
            
        }

        public override string CanPerform(DiagramViewModel node)
        {
            return null;
        }

        public override ToolbarPosition Position
        {
            get { return ToolbarPosition.BottomRight; }
        }
    }
    public class GenerateDocsCommand : ToolbarCommand<DiagramViewModel>
    {
        public override void Perform(DiagramViewModel node)
        {
            DocumentationWindow.ShowWindowAndGenerate();
            //var htmlOutput = new HtmlDocsBuilder();
            //htmlOutput.ScreenshotsRelativePath = "Screenshots";
            //htmlOutput.StyleSheet = "styles";
            //node.DiagramData.Document(htmlOutput);

            //File.WriteAllText(Path2.Combine("Documentation", "index.html"), htmlOutput.ToString());

        }

        public override string CanPerform(DiagramViewModel node)
        {
            if (!node.GraphData.DocumentationMode) return "You must be in documentation mode first";
            return null;
        }
        public override string Name
        {
            get { return "Generate Docs"; }
        }

        public override ToolbarPosition Position
        {
            get { return ToolbarPosition.BottomRight; }
        }
    }

    public class DocsModeCommand : ToolbarCommand<DiagramViewModel>
    {
        public override string Name
        {
            get { return "Documentation Mode"; }
        }

        public override ToolbarPosition Position
        {
            get { return ToolbarPosition.BottomRight; }
        }

        public override void Perform(DiagramViewModel node)
        {
            node.GraphData.DocumentationMode = !node.GraphData.DocumentationMode;
        }

        public override string CanPerform(DiagramViewModel node)
        {
            return null;
        }
    }

    //public class HtmlDocsBuilder : IDocumentationBuilder
    //{
    //    private StringBuilder _output;

    //    private StringBuilder Output
    //    {
    //        get { return _output ?? (_output = new StringBuilder()); }
    //        set { _output = value; }
    //    }
    //    public string ScreenshotsRelativePath { get; set; }
    //    public string StyleSheet { get; set; }
    //    public void BeginArea(string id)
    //    {
    //        Output.AppendFormat("<div class='{0}'>",id);

            
    //    }

    //    public void EndArea()
    //    {
    //        Output.AppendFormat("</div>");
    //    }
    //    public void BeginSection(string id)
    //    {
    //        Output.AppendFormat("<div id='{0}' class='{0}'>", id);
    //    }

    //    public void EndSection()
    //    {
    //        Output.AppendFormat("</div>");
    //    }
    //    public void PushIndent()
    //    {
    //        Output.AppendFormat("<div style='margin-left: 10px;'>");
    //    }
    //    public void PopIndent()
    //    {
    //        Output.AppendFormat("</div>");
    //    }
    //    public void LinkToNode(IDiagramNodeItem node, string text = null)
    //    {
    //        Output.AppendFormat("<a href='#{0}'>{1}</a>", node.Name, text ?? node.Name);
    //    }

    //    public void Columns(params Action[] actions)
    //    {
    //        foreach (var item in actions)
    //        {
    //            Output.AppendFormat("<div style='float: left'>");
    //            item();
    //            Output.AppendFormat("</div>");
    //        }
    //    }

    //    public void YouTubeLink(string id)
    //    {
            
    //    }

    //    public void TemplateExample<TTemplate, TData>(TData data, string templateMember = null) where TTemplate : IClassTemplate<TData>
    //    {
    //    }

    //    public void ShowGist(string id, string filename, string userId = "micahosborne")
    //    {
         
    //    }

    //    public void ShowTutorialStep(ITutorialStep step)
    //    {
            
    //    }

    //    public void BeginTutorial(string walkthrough)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Rows(params Action[] actions)
    //    {
    //        foreach (var item in actions)
    //        {
    //            Output.AppendFormat("<div style='clear: both;'>");
    //            item();
    //            Output.AppendFormat("</div>");
    //        }
    //    }
    //    public void NodeImage(DiagramNode node)
    //    {
    //        Output.AppendFormat("<img src='{0}' />", string.Format(Path.Combine(ScreenshotsRelativePath, node.Name + ".png")));
    //    }

    //    public void Paragraph(string text, params object[] args)
    //    {
    //        if (string.IsNullOrEmpty(text)) return;
    //        Output.AppendFormat("<p>{0}</p>", string.Format(text, args));
    //    }

    //    public void Lines(params string[] lines)
    //    {
    //        foreach (var item in lines)
    //        {
                
    //        }
    //    }

    //    public void Section(string text, params object[] args)
    //    {
    //        if (string.IsNullOrEmpty(text)) return;
    //        Output.AppendFormat("<h1 id='{0}'>{1}</h1>", string.Format(text, args).Replace(" ", ""), string.Format(text, args));
    //    }

    //    public void Title(string text, params object[] args)
    //    {
    //        if (string.IsNullOrEmpty(text)) return;
    //        Output.AppendFormat("<h1 class='title'>{0}</h1>", string.Format(text, args));
    //    }

    //    public void Title2(string text, params object[] args)
    //    {
    //        if (string.IsNullOrEmpty(text)) return;
    //        Output.AppendFormat("<h2 class='title2'>{0}</h2>", string.Format(text, args));
    //    }

    //    public void Title3(string text, params object[] args)
    //    {
    //        if (string.IsNullOrEmpty(text)) return;
    //        Output.AppendFormat("<h3 class='title3'>{0}</h3>", string.Format(text, args));
    //    }

    //    public void Note(string text, params object[] args)
    //    {
    //        if (string.IsNullOrEmpty(text)) return;
    //        Output.AppendFormat("<div class='note'>{0}</div>", string.Format(text, args));
    //    }

    //    public void TemplateLink()
    //    {

    //    }

    //    public void Literal(string text, params object[] args)
    //    {
    //        Output.AppendFormat(text, args);
    //    }

    //    public override string ToString()
    //    {
    //        var finalOutput = new StringBuilder();
    //        finalOutput.AppendLine("<html>");
    //        finalOutput.AppendLine("<head>");
    //        if (this.StyleSheet != null)
    //            finalOutput.AppendLine(string.Format("<link rel='stylesheet' type='text/css' href='{0}.css'>", this.StyleSheet));

    //        finalOutput.AppendLine("</head>");
    //        finalOutput.AppendLine("<body>");
    //        finalOutput.Append(Output.ToString());
    //        finalOutput.AppendLine("</body>");
    //        finalOutput.AppendLine("</html>");
    //        return finalOutput.ToString();
    //    }
    //}
    //public class DocsGenerator
    //{
    //    public INodeRepository NodeRepository { get; set; }

    //    public override string ToString()
    //    {
    //        var sb = new StringBuilder();
    //        sb.AppendFormat("<h2>{0}</h2>", NodeRepository.Name);
    //        foreach (var nodeConfig in NodeRepository.AllGraphItems.OfType<DiagramNode>())
    //        {
    //            sb.AppendFormat("<h3>{0}</h3>", nodeConfig.Name);
    //            sb.AppendFormat("<img src='{0}' />",
    //                Path2.Combine("Screenshots", nodeConfig.Name + ".png"));
    //        }
    //        return sb.ToString();
    //    }
    //}

}
