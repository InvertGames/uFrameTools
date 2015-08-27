using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Invert.Common;
using Invert.Core.GraphDesigner.Systems.GraphUI.api;
using Invert.Core.GraphDesigner.Unity;
using Invert.Core.GraphDesigner.Unity.InspectorWindow;
using Invert.Core.GraphDesigner.Unity.Wizards;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Systems.GraphUI
{

    public class DatabasesUISystem : DiagramPlugin, 
        IQueryDesignerWindowModalContent,
        IDrawDatabasesList, 
        IQueryDatabasesActions, 
        IQueryDatabasesListItems,
        IToolbarQuery
    {

        private DatabaseService _databaseService;
        private IPlatformDrawer _drawer;
        private Vector2 _scrollPos;

        public DatabaseService DatabaseService
        {
            get { return _databaseService ?? (_databaseService = InvertApplication.Container.Resolve<DatabaseService>()); }
            set { _databaseService = value; }
        }

        public IPlatformDrawer Drawer
        {
            get { return _drawer ?? (_drawer = InvertApplication.Container.Resolve<IPlatformDrawer>()); }
            set { _drawer = value; }
        }

        public ActionItem SelectedItem { get; set; }

        #region Drawing

        public void DrawDatabasesWizard(Rect bounds)
        {

           // platform.DrawStretchBox(bounds, CachedStyles.WizardBoxStyle, 13);

            var actions = new List<ActionItem>();
            var items = new List<DatabasesListItem>();
            var databasesActionsBounds = bounds.LeftHalf().TopHalf().PadSides(2);
            var databasesListBounds = bounds.RightHalf().PadSides(2);
            var databasesActionInspectorBounds = bounds.LeftHalf().BottomHalf().PadSides(2);
            var closeButtonBounds = new Rect().WithSize(80, 30).InnerAlignWithBottomRight(databasesListBounds.PadSides(15));

            Signal<IQueryDatabasesActions>(_ => _.QueryDatabasesActions(actions));
            Signal<IQueryDatabasesListItems>(_ => _.QueryDatabasesListItems(items));
            Signal<IDrawActionsPanel>(_ => _.DrawActionsPanel(Drawer, databasesActionsBounds, actions,(a,m)=> SelectedItem = a));
            Signal<IDrawActionDialog>(_ => _.DrawActionDialog(Drawer, databasesActionInspectorBounds,SelectedItem,()=> SelectedItem = null));
            Signal<IDrawDatabasesList>(_ => _.DrawDatabasesList(Drawer, databasesListBounds, items));

            Drawer.DoButton(closeButtonBounds, "Close", ElementDesignerStyles.ButtonStyle, () => EnableWizard = false);
        }

        public void DrawDatabasesList(IPlatformDrawer Drawer, Rect bounds, List<DatabasesListItem> items)
        {

            Drawer.DrawStretchBox(bounds, CachedStyles.WizardSubBoxStyle, 13);
            
            var scrollBounds = bounds.Translate(15,0).Pad(0,0,15,0);
            
            bounds = bounds.PadSides(15);


            var headerRect = bounds.WithHeight(40);
             
            Drawer.DrawLabel(headerRect, "Databases", CachedStyles.WizardSubBoxTitleStyle, DrawingAlignment.TopCenter);

            var unpaddedItemRect = bounds.Below(headerRect).WithHeight(150);

            var databasesListItems = items.ToArray();
         
            var position = scrollBounds.Below(headerRect).Clip(scrollBounds).Pad(0, 0, 0, 55);
            var usedRect = position.Pad(0, 0, 15, 0).WithHeight((unpaddedItemRect.height + 1)*databasesListItems.Length);
            
            _scrollPos = GUI.BeginScrollView(position, _scrollPos, usedRect);

            foreach (var db in databasesListItems)
            {

                Drawer.DrawStretchBox(unpaddedItemRect,CachedStyles.WizardListItemBoxStyle,2);
                var itemRect = unpaddedItemRect.PadSides(15);
                var titleRect = itemRect.WithHeight(40);

                Drawer.DrawLabel(titleRect,db.GraphConfiguration.Title,CachedStyles.WizardSubBoxTitleStyle,DrawingAlignment.TopLeft);

                var infoRect = itemRect.Below(titleRect).WithHeight(30);
                (Drawer as UnityDrawer).DrawInfo(infoRect,string.Format("Namespace: {0}\nPath: {1}",db.GraphConfiguration.Namespace ?? "-",db.GraphConfiguration.FullPath));


                var openButton = new Rect().WithSize(80,25).InnerAlignWithBottomRight(itemRect);
                var configButton = openButton.LeftOf(openButton).Translate(-2,0);
                var showButton = configButton.WithWidth(120).InnerAlignWithBottomLeft(itemRect);

                Drawer.DoButton(openButton,"Open",ElementDesignerStyles.ButtonStyle, () =>
                {
                    /* OPEN DATABASE */

                    Signal<INotify>(_=>_.Notify("Hello, World!",NotificationIcon.Info));

                    //DatabaseListWindow.Init(new Vector2(Screen.currentResolution.width / 2 - 200, Screen.currentResolution.height/2- 300));

                });
                Drawer.DoButton(configButton, "Config", ElementDesignerStyles.ButtonStyle, () => { /* CONFIG DATABASE */ });
                Drawer.DoButton(showButton, "Show In Explorer", ElementDesignerStyles.ButtonStyle, () => { /* SHOW DATABASE IN EXPLORER */ });

                unpaddedItemRect = unpaddedItemRect.Below(unpaddedItemRect).Translate(0,1);

            }
            GUI.EndScrollView(true);
        }

        #endregion

        #region Quering

        public void QueryDatabasesActions(List<ActionItem> items)
        {
            items.Add(new ActionItem()
            {
                Command = new CreateDatabaseCommand(),
                Title = "New Empty Database",
                Description = "Create a completely empty database. Suitable, if you are going to start from scratch. Do not forget to specify code path and the namespace. Changing namespace later will require refactoring of the existing code!",
                Icon = "CreateEmptyDatabaseIcon"
            });         
            
        }

        public void QueryDatabasesListItems(List<DatabasesListItem> items)
        {
            foreach (var db in DatabaseService.Configurations)
            {
                items.Add(new DatabasesListItem()
                {
                    GraphConfiguration = db.Value
                });
            }
        }


        #endregion

        public bool EnableWizard { get; set; }

        public void QueryDesignerWindowModalContent(List<DesignerWindowModalContent> content)
        {
            if(EnableWizard)    
            content.Add(new DesignerWindowModalContent()
            {
                Drawer = DrawDatabasesWizard,
                ZIndex = 1
            });
        }

        public void QueryToolbarCommands(ToolbarUI ui)
        {
            ui.AddCommand(new ToolbarItem()
            {
                Command = new LambdaCommand("Manage Databases",()=>EnableWizard = true),
                Description = "Open database management dialog.",
                Position = ToolbarPosition.Left,
                Title = "Manage Databases"
            });
        }
    }

    public class CreateDatabaseCommand : ICommand
    {

        public string Title { get; set; }

        [InspectorProperty("A namespace to be used for all the generated classes.")]
        public string Namespace { get; set; }

        [InspectorProperty("A path for the generated code output.")]
        public string CodePath { get; set; }

        [InspectorProperty("A unique name for the database.")]
        public string Name { get; set; }

    }
}
