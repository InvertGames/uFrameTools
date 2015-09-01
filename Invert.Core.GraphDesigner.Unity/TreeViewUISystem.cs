using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Core.GraphDesigner.Systems.GraphUI;
using UnityEngine;

/*
 * ACHTUNG: This is an ugly piece of code, but it does everything.
 */

public class TreeViewUISystem : DiagramPlugin, IDrawTreeView, IQueryDesignerWindowModalContent
{
    private IPlatformDrawer _platformDrawer;
    private Vector2 _scrollPos;
    private string _searchCriteria;
    private IItem[] _treeData;
    private TreeViewModel _treeViewModel;
    private WorkspaceService _workspaceService;


    public WorkspaceService WorkspaceService
    {
        get
        {
            return _workspaceService ?? (_workspaceService = InvertApplication.Container.Resolve<WorkspaceService>());
        }
        set { _workspaceService = value; }
    }

    public IPlatformDrawer PlatformDrawer
    {
        get { return _platformDrawer ?? (_platformDrawer = InvertApplication.Container.Resolve<IPlatformDrawer>()); }
        set { _platformDrawer = value; }
    }

    public IItem[] TreeData
    {
        get { return _treeData ?? (_treeData = WorkspaceService.CurrentWorkspace.Graphs.OfType<IItem>().ToArray()); }
        set { _treeData = value; }
    }

//    private void DrawTestTree(Rect obj)
//    {
//
//        PlatformDrawer.DrawTextbox("asdfsadf",new Rect().WithSize(80,30).Align(obj).Above(obj),_searchCriteria,ElementDesignerStyles.ItemTextEditingStyle,
//            (a, b) =>
//            {
//                _searchCriteria = a;
//                TreeViewModel.Refresh();
//            });
//        PlatformDrawer.DrawStretchBox(obj, CachedStyles.WizardSubBoxStyle, 13);
////
////        if (string.IsNullOrEmpty(_searchCriteria))
////        {
////            DrawTreeView(obj.PadSides(15), TreeData,ref _scrollPos, (m, i) => { });
////        }
////        else
////        {
////            DrawTreeView(obj.PadSides(15), TreeData, ref _scrollPos, (m, i) => { }, null, i => i.Title.Contains(_searchCriteria));
////        }
//
//        if (!string.IsNullOrEmpty(_searchCriteria))
//        {
//            TreeViewModel.Predicate = i => i.Title.Contains(_searchCriteria);
//        }
//        else
//        {
//            TreeViewModel.Predicate = null;
//        }
//
//      
//
//        DrawTreeView(obj.PadSides(15),TreeViewModel,(m,i)=>{});
//
//
//    }

    public void DrawTreeView(Rect bounds, TreeViewModel viewModel, Action<Vector2, IItem> itemClicked,
        Action<Vector2, IItem> itemRightClicked = null)
    {
        var boundY = bounds.height;
        if (Event.current != null && Event.current.isKey && Event.current.rawType == EventType.KeyUp)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.UpArrow:
                    viewModel.MoveUp();
                    break;
                case KeyCode.DownArrow:
                    viewModel.MoveDown();
                    break;
                case KeyCode.RightArrow:
                {
                    var selectedContainer = viewModel.SelectedData as ITreeItem;
                    if (selectedContainer != null)
                    {
                        selectedContainer.Expanded = true;
                        viewModel.IsDirty = true;
                    }
                }
                    break;
                case KeyCode.LeftArrow:
                {
                    var selectedContainer = viewModel.SelectedData as ITreeItem;
                    if (selectedContainer != null)
                    {
                        selectedContainer.Expanded = false;
                        viewModel.IsDirty = true;
                    }
                }
                    break;
                case KeyCode.Return:
                    if(viewModel!= null)
                    viewModel.InvokeSubmit();
                    break;
                default:
                    break;
            }
        }
        //   PlatformDrawer.DrawLabel(new Rect().WithSize(100,100).InnerAlignWithBottomRight(bounds),"Total height: {0}, Total Items: {1}");
        var dirty = false;
        var position = bounds;
        var usedRect = position.Pad(0, 0, 15, 0).WithHeight(25*viewModel.TreeData.Count(s => s.Visible));

        PlatformDrawer.DrawStretchBox(position.PadSides(-1),CachedStyles.WizardListItemBoxStyle,10);

      

        viewModel.Scroll = GUI.BeginScrollView(position, viewModel.Scroll, usedRect);

        var itemTemplateRect = bounds.WithHeight(25);
        bool hasItems = false;

        foreach (var treeViewItem in viewModel.TreeData)
        {
            if (!treeViewItem.Visible) continue;
            hasItems = true;
            var data = treeViewItem.Data;

            var treeData = data as ITreeItem;

            var itemRect = itemTemplateRect.Pad(10*treeViewItem.Indent, 0, 10*treeViewItem.Indent, 0);
            
            var localItemY = itemRect.Translate(0, -position.yMin).y;

            var imageRect = new Rect().WithSize(16, 16)
                .Align(itemRect)
                .AlignHorisonallyByCenter(itemRect)
                .Translate(5, 0);

            var labelRect =
                itemRect.WithWidth(
                    PlatformDrawer.CalculateTextSize(treeViewItem.Data.Title, CachedStyles.BreadcrumbTitleStyle).x)
                    .Translate(25, 0);

            if (treeViewItem == viewModel.ScrollTarget)
            {
                viewModel.Scroll = new Vector2(0, localItemY-25*5);
                viewModel.ScrollToItem(null);
            }

            if (treeViewItem.Selected)
            {
                PlatformDrawer.DrawStretchBox(itemRect, CachedStyles.WizardSubBoxStyle, 14);
            }

            PlatformDrawer.DrawLabel(labelRect, treeViewItem.Data.Title, CachedStyles.BreadcrumbTitleStyle);
            PlatformDrawer.DrawImage(imageRect, treeViewItem.Icon, true);


            var item1 = treeViewItem;
            PlatformDrawer.DoButton(itemRect.Translate(25, 0), "", CachedStyles.ClearItemStyle,
                m =>
                {
                    viewModel.SelectedIndex = item1.Index;
                    //TODO PUBLISH EVENT
                    if (itemClicked != null) itemClicked(m, item1.Data);
                }, m => { if (itemRightClicked != null) itemRightClicked(m, item1.Data); });

            if (treeData != null)
                PlatformDrawer.DoButton(imageRect, "", CachedStyles.ClearItemStyle,
                    () =>
                    {
                        treeData.Expanded = !treeData.Expanded;
                        dirty = true;
                    });

            if (treeViewItem.Highlighted)
            {
                PlatformDrawer.DrawLine(new[]
                {
                    new Vector3(labelRect.x, itemRect.yMax - 1, 0),
                    new Vector3(labelRect.x + 75, itemRect.yMax - 1, 0)
                }, Color.cyan);
            }

            itemTemplateRect = itemTemplateRect.Below(itemTemplateRect);
        }

  


        GUI.EndScrollView();

        if (!hasItems)
        {
            var textRect = bounds;
            var cacheColor = GUI.color;
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 0.4f);
            PlatformDrawer.DrawLabel(textRect, "No Items Found", CachedStyles.WizardSubBoxTitleStyle, DrawingAlignment.MiddleCenter);
            GUI.color = cacheColor;
            return;
        }

        if (dirty) viewModel.IsDirty = true;
    }

    // Make cool icons

    public void QueryDesignerWindowModalContent(List<DesignerWindowModalContent> content)
    {
//        content.Add(new DesignerWindowModalContent()
//        {
//            Drawer = DrawTestTree,
//            ZIndex = 1
//        });
    }
}

public static class ITreeItemExtensions
{
    public static int CountVisibleItems(this ITreeItem item)
    {
        var items = 0;

        if (item.Expanded)
            foreach (var childItems in item.Children)
            {
                items++;
                var childTree = childItems as ITreeItem;
                if (childTree != null) items += childTree.CountVisibleItems();
            }

        return items;
    }
}

public class TreeViewModel
{
    private List<IItem> _data;
    private Func<IItem, bool> _predicate;
    private int _selectedIndex;
    private List<TreeViewItem> _treeData;

    public TreeViewModel()
    {
        IsDirty = true;
    }

    public Vector2 Scroll { get; set; }

    public List<IItem> Data
    {
        get { return _data; }
        set
        {
            _data = value;
            ConstructData();
            Refresh();
        }
    }

    public IItem SelectedData
    {
        get { return SelectedItem != null ? SelectedItem.Data : null; }
    }

    public TreeViewItem SelectedItem
    {
        get { return SelectedIndex > -1 && SelectedIndex < TreeData.Count ? TreeData[SelectedIndex] : null; }
    }

    public int SelectedIndex
    {
        get { return _selectedIndex; }
        set
        {
            if (_selectedIndex == value) return;
            _selectedIndex = value;  
            ScrollToItem(SelectedItem);
            IsDirty = true;
        }
    }

    public Action<IItem> Submit { get; set; }

    public List<TreeViewItem> TreeData
    {
        get { return _treeData ?? (_treeData = new List<TreeViewItem>()); }
        set { _treeData = value; }
    }

    public bool IsDirty { get; set; }

    public Func<IItem, bool> Predicate
    {
        get { return _predicate; }
        set
        {
            if (_predicate == value) return;
            _predicate = value;
            IsDirty = true;
        }
    }

    public void MoveUp()
    {
        var ndx = SelectedIndex - 1;
        while (ndx >= 0)
        {
            if (TreeData[ndx].Visible)
            {
                SelectedIndex = ndx;
                return;
            }
            ndx--;
        }

        ndx = TreeData.Count - 1;
        while (ndx >= SelectedIndex)
        {
            if (TreeData[ndx].Visible)
            {
                SelectedIndex = ndx;

                return;
            }
            ndx--;
        }
    }

    public void MoveDown()
    {
        var ndx = SelectedIndex + 1;
        while (ndx < TreeData.Count)
        {
            if (TreeData[ndx].Visible)
            {
                SelectedIndex = ndx;
                return;
            }
            ndx++;
        }

        ndx = 0;
        while (ndx <= SelectedIndex)
        {
            if (TreeData[ndx].Visible)
            {
                SelectedIndex = ndx;
                return;
            }
            ndx++;
        }
    }

    public void ConstructData()
    {
        if (Data == null) return;
        TreeData.Clear();
        foreach (var items in Data)
        {
            ExtractItems(TreeData, items, null, 0);
        }
    }

    public void Refresh()
    {
        if (Predicate == null)
        {
            foreach (var item in TreeData)
            {
                var data = item.Data;
                var treeData = data as ITreeItem;
                item.Visible = item.Parent == null || item.Parent.Visible && item.ParentData.Expanded;
                item.Icon = treeData == null || !treeData.Children.Any() ? "DotIcon" : treeData.Expanded ? "MinusIcon_Micro" : "PlusIcon_Micro";
                item.Highlighted = false;
                item.Selected = SelectedIndex == item.Index;
            }
        }
        else
        {
            foreach (var item in TreeData)
            {
                var data = item.Data;
                var treeData = data as ITreeItem;
                var match = Predicate(data);
                item.Highlighted = match;
                item.Selected = SelectedIndex == item.Index;

                if (item.Parent != null && Predicate(item.Parent.Data) && item.ParentData.Expanded)
                {
                    //Show as a content of the parent
                    item.Visible = true;
                }
                else if (match)
                {
                    item.Visible = true;
                    //Force show parent
                    var parent = item.Parent;
                    if (parent != null)
                        while (parent != null)
                        {
                            parent.Visible = true;
                            (parent.Data as ITreeItem).Expanded = true;
                            parent = parent.Parent;
                        }
                }
                else
                {
                    item.Visible = false;
                }

                item.Icon = treeData == null ? "DotIcon" : treeData.Expanded ? "MinusIcon_Micro" : "PlusIcon_Micro";
            }
        }
        IsDirty = false;
    }

    public void ExtractItems(List<TreeViewItem> items, IItem data, TreeViewItem parent, int ident)
    {
        var treeViewItem = new TreeViewItem
        {
            Data = data,
            Index = items.Count,
            Parent = parent,
            Indent = ident
        };
        items.Add(treeViewItem);

        var treeItem = data as ITreeItem;

        if (treeItem != null)
        {
            foreach (var child in treeItem.Children.OrderBy(c =>
            {
                var t = c as ITreeItem;
                return !(t != null && t.Children.Any());
            }).ThenBy(c=>c.Title))
            {
                ExtractItems(items, child, treeViewItem, ident + 1);
            }
        }
    }

    public void InvokeSubmit()
    {
        if (Submit != null) Submit(SelectedData);
    }

    public void ExpandPathTo(TreeViewItem item)
    {
        var parent = item.ParentData;
        if (parent != null)
        {
            parent.Expanded = true;
            ExpandPathTo(item.Parent);
        }

    }


    public TreeViewItem ScrollTarget { get; set; }

    public void ScrollToItem(TreeViewItem item)
    {
        ScrollTarget = item;
    }
}

public class TreeViewItem
{
    public TreeViewItem Parent { get; set; }

    public ITreeItem ParentData
    {
        get { return Parent == null ? null : Parent.Data as ITreeItem; }
    }

    public IItem Data { get; set; }
    public int Index { get; set; }
    public bool Visible { get; set; }
    public int Indent { get; set; }
    public string Icon { get; set; }
    public bool Highlighted { get; set; }
    public bool Selected { get; set; }
}


public interface IDrawTreeView
{
    void DrawTreeView(Rect bounds, TreeViewModel viewModel, Action<Vector2, IItem> itemClicked,
        Action<Vector2, IItem> itemRightClicked = null);
}


