using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Invert.Core.GraphDesigner.UnitySpecific;
using Invert.Data;
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
            var projectService = InvertGraphEditor.Container.Resolve<WorkspaceService>();
            var currentProject = projectService.CurrentWorkspace as Workspace;
            // TODO 2.0 Asset Paths
            //Debug.Log("Asset Directory  : " + currentProject.AssetDirectory);
            //Debug.Log("Asset Path       : " + currentProject.AssetPath);
            //Debug.Log("System Directory : " + currentProject.SystemDirectory);
            //Debug.Log("System Path      : " + currentProject.SystemPath);

            //Debug.Log("Asset Directory  : " + currentProject.CurrentGraph.AssetDirectory);
            //Debug.Log("Asset Path       : " + currentProject.CurrentGraph.AssetPath);
            //Debug.Log("System Directory : " + currentProject.CurrentGraph.SystemDirectory);
            //Debug.Log("System Path      : " + currentProject.CurrentGraph.SystemPath);

            //var fileGenerators = InvertGraphEditor.GetAllFileGenerators(null, currentProject);
            //foreach (var fileGenerator in fileGenerators)
            //{
            //    Debug.Log(string.Format("FG SystemPath: {0}", fileGenerator.SystemPath));
            //    Debug.Log(string.Format("FG Asset Path: {0}", fileGenerator.AssetPath));
            //}
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

    public class UnityPlatformPlugin : DiagramPlugin, IAssetDeleted, ITaskHandler, IWorkspaceChanged
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
            // TODO 2.0: Obviously fix undo
            //Undo.undoRedoPerformed = delegate
            //{
            //    var ps = container.Resolve<WorkspaceService>();
           
            //    ps.RefreshProjects();
            //    InvertGraphEditor.DesignerWindow.RefreshContent();
            //};
            container.RegisterInstance<IPlatformDrawer>(InvertGraphEditor.PlatformDrawer);
            container.RegisterInstance<IStyleProvider>(new UnityStyleProvider());
#if DOCS
            container.RegisterToolbarCommand<GenerateDocsCommand>();
            container.RegisterToolbarCommand<DocsModeCommand>();
#endif
           // container.RegisterInstance<IToolbarCommand>(new Test(), "Test");
            container.RegisterToolbarCommand<ExportDiagramCommand>();

            container.RegisterInstance<IAssetManager>(new UnityAssetManager());

            // Command Drawers
            container.RegisterInstance<ToolbarUI>(new UnityToolbar()
            {
                
            });
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




        public void AssetDeleted(string filename)
        {
            // TODO 2.0 This is no longer valid
        }

        public void WorkspaceChanged(Workspace workspace)
        {
            if (InvertGraphEditor.DesignerWindow != null)
            {
                InvertGraphEditor.DesignerWindow.ProjectChanged(workspace);
            }
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
            // TODO 2.0 Rewrite Export Diagram
            //var graph = node.GraphData as INodeRepository;
            //var files = InvertGraphEditor.GetAllFileGenerators(null, node.GraphData.Repository, false).Select(p => p.AssetPath).ToList();
            //files.Add();

            //var path = EditorUtility.SaveFilePanelInProject("Export Graph Unity Package",
            //                    graph.Name + ".unitypackage",
            //                    "unitypackage",
            //                    "Please enter a file name to export to.");
            //if (path.Length != 0)
            //{
            //    AssetDatabase.ExportPackage(files.Distinct().ToArray(), path, ExportPackageOptions.Default | ExportPackageOptions.Interactive);
            //}


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


    

}
