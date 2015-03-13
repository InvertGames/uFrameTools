using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public interface IDesignerWindowEvents
    {
        void ProcessInput();
        void BeforeDrawGraph(Rect diagramRect);
        void AfterDrawGraph(Rect diagramRect);
        void DrawComplete();
    }
    public class DesignerWindow : IGraphWindow
    {

        public string LastLoadedDiagram
        {
            get
            {
                return InvertGraphEditor.Prefs.GetString("LastLoadedDiagram", null);
            }
            set
            {
                InvertGraphEditor.Prefs.SetString("LastLoadedDiagram", value);
            }
        }
        public DesignerViewModel Designer
        {
            get
            {
                if (CurrentProject == null)
                    return null;
                if (_designerViewModel == null)
                {
                    _designerViewModel = new DesignerViewModel()
                    {
                        Data = CurrentProject
                    };
                }
                return _designerViewModel;
            }
        }

        private void SelectDiagram()
        {
            var contextMenu = InvertApplication.Container.Resolve<ContextMenuUI>();

            foreach (var item in CurrentProject.Graphs)
            {
                IGraphData item1 = item;

                contextMenu.AddCommand(new SimpleEditorCommand<DiagramViewModel>(_ =>
                {
                    CurrentProject.CurrentGraph = item1;
                    LoadDiagram(CurrentProject.CurrentGraph);
                }, item.Name));

            }
            contextMenu.AddSeparator("");
            foreach (var graphType in InvertGraphEditor.Container.Mappings.Where(p => p.From == typeof(IGraphData)))
            {
                TypeMapping type = graphType;
                contextMenu.AddCommand(new SimpleEditorCommand<DiagramViewModel>(_ =>
                {
                    var diagram = CurrentProject.CreateNewDiagram(type.To);
                    DiagramDrawer = null;
                    SwitchDiagram(diagram);
                }, "Create " + type.To.Name));
            }
            contextMenu.AddSeparator("");
            contextMenu.AddCommand(new SimpleEditorCommand<DiagramViewModel>(_ =>
            {
                CurrentProject.Refresh();
            }, "Force Refresh"));
            contextMenu.Go();
        }

        private void SelectProject()
        {
            var projectService = InvertGraphEditor.Container.Resolve<ProjectService>();
            var projects = projectService.Projects;
            var contextMenu = InvertApplication.Container.Resolve<ContextMenuUI>();

            //var menu = new GenericMenu();
            foreach (var project in projects)
            {
                IProjectRepository project1 = project;
                var command = new SimpleEditorCommand<DiagramViewModel>(_ =>
                {
                    CurrentProject = project1;
                    LoadDiagram(CurrentProject.CurrentGraph);
                }, project.Name);

                contextMenu.AddCommand(command);
                //menu.AddItem(new GUIContent(project.Name), project1 == CurrentProject, () =>
                //{
                //    CurrentProject = project1;
                //    LoadDiagram(CurrentProject.CurrentGraph);
                //});
            }

            contextMenu.AddSeparator("");
            contextMenu.AddCommand(new SimpleEditorCommand<DiagramViewModel>(_ =>
            {
                projectService.RefreshProjects();
            }, "Force Refresh"));
            contextMenu.Go();

        }

        public void LoadDiagram(IGraphData diagram)
        {
            InvertGraphEditor.DesignerWindow = this;
            if (diagram == null) return;
            try
            {
                ModifierKeyStates = new ModifierKeyState();
                MouseEvent = null;
                CurrentProject.CurrentGraph.SetProject(CurrentProject);

                //SerializedGraph = new SerializedObject(diagram as UnityEngine.Object);
                //Diagram = uFrameEditor.Container.Resolve<ElementsDiagram>();
                DiagramDrawer = new DiagramDrawer(new DiagramViewModel(diagram, CurrentProject));
               
                MouseEvent = new MouseEvent(ModifierKeyStates, DiagramDrawer);
                DiagramDrawer.Dirty = true;
                //DiagramDrawer.Data.ApplyFilter();
                DiagramDrawer.Refresh(InvertGraphEditor.PlatformDrawer);

            }
            catch (Exception ex)
            {
                
                InvertApplication.LogException(ex);
                InvertApplication.Log("Either a plugin isn't installed or the file could no longer be found. See Exception error");

            }
          

        }

        public ModifierKeyState ModifierKeyStates
        {
            get { return _modifierKeyStates ?? (_modifierKeyStates = new ModifierKeyState()); }
            set { _modifierKeyStates = value; }
        }

        private MouseEvent _event;
        private ModifierKeyState _modifierKeyStates;

        private ICommandUI _toolbar;
        private DesignerViewModel _designerViewModel;
        private bool _drawToolbar = true;
        private ProjectService _projectService;

        public MouseEvent MouseEvent
        {
            get { return _event ?? (_event = new MouseEvent(ModifierKeyStates, DiagramDrawer)); }
            set { _event = value; }
        }

        public DiagramDrawer DiagramDrawer { get; set; }

        public void ProjectChanged(IProjectRepository project)
        {
            
            _designerViewModel = null;
            
            _projectService = null;
            
            DiagramDrawer = null;

            if (project.CurrentGraph != null)
            {
                LoadDiagram(project.CurrentGraph);
                
            }
            else
            {
                
            }

        }

        public IProjectRepository CurrentProject
        {
            get { return ProjectService.CurrentProject; }
            set { ProjectService.CurrentProject = value; }
        }

        //public override void Initialize(uFrameContainer container)
        //{

        //}

        //public override void Loaded(uFrameContainer container)
        //{
        //    base.Loaded(container);
        //    //ProjectService = container.Resolve<ProjectService>();
        //    //InvertApplication.ListenFor<IProjectEvents>(this);

        //}

        public ProjectService ProjectService
        {
            get { return _projectService ?? (_projectService = InvertGraphEditor.Container.Resolve<ProjectService>()); }
            set { _projectService = value; }
        }

        public ICommandUI Toolbar
        {
            get
            {
                if (_toolbar != null) return _toolbar;

                return _toolbar = InvertGraphEditor.CreateCommandUI<ToolbarUI>(typeof(IToolbarCommand));
            }
            set { _toolbar = value; }
        }

        public bool DrawToolbar
        {
            get { return _drawToolbar; }
            set { _drawToolbar = value; }
        }

        public void Draw(IPlatformDrawer drawer, float width, float height, Vector2 scrollPosition, float scale)
        {
            if (drawer == null) return;
            Rect diagramRect = new Rect();

            if (DrawToolbar)
            {
                var toolbarTopRect = new Rect(0, 0, width, 18);
                var tabsRect = new Rect(0, toolbarTopRect.height, width, 31);

                diagramRect = new Rect(0f, tabsRect.y + tabsRect.height, width - 3,
                    height - ((toolbarTopRect.height * 2)) - tabsRect.height - 20);
                var toolbarBottomRect = new Rect(0f, diagramRect.y + diagramRect.height, width - 3,
                    toolbarTopRect.height);

                drawer.DrawStretchBox(toolbarTopRect, CachedStyles.Toolbar, 0f);
                drawer.DoToolbar(toolbarTopRect, this, ToolbarPosition.Left);
                drawer.DoToolbar(toolbarTopRect, this, ToolbarPosition.Right);
                drawer.DoTabs(tabsRect, this); DiagramRect = diagramRect;
                DiagramRect = diagramRect;
                DrawDiagram(drawer, scrollPosition, scale, diagramRect);
                drawer.DoToolbar(toolbarBottomRect, this, ToolbarPosition.BottomLeft);
                drawer.DoToolbar(toolbarBottomRect, this, ToolbarPosition.BottomRight);

            }
            else
            {
                diagramRect = new Rect(0f, 0f, width, height);
                DiagramRect = diagramRect;
                DrawDiagram(drawer, scrollPosition, scale, diagramRect);
            }



            ParentHandler.DrawComplete();
            InvertApplication.SignalEvent<IDesignerWindowEvents>(_ => _.DrawComplete());

        }

        public IDesignerWindowEvents ParentHandler { get; set; }
        private bool DrawDiagram(IPlatformDrawer drawer, Vector2 scrollPosition, float scale, Rect diagramRect)
        {
            if (DiagramDrawer == null)
            {
                if (CurrentProject != null)
                {
                    if (CurrentProject.CurrentGraph != null)
                    {
                        LoadDiagram(CurrentProject.CurrentGraph);
                    }
                }
            }


            if (DiagramDrawer != null && DiagramViewModel != null && InvertGraphEditor.Settings.UseGrid)
            {
                var softColor = InvertGraphEditor.Settings.GridLinesColor;
                var hardColor = InvertGraphEditor.Settings.GridLinesColorSecondary;
                var x = -scrollPosition.x;

                var every10 = 0;

                while (x < DiagramRect.x + DiagramRect.width + scrollPosition.x)
                {
                    var color = softColor;
                    if (every10 == 10)
                    {
                        color = hardColor;
                        every10 = 0;
                    }
                    if (x > diagramRect.x)
                    {
                        drawer.DrawPolyLine(
                            new[]
                            {
                                new Vector2(x, diagramRect.y),
                                new Vector2(x, diagramRect.x + diagramRect.height + scrollPosition.y + 50)
                            }, color);
                    }


                    x += DiagramViewModel.Settings.SnapSize * scale;
                    every10++;
                }
                var y = -scrollPosition.y;
                every10 = 0;
                while (y < DiagramRect.y + DiagramRect.height + scrollPosition.y)
                {
                    var color = softColor;
                    if (every10 == 10)
                    {
                        color = hardColor;
                        every10 = 0;
                    }
                    if (y > diagramRect.y)
                    {
                        drawer.DrawPolyLine(
                            new[]
                            {
                                new Vector2(diagramRect.x, y), new Vector2(diagramRect.x + diagramRect.width + scrollPosition.x, y)
                            }, color);
                    }


                    y += DiagramViewModel.Settings.SnapSize * scale;
                    every10++;
                }
            }
            if (DiagramDrawer != null)
            {
                ParentHandler.BeforeDrawGraph(DiagramRect);
                InvertApplication.SignalEvent<IDesignerWindowEvents>(_ => _.BeforeDrawGraph(DiagramRect));
                DiagramDrawer.Draw(drawer, 1f);
                ParentHandler.ProcessInput();
                InvertApplication.SignalEvent<IDesignerWindowEvents>(_ => _.ProcessInput());
                ParentHandler.AfterDrawGraph(diagramRect);
                InvertApplication.SignalEvent<IDesignerWindowEvents>(_ => _.AfterDrawGraph(DiagramRect));
            }
            return false;
        }

        public Rect DiagramRect { get; set; }


        public DiagramViewModel DiagramViewModel
        {
            get
            {
                if (DiagramDrawer == null) return null;
                return DiagramDrawer.DiagramViewModel;
            }
        }

        public float Scale
        {
            get { return 1f; }
            set
            {

            }
        }

        public void SwitchDiagram(IGraphData data)
        {

            Designer.OpenTab(data);
            LoadDiagram(CurrentProject.CurrentGraph);
        }
        public void RefreshContent()
        {
            if (DiagramDrawer != null)
            {
                DiagramDrawer.Refresh(InvertGraphEditor.PlatformDrawer);
            }
        }
        public IEnumerable<object> ContextObjects
        {
            get
            {
                yield return this;
                if (DiagramViewModel != null)
                {
                    foreach (var co in DiagramViewModel.ContextObjects)
                    {
                        yield return co;
                    }
                }
            }
        }

        public void CommandExecuted(IEditorCommand command)
        {
            if (DiagramDrawer != null)
            {
                DiagramDrawer.Refresh(InvertGraphEditor.PlatformDrawer);
            }
            if (CurrentProject != null)
            {
                if (CurrentProject.CurrentGraph != null)
                {
                    CurrentProject.Save();
                }
            }

        }

        public void CommandExecuting(IEditorCommand command)
        {
        }
    }

}