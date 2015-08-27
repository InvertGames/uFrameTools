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
        IToolbarQuery,
        IContextMenuQuery
    {
        private WorkspaceService _workspaceService;

        public WorkspaceService WorkspaceService
        {
            get { return _workspaceService ?? (_workspaceService = InvertApplication.Container.Resolve<WorkspaceService>()); }
            set { _workspaceService = value; }
        }

        private IPlatformDrawer _drawer;

        public IPlatformDrawer Drawer
        {
            get { return _drawer ?? (_drawer = InvertApplication.Container.Resolve<IPlatformDrawer>()); }
            set { _drawer = value; }
        }

        public void QueryWorkspacesActions(List<ActionItem> items)
        {
            foreach (var item in WorkspaceService.Configurations)
            {
                items.Add(new ActionItem()
                {
                    Title = string.Format("Create {0} Workspace", item.Value.Title),
                    Description = item.Value.Description,
                    Command = new CreateWorkspaceCommand()
                    {
                        Name = string.Format("New{0}Workspace", item.Value.Title),
                        WorkspaceType = item.Key
                    }
                });

              
            }
           
           
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
            bounds = bounds.PadSides(15);


            var headerRect = bounds.WithHeight(40);

            platform.DrawLabel(headerRect, "Workspaces", CachedStyles.WizardSubBoxTitleStyle, DrawingAlignment.TopCenter);

            var unpaddedItemRect = bounds.Below(headerRect).WithHeight(100);

            foreach (var db in items)
            {
                var workspace = db;
                platform.DrawStretchBox(unpaddedItemRect, CachedStyles.WizardListItemBoxStyle, 2);
                var itemRect = unpaddedItemRect.PadSides(15);
                var titleRect = itemRect.WithHeight(40);

                platform.DrawLabel(titleRect, db.Workspace.Title, CachedStyles.WizardSubBoxTitleStyle, DrawingAlignment.TopLeft);

                var infoRect = itemRect.Below(titleRect).WithHeight(30);
                //(platform as UnityDrawer).DrawInfo(infoRect, string.Format("Namespace: {0}\nPath: {1}", db.GraphConfiguration.Namespace ?? "-", db.GraphConfiguration.FullPath));


                var openButton = new Rect().WithSize(80, 25).InnerAlignWithBottomRight(itemRect);
                var configButton = openButton.LeftOf(openButton).Translate(-2, 0);
                var deleteButton = configButton.LeftOf(configButton).Translate(-2, 0);

                platform.DoButton(openButton, "Open", ElementDesignerStyles.ButtonStyle, () =>
                {
                    Execute(new OpenWorkspaceCommand() { Workspace = workspace.Workspace });
                    EnableWizard = false;
                });
                platform.DoButton(configButton, "Config", ElementDesignerStyles.ButtonStyle, () => { /* CONFIG WORKSPACE */ });
                platform.DoButton(deleteButton, "Remove", ElementDesignerStyles.ButtonStyle, () => { Execute(new RemoveWorkspaceCommand() { Workspace = workspace.Workspace }); });
                //platform.DoButton(showButton, "Show In Explorer", ElementDesignerStyles.ButtonStyle, () => { });


                unpaddedItemRect = unpaddedItemRect.Below(unpaddedItemRect).Translate(0, 1);

            }
        }

        public void DrawWorkspacesWizard(Rect bounds)
        {
            var actions = new List<ActionItem>();
            var items = new List<WorkspacesListItem>();
            var databasesActionsBounds = bounds.LeftHalf().TopHalf().PadSides(2);
            var databasesListBounds = bounds.RightHalf().PadSides(2);
            var databasesActionInspectorBounds = bounds.LeftHalf().BottomHalf().PadSides(2);
            var closeButtonBounds = new Rect().WithSize(80, 30).InnerAlignWithBottomRight(databasesListBounds.PadSides(15));

            Signal<IQueryWorkspacesActions>(_ => _.QueryWorkspacesActions(actions));
            Signal<IQueryWorkspacesListItems>(_ => _.QueryWorkspacesListItems(items));
            Signal<IDrawActionsPanel>(_ => _.DrawActionsPanel(Drawer, databasesActionsBounds, actions, (a, m) => SelectedAction = a));
            Signal<IDrawActionDialog>(_ => _.DrawActionDialog(Drawer, databasesActionInspectorBounds, SelectedAction, () => SelectedAction = null));
            Signal<IDrawWorkspacesList>(_ => _.DrawWorkspacesList(Drawer, databasesListBounds, items));

            Drawer.DoButton(closeButtonBounds, "Close", ElementDesignerStyles.ButtonStyle, () => EnableWizard = false);

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

            if (WorkspaceService.Workspaces.Count() < 8)
            {
           
                foreach (var item in WorkspaceService.Workspaces)
                {
                    ui.AddCommand(new ToolbarItem()
                    {
                        Title = item.Name,
                        Checked = item == WorkspaceService.CurrentWorkspace,
                        Command = new OpenWorkspaceCommand()
                        {
                            Workspace = item
                        }
                    });
                }
                ui.AddCommand(new ToolbarItem()
                {
                    Command = new LambdaCommand("Workspaces", () => EnableWizard = true),
                    Title = "..."
                });
            }
            else
            {
                ui.AddCommand(new ToolbarItem()
                {
                    Title = WorkspaceService.CurrentWorkspace == null ? "--Choose Workspace--" : WorkspaceService.CurrentWorkspace.Name,
                    IsDropdown = true,
                    Command = new SelectWorkspaceCommand(),
                    Position = ToolbarPosition.Left
                });
            }
        }

        public void QueryContextMenu(ContextMenuUI ui, MouseEvent evt, object obj)
        {
            if (WorkspaceService.Workspaces.Count() >= 8)
            {
                var selectProject = obj as SelectWorkspaceCommand;
                if (selectProject != null)
                {
                    foreach (var item in WorkspaceService.Workspaces)
                    {
                        ui.AddCommand(new ContextMenuItem()
                        {
                            Title = item.Name,
                            Checked = item == WorkspaceService.CurrentWorkspace,
                            Command = new OpenWorkspaceCommand()
                            {
                                Workspace = item
                            }
                        });
                    }
                    //if (WorkspaceService.Configurations != null)
                    //{
                    //    ui.AddSeparator();
                    //    foreach (var item in WorkspaceService.Configurations)
                    //    {
                    //        var title = item.Value.Title ?? item.Key.Name;
                    //        ui.AddCommand(new ContextMenuItem()
                    //        {
                    //            Title = string.Format("Create New {0} Workspace", title),
                    //            Command = new CreateWorkspaceCommand()
                    //            {
                    //                Name = string.Format("New {0} Workspace", title),
                    //                Title = string.Format("New {0} Workspace", title),
                    //                WorkspaceType = item.Key,
                    //            }
                    //        });
                    //    }
                    //}
                    ui.AddSeparator();
                    ui.AddCommand(new ContextMenuItem()
                    {
                        Command = new LambdaCommand("Manage Workspaces", () => EnableWizard = true),
                        Title = "Manage"
                    });



                }
            }
           
        }
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
