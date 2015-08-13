using System;
using System.Collections.Generic;
using Invert.IOC;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public interface IDesignerWindowEvents
    {
        void AfterDrawGraph(Rect diagramRect);

        void BeforeDrawGraph(Rect diagramRect);

        void DrawComplete();

        void ProcessInput();
    }

    public interface IProjectInspectorEvents
    {
        void DoInspector(IProjectRepository target);
    }
    public interface IDrawUFrameWindow
    {
        void Draw(float width, float height, Vector2 scrollPosition, float scale);
    }

    public class DesignerWindow : DiagramPlugin,
        IGraphWindow,
        IDrawUFrameWindow,
        ICommandExecuted
    {
        private DesignerViewModel _designerViewModel;

        private bool _drawToolbar = true;



        private WorkspaceService _workspaceService;


        public Toolbars Toolbars
        {
            get { return Container.Resolve<Toolbars>(); }
        }

        public IPlatformDrawer Drawer
        {
            get { return InvertGraphEditor.PlatformDrawer; }
        }

        public DesignerViewModel Designer
        {
            get
            {
                if (Workspace == null)
                    return null;
                if (_designerViewModel == null)
                {
                    _designerViewModel = new DesignerViewModel()
                    {
                        Data = Workspace
                    };
                }
                return _designerViewModel;
            }
            set { _designerViewModel = value; }
        }

        public DiagramDrawer DiagramDrawer { get; set; }

        public Rect DiagramRect { get; set; }

        public DiagramViewModel DiagramViewModel
        {
            get
            {
                if (DiagramDrawer == null) return null;
                return DiagramDrawer.DiagramViewModel;
            }
        }

        public bool DrawToolbar
        {
            get { return _drawToolbar; }
            set { _drawToolbar = value; }
        }



        public float Scale
        {
            get { return 1f; }
            set
            {
            }
        }

        public ToolbarUI Toolbar
        {
            get { return Toolbars.ToolbarUI; }
        }
        
        public Workspace Workspace
        {
            get { return WorkspaceService.CurrentWorkspace; }
            //set { InvertApplication.Execute();<IOpenWorkspace>(_=>_.OpenWorkspace(value)); }
        }

        public WorkspaceService WorkspaceService
        {
            get { return _workspaceService ?? (_workspaceService = InvertGraphEditor.Container.Resolve<WorkspaceService>()); }
            set { _workspaceService = value; }
        }

        public override void Loaded(UFrameContainer container)
        {
            base.Loaded(container);
            InvertGraphEditor.DesignerWindow = this;
        }

        public void Draw( float width, float height, Vector2 scrollPosition, float scale)
        {
            DiagramDrawer.IsEditingField = false;
            if (Drawer == null) return;
            Rect diagramRect = new Rect();

            if (DrawToolbar)
            {
                var toolbarTopRect = new Rect(0, 0, width, 18);
                var tabsRect = new Rect(0, toolbarTopRect.height, width, 31);
                var breadCrumbsRect = new Rect(0, tabsRect.y + tabsRect.height, width, 40);

                diagramRect = new Rect(0f, breadCrumbsRect.y + breadCrumbsRect.height, width,
                    height - ((toolbarTopRect.height * 2)) - breadCrumbsRect.height - 20 - 32);
                var toolbarBottomRect = new Rect(0f, diagramRect.y + diagramRect.height, width,
                    toolbarTopRect.height);

                Drawer.DrawStretchBox(toolbarTopRect, CachedStyles.Toolbar, 0f);
                Drawer.DoToolbar(toolbarTopRect, this, ToolbarPosition.Left);
                //drawer.DoToolbar(toolbarTopRect, this, ToolbarPosition.Right);
                Drawer.DoTabs(tabsRect, this); DiagramRect = diagramRect;
                if (DiagramDrawer != null)
                {
                    DiagramDrawer.DrawBreadcrumbs(Drawer, breadCrumbsRect.y);
                }

                DiagramRect = diagramRect;

                Drawer.DrawRect(diagramRect, InvertGraphEditor.Settings.BackgroundColor);
                DrawDiagram(Drawer, scrollPosition, scale, diagramRect);
                Drawer.DoToolbar(toolbarBottomRect, this, ToolbarPosition.BottomLeft);
                //drawer.DoToolbar(toolbarBottomRect, this, ToolbarPosition.BottomRight);
            }
            else
            {
                diagramRect = new Rect(0f, 0f, width, height);
                DiagramRect = diagramRect;
                DrawDiagram(Drawer, scrollPosition, scale, diagramRect);
            }

            
            InvertApplication.SignalEvent<IDesignerWindowEvents>(_ => _.DrawComplete());
        }

        public void LoadDiagram(IGraphData diagram)
        {
            InvertGraphEditor.DesignerWindow = this;
            if (diagram == null) return;
            try
            {
              
                DiagramDrawer = new DiagramDrawer(new DiagramViewModel(diagram));
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

        public void ProjectChanged(Workspace project)
        {
            _designerViewModel = null;

            _workspaceService = null;

            DiagramDrawer = null;

            if (project.CurrentGraph != null)
            {
                LoadDiagram(project.CurrentGraph);
            }
            else
            {
            }
        }

        public void RefreshContent()
        {
            LoadDiagram(Workspace.CurrentGraph);
            if (DiagramDrawer != null)
            {
                DiagramDrawer.Refresh(InvertGraphEditor.PlatformDrawer);
            }
        }

        public void SwitchDiagram(IGraphData data)
        {
            if (data != null)
            {
                Designer.OpenTab(data);
                LoadDiagram(Workspace.CurrentGraph);
            }
        }

        private bool DrawDiagram(IPlatformDrawer drawer, Vector2 scrollPosition, float scale, Rect diagramRect)
        {
            if (DiagramDrawer == null)
            {
                if (Workspace != null)
                {
                    if (Workspace.CurrentGraph != null)
                    {
                        LoadDiagram(Workspace.CurrentGraph);
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
                                new Vector2(x, diagramRect.x + diagramRect.height + scrollPosition.y + 85)
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

                InvertApplication.SignalEvent<IDesignerWindowEvents>(_ => _.BeforeDrawGraph(diagramRect));
                DiagramDrawer.Bounds = new Rect(0f, 0f, diagramRect.width, diagramRect.height);
                DiagramDrawer.Draw(drawer, 1f);
                InvertApplication.SignalEvent<IDesignerWindowEvents>(_ => _.ProcessInput());
                InvertApplication.SignalEvent<IDesignerWindowEvents>(_ => _.AfterDrawGraph(diagramRect));
            }
            return false;
        }

        public void CommandExecuted(ICommand command)
        {
            if (DiagramDrawer != null)
            {
                DiagramDrawer.Refresh(InvertGraphEditor.PlatformDrawer);
            }
            if (Workspace != null)
            {
                if (Workspace.CurrentGraph != null)
                {
                    Workspace.Save();
                }
            }
        }
    }
}