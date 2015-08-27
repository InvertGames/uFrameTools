using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Common;
using Invert.Core.GraphDesigner.Systems.GraphUI.api;
using Invert.Core.GraphDesigner.Systems.Wizards.api;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Systems.GraphUI
{
    public class WorkspacesUISystem : DiagramPlugin, 
        IQueryWorkspacesActions, 
        IQueryWorkspacesListItems, 
        IDrawWorkspacesList, 
        IQueryDesignerWindowModalContent,
        IToolbarQuery
    {
        private WorkspaceService _workspaceService;

        public WorkspaceService WorkspaceService
        {
            get { return _workspaceService ?? (_workspaceService = InvertApplication.Container.Resolve<WorkspaceService>()); }
            set { _workspaceService = value; }
        }

        private IPlatformDrawer _drawer;
        private Vector2 _scrollPos;

        public IPlatformDrawer Drawer
        {
            get { return _drawer ?? (_drawer = InvertApplication.Container.Resolve<IPlatformDrawer>()); }
            set { _drawer = value; }
        }

        public void QueryWorkspacesActions(List<ActionItem> items)
        {
            items.Add(new ActionItem()
            {
                Title = "Create Empty Workspace",
                Description = "Create an empty workspace without any graphs. Suitable, if you are going to start from scratch.",
                Command = new CreateEmptyWorkspaceCommand()
            });  
            
            items.Add(new ActionItem()
            {
                Title = "Create ECS Workspace",
                Description = "Create a workspace with Data and Behaviour graphs. Use this option for a quick start on ECS.",
                Command = new CreateECSWorkspaceCommand()
            });
        }

        public void QueryWorkspacesListItems(List<WorkspacesListItem> items)
        {
            items.AddRange(WorkspaceService.Workspaces.Select(workspace => new WorkspacesListItem()
            {
                Workspace = workspace
            }));
        }

        public void DrawWorkspacesList(IPlatformDrawer platform, Rect bounds, List<WorkspacesListItem> items)
        {
            platform.DrawStretchBox(bounds, CachedStyles.WizardSubBoxStyle, 13);

            var scrollBounds = bounds.Translate(15, 0).Pad(0, 0, 15, 0);
            bounds = bounds.PadSides(15);


            var headerRect = bounds.WithHeight(40);

            platform.DrawLabel(headerRect, "Workspaces", CachedStyles.WizardSubBoxTitleStyle, DrawingAlignment.TopCenter);

            var unpaddedItemRect = bounds.Below(headerRect).WithHeight(100);

            var workspaces = items.ToArray();
         
            var position = scrollBounds.Below(headerRect).Clip(scrollBounds).Pad(0, 0, 0, 55);
            var usedRect = position.Pad(0, 0, 15, 0).WithHeight((unpaddedItemRect.height + 1) * workspaces.Length);
            _scrollPos = GUI.BeginScrollView(position, _scrollPos, usedRect);
            foreach (var db in workspaces)
            {

                platform.DrawStretchBox(unpaddedItemRect, CachedStyles.WizardListItemBoxStyle, 2);
                var itemRect = unpaddedItemRect.PadSides(15);
                var titleRect = itemRect.WithHeight(40);

                platform.DrawLabel(titleRect, db.Workspace.Title, CachedStyles.WizardSubBoxTitleStyle, DrawingAlignment.TopLeft);

                var infoRect = itemRect.Below(titleRect).WithHeight(30);
                //(platform as UnityDrawer).DrawInfo(infoRect, string.Format("Namespace: {0}\nPath: {1}", db.GraphConfiguration.Namespace ?? "-", db.GraphConfiguration.FullPath));


                var openButton = new Rect().WithSize(80, 25).InnerAlignWithBottomRight(itemRect);
                var configButton = openButton.LeftOf(openButton).Translate(-2, 0);
                var deleteButton = configButton.LeftOf(configButton).Translate(-2, 0);

                platform.DoButton(openButton, "Open", ElementDesignerStyles.ButtonStyle, () => { /* OPEN WORKSPACE */});
                var db1 = db;
                platform.DoButton(configButton, "Config", ElementDesignerStyles.ButtonStyle,()=>InvokeConfigFor(db1));
                platform.DoButton(deleteButton, "Remove", ElementDesignerStyles.ButtonStyle, () => { /* REMOVE WORKSPACE */});
                //platform.DoButton(showButton, "Show In Explorer", ElementDesignerStyles.ButtonStyle, () => { });


                unpaddedItemRect = unpaddedItemRect.Below(unpaddedItemRect).Translate(0, 1);

            }

            GUI.EndScrollView(true);

        }

        private void InvokeConfigFor(WorkspacesListItem db)
        {
            SelectedAction = new ActionItem()
            {
                Command = new ConfigureWorkspaceCommand()
                {
                    Name = db.Workspace.Title
                },
                Description = "Configuration",
                Title = db.Workspace.Title,
                Verb = "Apply"
            };
        }

        public void DrawWorkspacesWizard( Rect bounds)
        {
            var actions = new List<ActionItem>();
            var items = new List<WorkspacesListItem>();
            var databasesActionsBounds = bounds.LeftHalf().TopHalf().PadSides(2);
            var databasesListBounds = bounds.RightHalf().PadSides(2);
            var databasesActionInspectorBounds = bounds.LeftHalf().BottomHalf().PadSides(2);
            var closeButtonBounds = new Rect().WithSize(80,30).InnerAlignWithBottomRight(databasesListBounds.PadSides(15));

            Signal<IQueryWorkspacesActions>(_ => _.QueryWorkspacesActions(actions));
            Signal<IQueryWorkspacesListItems>(_ => _.QueryWorkspacesListItems(items));
            Signal<IDrawActionsPanel>(_ => _.DrawActionsPanel(Drawer, databasesActionsBounds, actions, (a, m) => SelectedAction = a));
            Signal<IDrawActionDialog>(_ => _.DrawActionDialog(Drawer, databasesActionInspectorBounds, SelectedAction, () => SelectedAction = null));
            Signal<IDrawWorkspacesList>(_ => _.DrawWorkspacesList(Drawer, databasesListBounds, items));

            Drawer.DoButton(closeButtonBounds,"Close",ElementDesignerStyles.ButtonStyle,()=>EnableWizard = false);

        }

        public ActionItem SelectedAction { get; set; }

        public bool EnableWizard { get; set; }

        public void QueryDesignerWindowModalContent(List<DesignerWindowModalContent> content)
        {
            if (EnableWizard)
            content.Add(new DesignerWindowModalContent()
            {
                Drawer = DrawWorkspacesWizard,
                ZIndex = 1
            });
        }

        public void QueryToolbarCommands(ToolbarUI ui)
        {
            ui.AddCommand(new ToolbarItem()
            {
                Command = new LambdaCommand("Manage Workspaces", () => EnableWizard = true),
                Description = "Open workspace management dialog.",
                Position = ToolbarPosition.Left,
                Title = "Manage Workspaces"
            });
        }
    }

    public class ConfigureWorkspaceCommand : ICommand
    {
        public string Title { get; set; }

        [InspectorProperty]
        public string Name { get; set; }

    }

    public class CreateECSWorkspaceCommand : ICommand
    {
        [InspectorProperty("Name for the workspace")]
        public string Name { get; set; }

        [InspectorProperty("Prefix for both Data and Behaviour graphs")]
        public string GraphPrefix { get; set; }

        public string Title { get; set; }
    }

    public class CreateEmptyWorkspaceCommand : ICommand
    {
        public string Title { get; set; }

        [InspectorProperty("Name for the workspace")]
        public string Name { get; set; }

    }
}
