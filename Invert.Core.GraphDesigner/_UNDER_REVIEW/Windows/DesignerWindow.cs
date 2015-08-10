using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Invert.Core.GraphDesigner.Pro;
using Invert.Core.GraphDesigner;
using Invert.IOC;
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

    public interface IProjectInspectorEvents
    {
        void DoInspector(IProjectRepository target);
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

        public void LoadDiagram(IGraphData diagram)
        {
            InvertGraphEditor.DesignerWindow = this;
            if (diagram == null) return;
            try
            {
                ModifierKeyStates = new ModifierKeyState();
                MouseEvent = null;
                DiagramDrawer = new DiagramDrawer(new DiagramViewModel(diagram));
               
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

        private ToolbarUI _toolbar;
        private DesignerViewModel _designerViewModel;
        private bool _drawToolbar = true;
        private WorkspaceService _workspaceService;

        public MouseEvent MouseEvent
        {
            get { return _event ?? (_event = new MouseEvent(ModifierKeyStates, DiagramDrawer)); }
            set { _event = value; }
        }

        public DiagramDrawer DiagramDrawer { get; set; }

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

        public ToolbarUI Toolbar
        {
            get
            {
                if (_toolbar != null) return _toolbar;

                return (_toolbar = InvertApplication.Container.Resolve<ToolbarUI>());
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
        
            GraphDesigner.DiagramDrawer.IsEditingField = false;
            if (drawer == null) return;
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

                drawer.DrawStretchBox(toolbarTopRect, CachedStyles.Toolbar, 0f);
                drawer.DoToolbar(toolbarTopRect, this, ToolbarPosition.Left);
                //drawer.DoToolbar(toolbarTopRect, this, ToolbarPosition.Right);
                drawer.DoTabs(tabsRect, this); DiagramRect = diagramRect;
                if (DiagramDrawer != null)
                {
                    DiagramDrawer.DrawBreadcrumbs(drawer, breadCrumbsRect.y);
                }

                DiagramRect = diagramRect;
                
             
                drawer.DrawRect(diagramRect, InvertGraphEditor.Settings.BackgroundColor);
                DrawDiagram(drawer, scrollPosition, scale, diagramRect);
                drawer.DoToolbar(toolbarBottomRect, this, ToolbarPosition.BottomLeft);
                //drawer.DoToolbar(toolbarBottomRect, this, ToolbarPosition.BottomRight);

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
              
           
                ParentHandler.BeforeDrawGraph(DiagramRect);
                InvertApplication.SignalEvent<IDesignerWindowEvents>(_ => _.BeforeDrawGraph(DiagramRect));
                DiagramDrawer.Bounds = new Rect(0f,0f,diagramRect.width,diagramRect.height);
             
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
            
            if (data != null)
            {
                Designer.OpenTab(data);
                LoadDiagram(Workspace.CurrentGraph);
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
            if (Workspace != null)
            {
                if (Workspace.CurrentGraph != null)
                {
                    Workspace.Save();
                }
            }

        }

        public void CommandExecuting(IEditorCommand command)
        {
        }
    }

}