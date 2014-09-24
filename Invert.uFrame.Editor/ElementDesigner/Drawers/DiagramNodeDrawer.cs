using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Invert.Common;
using Invert.MVVM;
using Invert.uFrame;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

public abstract class DiagramNodeDrawer<TViewModel> : DiagramNodeDrawer where TViewModel : DiagramNodeViewModel
{
    protected DiagramNodeDrawer()
    {
    }

    protected DiagramNodeDrawer(TViewModel viewModel)
    {
        this.ViewModelObject = viewModel;
    }

    public override Rect Bounds
    {
        get { return ViewModelObject.Bounds; }
        set { ViewModelObject.Bounds = value; }
    }

    public TViewModel NodeViewModel
    {
        get { return ViewModel as TViewModel; }
    }
}
public abstract class DiagramNodeDrawer : Drawer, INodeDrawer,IDisposable
{
    private static GUIStyle _itemStyle;
    
    private string _cachedLabel;

    [Inject]
    public IUFrameContainer Container { get; set; }

    public DiagramNodeViewModel ViewModel
    {
        get { return DataContext as DiagramNodeViewModel; }
        set { DataContext = value; }
    }

    protected DiagramNodeDrawer()
    {

    }

    protected override void DataContextChanged()
    {
        base.DataContextChanged();
        
        ViewModel.ContentItems.CollectionChanged += ContentItemsOnCollectionChangedWith;
    }

    private void ContentItemsOnCollectionChangedWith(NotifyCollectionChangedEventArgs changeargs)
    {
        this.RefreshContent();
    }


    public float Scale
    {
        get { return ElementDesignerStyles.Scale; }
    }

    public virtual GUIStyle ItemStyle
    {
        get
        {

            return ElementDesignerStyles.Item4;
        }
    }

    public GUIStyle SelectedItemStyle
    {
        get { return ElementDesignerStyles.SelectedItemStyle; }
    }
    
    public virtual float Width
    {
        get
        {
            var maxLengthItem = EditorStyles.largeLabel.CalcSize(new GUIContent(ViewModel.FullLabel));
            if (ViewModel.IsCollapsed)
            {
                foreach (var item in ViewModel.Items)
                {
                    var newSize = EditorStyles.largeLabel.CalcSize(new GUIContent(item.FullLabel));

                    if (maxLengthItem.x < newSize.x)
                    {
                        maxLengthItem = newSize;
                    }
                }
            }
            if (ViewModel.ShowSubtitle)
            {
                var subTitle = EditorStyles.largeLabel.CalcSize(new GUIContent(ViewModel.SubTitle));
                if (subTitle.x > maxLengthItem.x)
                {
                    maxLengthItem = subTitle;
                }
            }


            return Math.Max(150f, maxLengthItem.x + 40);
        }
    }

    public float ItemHeight { get { return 20; } }
    
    public virtual float Padding
    {
        get { return 8; }
    }
    
    public virtual GUIStyle BackgroundStyle
    {
        get
        {

            return ElementDesignerStyles.NodeHeader1;
        }
    }

    public float ItemExpandedHeight
    {
        get { return 0; }
    }

    string IDrawer.ShouldFocus { get; set; }

    

    public ElementsDiagram Diagram { get; set; }


    protected virtual void GetContentDrawers(List<IDrawer> drawers)
    {
      
        foreach (var item in ViewModel.ContentItems)
        {
            var drawer = uFrameEditor.CreateDrawer(item);
            if (drawer == null) Debug.Log(string.Format("Couldn't create drawer for {0} make sure it is registered.", item.GetType().Name));
            drawers.Add(drawer);
        }
    }



    public override void Draw(float scale)
    {
        var offsetPosition = new Rect(Bounds);

        //var label = ViewModel.InfoLabel;
        //if (!string.IsNullOrEmpty(label))
        //{
        //    var style = new GUIStyle(EditorStyles.miniLabel);
        //    style.normal.textColor = new Color(0.1f, 0.1f, 0.1f);
        //    style.fontSize = Mathf.RoundToInt(10 * Scale);
        //    style.alignment = TextAnchor.MiddleCenter;
        //    style.fontStyle = FontStyle.Italic;
        //    GUI.Label(offsetPosition.Scale(Scale), label, style);

        //}


        
       
        var adjustedBounds = new Rect(Bounds.x - 9, Bounds.y + 1, Bounds.width + 19, Bounds.height + 9);
        ElementDesignerStyles.DrawExpandableBox(adjustedBounds.Scale(Scale), ElementDesignerStyles.NodeBackground, string.Empty, 20);

        if (ViewModel.AllowCollapsing)
        {
            
            var rect = new Rect((Bounds.x + (Bounds.width / 2f)) - (ElementDesignerStyles.NodeCollapse.fixedWidth / 2f),
                Bounds.y + Bounds.height, 42f, 18f);

            if (GUI.Button(rect.Scale(Scale), string.Empty,
                ViewModel.IsCollapsed ? ElementDesignerStyles.NodeExpand : ElementDesignerStyles.NodeCollapse))
            {
                uFrameEditor.ExecuteCommand((item) =>
                {
                    ViewModel.IsCollapsed = !ViewModel.IsCollapsed;
                    Dirty = true;
                });
            }

        }


        foreach (var item in Children)
        {
            if (item.Dirty)
            {
                Refresh();
                item.Dirty = false;
            }
            item.Draw(scale);
        }
        
        if (!ViewModel.IsLocal)
        {
            ElementDesignerStyles.DrawExpandableBox(adjustedBounds.Scale(Scale), ElementDesignerStyles.BoxHighlighter5, string.Empty, 20);
        }
        if (ViewModel.IsMouseOver)
        {
            ElementDesignerStyles.DrawExpandableBox(adjustedBounds.Scale(Scale), ElementDesignerStyles.BoxHighlighter3, string.Empty, 20);
        }
        if (ViewModel.IsSelected)
        {
            ElementDesignerStyles.DrawExpandableBox(adjustedBounds.Scale(Scale), ElementDesignerStyles.BoxHighlighter2, string.Empty, 20);
        }
        
    }

    protected virtual GUIStyle HeaderStyle
    {
        get
        {
            return ElementDesignerStyles.NodeHeader1;
        }
    }

    protected virtual GUIStyle GetHighlighter()
    {
        return ElementDesignerStyles.BoxHighlighter4;
    }

    //protected virtual void DrawItem(IDiagramNodeItem item, ElementsDiagram diagram, bool importOnly)
    //{
    //    if (item.IsSelected && item.IsSelectable && !importOnly)
    //    {
    //        var rect = new Rect(item.Position).Scale(Scale);
    //        //rect.y += ItemHeight;
    //        //rect.height -= ItemHeight;
    //        //rect.height += ItemExpandedHeight;
    //        GUI.Box(rect, string.Empty, SelectedItemStyle);
    //        GUILayout.BeginArea(rect);
    //        EditorGUI.BeginChangeCheck();
    //        EditorGUILayout.BeginHorizontal();

    //        DrawSelectedItem(item, diagram);
    //        EditorGUILayout.EndHorizontal();
    //        GUILayout.EndArea();
    //    }
    //    else
    //    {

    //        GUI.Box(item.Position.Scale(Scale), string.Empty, item.IsSelected ? SelectedItemStyle : ItemStyle);

    //        DrawItemLabel(item);

    //    }
    //    if (!string.IsNullOrEmpty(item.Highlighter))
    //    {
    //        var highlighterPosition = new Rect(item.Position);
    //        highlighterPosition.width = 4;
    //        highlighterPosition.y += 2;
    //        highlighterPosition.x += 2;
    //        highlighterPosition.height = ItemHeight - 6;
    //        GUI.Box(highlighterPosition.Scale(Scale), string.Empty, ElementDesignerStyles.GetHighlighter(item.Highlighter));
    //    }
    //}

    //protected virtual void DrawItemLabel(IDiagramNodeItem item)
    //{
    //    var style = new GUIStyle(ItemStyle);
    //    style.normal.textColor = BackgroundStyle.normal.textColor;
    //    GUI.Label(item.Position.Scale(Scale), item.Label, style);


    //}

    public override void OnMouseDown(MouseEvent mouseEvent)
    {
        ViewModelObject.Select();
    }

    public override void OnMouseMove(MouseEvent e)
    {
        base.OnMouseMove(e);
        ViewModel.IsMouseOver = true;
    }

    //protected virtual void DrawSelectedItem(IDiagramNodeItem nodeItem, ElementsDiagram diagram)
    //{
    //    DrawSelectedItemLabel(nodeItem);


    //    if (GUILayout.Button(string.Empty, UBStyles.RemoveButtonStyle.Scale(Scale)))
    //    {
    //        //this.ExecuteCommand(new SimpleEditorCommand<DiagramNodeItem>(p => nodeItem.Rename(Data, newName)));
    //        uFrameEditor.ExecuteCommand(RemoveItemCommand);
    //    }
    //}

    //protected virtual void DrawSelectedItemLabel(IDiagramNodeItem nodeItem)
    //{
    //    GUI.SetNextControlName(nodeItem.Name);
    //    var newName = EditorGUILayout.TextField(nodeItem.Name, ElementDesignerStyles.ClearItemStyle);
    //    if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(newName))
    //    {
    //        if (ViewModel.Items.All(p => p.Name != newName))
    //        {
    //            //Undo.RecordObject(diagram.Data, "Rename");
    //            //Diagram.ExecuteCommand(RemoveItemCommand);
    //            uFrameEditor.ExecuteCommand(p => nodeItem.Rename(ViewModel.GraphItemObject, newName));
    //            //EditorUtility.SetDirty(diagram.Data);
    //        }
    //    }
    //}

    public virtual IEditorCommand RemoveItemCommand
    {
        get { return uFrameEditor.Container.Resolve<IEditorCommand>("RemoveNodeItem"); }
    }
    
    public virtual void RefreshContent()
    {
        var drawers = new List<IDrawer>();
        drawers.Add(new HeaderDrawer()
        {
            BackgroundStyle = HeaderStyle,
            TextStyle = ElementDesignerStyles.ViewModelHeaderStyle,
            ViewModelObject = ViewModelObject,
            Padding = HeaderPadding
        });
        if (!ViewModel.IsCollapsed)
        {
            GetContentDrawers(drawers);
        }
        Children = drawers.ToList();
    }

    public virtual float HeaderPadding
    {
        get { return 10; }
    }

    public override void Refresh(Vector2 position)
    {

        if (Children == null || Children.Count < 1)
        {
            RefreshContent();
        }

        var startY = ViewModel.Position.y;

        // Get our content drawers
        foreach (var child in Children)
        {
            child.Refresh(new Vector2(ViewModel.Position.x,startY));
            startY += child.Bounds.height;
        }
        // Now lets stretch all the content drawers to the maximum width
        var maxWidth = 145f;
        var height = 0f;

        foreach (var item in Children)
        {
            if (item.Bounds.width > maxWidth) maxWidth = item.Bounds.width;
            height += item.Bounds.height;
        }
           

        foreach (var cachedDrawer in Children)
        {
            cachedDrawer.Bounds = new Rect(cachedDrawer.Bounds) { width = maxWidth };
            cachedDrawer.Dirty = false;
        }
        
        _cachedLabel = ViewModel.Label;
        
        if (!ViewModel.IsCollapsed)
        {
            Bounds = new Rect(ViewModel.Position.x, ViewModel.Position.y, maxWidth, height + Padding);
        }
        else
        {
            Bounds = new Rect(ViewModel.Position.x, ViewModel.Position.y, maxWidth, height);
        }

        ViewModel.ConnectorBounds = Children[0].Bounds;
        //ViewModel.HeaderPosition = new Rect(ViewModel.Position.x, ViewModel.Position.y, maxWidth, ViewModel.HeaderSize);
    }

    public bool IsExternal { get; set; }

    public IEnumerable<IDrawer> SelectedChildren
    {
        get { return Children.Where(p => p.IsSelected); }
    }
   

    //private float CalculateGroupBounds(IDrawer group, float width, float startY)
    //{
    //    var sy = startY;
    //    @group.Header.Position = CalculateItemBounds(width, sy);
    //    sy += @group.Header.Position.height;
    //    foreach (var property in @group.Items)
    //    {
    //        property.Position = CalculateItemBounds(width, sy);
    //        sy += property.Position.height;
    //        if (property.IsSelected)
    //        {
    //            sy += ItemExpandedHeight;
    //        }
    //    }
    //    if (ViewModel.IsCollapsed)
    //        return startY;
    //    return sy;
    //}


    public void Dispose()
    {
        ViewModel.ContentItems.CollectionChanged-= ContentItemsOnCollectionChangedWith;
    }
}