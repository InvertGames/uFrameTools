﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner.UnitySpecific;
using Invert.IOC;
using Invert.Windows;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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
            var projectService = InvertGraphEditor.Container.Resolve<ProjectService>();
            var currentProject = projectService.CurrentProject as ProjectRepository;

            Debug.Log("Asset Directory  : " + currentProject.AssetDirectory);
            Debug.Log("Asset Path       : " + currentProject.AssetPath);
            Debug.Log("System Directory : " + currentProject.SystemDirectory);
            Debug.Log("System Path      : " + currentProject.SystemPath);

            Debug.Log("Asset Directory  : " + currentProject.CurrentGraph.AssetDirectory);
            Debug.Log("Asset Path       : " + currentProject.CurrentGraph.AssetPath);
            Debug.Log("System Directory : " + currentProject.CurrentGraph.SystemDirectory);
            Debug.Log("System Path      : " + currentProject.CurrentGraph.SystemPath);

            var fileGenerators = InvertGraphEditor.GetAllFileGenerators(null, currentProject);
            foreach (var fileGenerator in fileGenerators)
            {
                Debug.Log(string.Format("FG SystemPath: {0}", fileGenerator.SystemPath));
                Debug.Log(string.Format("FG Asset Path: {0}", fileGenerator.AssetPath));
            }
            return;
            //var projectService = InvertGraphEditor.Container.Resolve<ProjectService>();
            //var currentProject = projectService.CurrentProject as ProjectRepository;

            //List<string> allIds = new List<string>();
            //string[] legitimateIds = currentProject.AllGraphItems.Select(p => p.Identifier).ToArray();

            //foreach (var d in currentProject.Diagrams.OfType<UnityGraphData>())
            //{
            //    allIds.AddRange(d.PositionData.Positions.Keys);
            //}
            //foreach (var d in currentProject.AllGraphItems.OfType<IDiagramFilter>())
            //{
            //    allIds.AddRange(d.CollapsedValues.Keys);
            //    allIds.AddRange(d.Locations.Keys);
            //}

            //var tempAllIds = allIds.ToArray();
            //foreach (var item in tempAllIds)
            //{
            //    {
            //        if (legitimateIds.Contains(item))
            //        {
            //            allIds.Remove(item);
            //        }
            //    }
            //}
            //Debug.Log(string.Format("There are {0} bad ids", allIds.Count));
            //foreach (var d in currentProject.Diagrams.OfType<UnityGraphData>())
            //{
            //    foreach (var item in allIds)
            //    {
            //        d.PositionData.Positions.Remove(item);
            //    }
              

            //}
            //foreach (var d in currentProject.AllGraphItems.OfType<IDiagramFilter>())
            //{
            //    d.CollapsedValues.Keys.Clear();
            //    d.CollapsedValues.Values.Clear();
            //    //foreach (var item in allIds)
            //    //{
            //    //    try
            //    //    {
            //    //        d.CollapsedValues.Remove(item);

            //    //    }
            //    //    catch (Exception ex)
            //    //    {
                        
            //    //    }
                    
            //    //}
            //    foreach (var item in allIds)
            //    {
            //        try
            //        {
            //            d.Locations.Remove(item);
                        
            //        }
            //        catch (Exception ex)
            //        {

            //        }
                 
            //    }
               
            //}
            //foreach (var d in currentProject.Diagrams.OfType<UnityGraphData>())
            //{
            //    EditorUtility.SetDirty(d);


            //}
            //AssetDatabase.SaveAssets();

        }
    }

    public class CompilationProgress : DiagramPlugin, IDesignerWindowEvents, ITaskProgressHandler
    {
        public override bool Required
        {
            get { return true; }
        }

        public override void Initialize(UFrameContainer container)
        {
            ListenFor<IDesignerWindowEvents>();
            ListenFor<ITaskProgressHandler>();
            
        }

        public void ProcessInput()
        {
           
        }

        public void BeforeDrawGraph(Rect diagramRect)
        {
            
        }

        public void AfterDrawGraph(Rect diagramRect)
        {
            if (Percentage > 1.0f)
            {
                Percentage = 1.0f;
            }
            if (Percentage > 0.0f && Percentage < 1.0f)
            {
                var drawer = InvertGraphEditor.PlatformDrawer;
                var width = 400f;
                var height = 75f;
                var boxRect = new Rect((diagramRect.width/2f) - (width/2f), (diagramRect.height/2f) - (height/2f), width,
                    height);
                var progressRect = new Rect(boxRect);
                progressRect.y += (boxRect.height - 35f);

                progressRect.height = 7f;
                progressRect.width = boxRect.width*0.8f;
                progressRect.x = (diagramRect.width/2f) - (progressRect.width/2f);

                var progressFill = new Rect(progressRect);
                progressFill.width = (progressRect.width/100f)*(Percentage*100f);
                progressFill.x += 1;
                progressFill.y += 1;
                progressFill.height -= 2f;

                drawer.DrawRect(diagramRect, new Color(0.1f, 0.1f, 0.1f, 0.8f));
                drawer.DoButton(new Rect(0f, 0f, Screen.width, Screen.height), " ", CachedStyles.ClearItemStyle,
                    () => { });
              //  drawer.DrawStretchBox(boxRect, CachedStyles.NodeBackground, 12f);
                drawer.DrawStretchBox(boxRect, CachedStyles.NodeBackground, 12f);
                //drawer.DrawStretchBox(boxRect,CachedStyles.NodeBackground,12f);
                boxRect.x += 15f;
                boxRect.y += 15f;
                boxRect.width -= 30f;
                drawer.DrawLabel(boxRect, string.Format("{0}", Message), CachedStyles.ViewModelHeaderStyle,
                    DrawingAlignment.MiddleCenter);
                drawer.DrawRect(progressRect, Color.black);
                drawer.DrawRect(progressFill, Color.blue);
            }
            //}
            //else
            //{
            //    Percentage = 0f;
            //}
           

        }

        public void DrawComplete()
        {
            
        }
        public string Message { get; set; }
        public float Percentage { get; set; }

        public void Progress(float progress, string message)
        {
            Message = message;
            Percentage = progress / 100f;
           
        }

 
    }
    public class UnityPlatformPlugin : DiagramPlugin, INodeItemEvents, IProjectEvents, IAssetDeleted, ITaskHandler
    {
        public override decimal LoadPriority
        {
            get { return -95; }
        }

        public override bool Required
        {
            get { return true; }
        }
        public void BeginTask(IEnumerator task)
        {
            ElementsDesigner.Instance.Task = task;
        }
        static UnityPlatformPlugin()
        {
            InvertApplication.CachedAssemblies.Add(typeof(Vector3).Assembly);
            InvertApplication.TypeAssemblies.Add(typeof(Vector3).Assembly);
            InvertGraphEditor.Prefs = new UnityPlatformPreferences();
            InvertApplication.Logger = new UnityPlatform();
            InvertGraphEditor.Platform = new UnityPlatform();
            InvertGraphEditor.PlatformDrawer = new UnityDrawer();
        }
        public override bool Enabled { get { return true; } set { } }
        public override void Initialize(UFrameContainer container)
        {
            EditorUtility.ClearProgressBar();
            Undo.undoRedoPerformed = delegate
            {
                var ps = container.Resolve<ProjectService>();
           
                ps.RefreshProjects();
                InvertGraphEditor.DesignerWindow.RefreshContent();
            };
            container.RegisterInstance<IPlatformDrawer>(InvertGraphEditor.PlatformDrawer);
            container.RegisterInstance<IStyleProvider>(new UnityStyleProvider());
#if DOCS
            container.RegisterToolbarCommand<GenerateDocsCommand>();
            container.RegisterToolbarCommand<DocsModeCommand>();
#endif
           // container.RegisterInstance<IToolbarCommand>(new Test(), "Test");
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

        public override void Loaded(UFrameContainer container)
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
                var graph = n.Graph.Project.Graphs.FirstOrDefault(p => p.Identifier == n.Graph.Identifier) as Object;
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

        public void AssetDeleted(string filename)
        {

            if (!filename.ToLower().EndsWith(".asset")) return;
            var ps = InvertApplication.Container.Resolve<ProjectService>();
            var graphs = ps.Projects.SelectMany(p => p.Graphs.Select(x => x.Identifier));

            foreach (var project in ps.Projects)
            {
                var close = project.OpenGraphs.Where(p=>!graphs.Contains(p.GraphIdentifier)).ToArray();
                foreach (var item in close)
                {
                    project.CloseGraph(item);
                }
                
            }

            //var ps = InvertApplication.Container.Resolve<ProjectService>();
            //foreach (var project in ps.Projects)
            //{
            //    foreach (var g in project.Graphs)
            //        Debug.Log(g.Path);
            //    var graph = project.Graphs.FirstOrDefault(p => p.Path == filename);
            //    if (graph == null)
            //    {
                    
            //        continue;
            //    }
            //    InvertApplication.Log("Graph found.");
            //    var openGraph = project.OpenGraphs.First(p => p.GraphIdentifier == graph.Identifier);
            //    if (openGraph != null)
            //    {
            //        project.CloseGraph(openGraph);
            //    }
            //}

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
            var files = InvertGraphEditor.GetAllFileGenerators(null, graph, false).Select(p => p.AssetPath).ToList();
            files.Add(AssetDatabase.GetAssetPath(graph as Object));

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

    

}