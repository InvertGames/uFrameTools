using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class GraphDesignerNavigationViewModel
    {
        private List<NavigationItem> _tabs;
        private List<NavigationItem> _breadcrubs;
        private DesignerWindow _designerWindow;


        public IBreadcrumbsStyleSchema BreadcrumbsStyle
        {
            get { return CachedStyles.DefaultBreadcrumbsStyleSchema; }
            set { }
        }

        public List<NavigationItem> Tabs
        {
            get { return _tabs ?? (_tabs = new List<NavigationItem>()); }
            set { _tabs = value; }
        }

        public List<NavigationItem> Breadcrubs
        {
            get { return _breadcrubs ?? (_breadcrubs = new List<NavigationItem>()); }
            set { _breadcrubs = value; }
        }

        public DesignerWindow DesignerWindow
        {
            get
            {
                return _designerWindow ?? InvertApplication.Container.Resolve<DesignerWindow>();
            }
            set { _designerWindow = value; }
        }

        public void Refresh()
        {
//            foreach (var tab in DesignerWindow.Designer.Tabs)
//            {
//                Tabs.Add(new NavigationItem()
//                {
//                    Icon = "CommandIcon",
//                    SpecializedIcon = null,
//                    State = DesignerWindow.Designer.CurrentTab.Graph == tab ? NavigationItemState.Current : NavigationItemState.Regular,
//                    Title = tab.Title,
//                    NavigationAction = x =>
//                    {
//                        Debug.Log("Please implement navigation");
//                    }
//                });
//            }

            Breadcrubs.Clear();

            foreach (var filter in new[] { DiagramViewModel.GraphData.RootFilter }.Concat(this.DiagramViewModel.GraphData.GetFilterPath()))
            {
                var navigationItem = new NavigationItem()
                {
                    Icon = "CommandIcon",
                    Title = filter.Name,
                    State = DiagramViewModel.GraphData != null && DiagramViewModel.GraphData.CurrentFilter == filter ? NavigationItemState.Current : NavigationItemState.Regular,
                    NavigationAction = x =>
                    {
                        Debug.Log("Please implement navigation");
                    }       
                };

                if (filter == DiagramViewModel.GraphData.RootFilter) navigationItem.SpecializedIcon = "RootFilterIcon";

                Breadcrubs.Add(navigationItem);
            }
        }

        public DiagramViewModel DiagramViewModel { get; set; }

    }

    public class NavigationItem
    {
        public string Title { get; set; }
        public string Icon { get; set; }
        public string SpecializedIcon { get; set; }
        public NavigationItemState State { get; set; }
        public Action<Vector2> NavigationAction { get; set; }
    }

    public enum NavigationItemState
    {
        Regular,
        Current,
        Disabled,
    }


}