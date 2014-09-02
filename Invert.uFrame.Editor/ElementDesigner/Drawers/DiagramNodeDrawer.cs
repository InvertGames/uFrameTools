using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Common;
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

    public TViewModel NodeViewModel
    {
        get { return ViewModel as TViewModel; }
    }
}
public abstract class DiagramNodeDrawer : INodeDrawer
{
    private static GUIStyle _itemStyle;
    
    private string _cachedLabel;

    [Inject]
    public IUFrameContainer Container { get; set; }

    public DiagramNodeViewModel ViewModel { get; set; }

    protected DiagramNodeDrawer()
    {

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
        get { return 12; }
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

    public bool Dirty { get; set; }
    string IDrawer.ShouldFocus { get; set; }


    public  Rect Bounds { get; set; }
    

    public ElementsDiagram Diagram { get; set; }


    protected virtual void GetContentDrawers(List<IDrawer> drawers)
    {
        drawers.Add(new HeaderDrawer()
        {
            BackgroundStyle = HeaderStyle,
            TextStyle = ElementDesignerStyles.ViewModelHeaderStyle,
            ViewModelObject = ViewModelObject
        });
    }


    public string ShouldFocus { get { return ViewModel.IsEditing ? ViewModel.Name : ViewModel.ContainedItems.Where(p => p.IsSelected).Select(p => p.Name).FirstOrDefault(); } }

 

    public virtual void Draw(float scale)
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
        if (ViewModel.IsSelected)
        {
            ElementDesignerStyles.DrawExpandableBox(adjustedBounds.Scale(Scale), ElementDesignerStyles.BoxHighlighter2, string.Empty, 20);
        }
    }

    public void Refresh()
    {
        Refresh(Vector2.zero);
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

    protected virtual void DrawItem(IDiagramNodeItem item, ElementsDiagram diagram, bool importOnly)
    {
        if (item.IsSelected && item.IsSelectable && !importOnly)
        {
            var rect = new Rect(item.Position).Scale(Scale);
            //rect.y += ItemHeight;
            //rect.height -= ItemHeight;
            //rect.height += ItemExpandedHeight;
            GUI.Box(rect, string.Empty, SelectedItemStyle);
            GUILayout.BeginArea(rect);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();

            DrawSelectedItem(item, diagram);
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        else
        {

            GUI.Box(item.Position.Scale(Scale), string.Empty, item.IsSelected ? SelectedItemStyle : ItemStyle);

            DrawItemLabel(item);

        }
        if (!string.IsNullOrEmpty(item.Highlighter))
        {
            var highlighterPosition = new Rect(item.Position);
            highlighterPosition.width = 4;
            highlighterPosition.y += 2;
            highlighterPosition.x += 2;
            highlighterPosition.height = ItemHeight - 6;
            GUI.Box(highlighterPosition.Scale(Scale), string.Empty, ElementDesignerStyles.GetHighlighter(item.Highlighter));
        }
    }

    protected virtual void DrawItemLabel(IDiagramNodeItem item)
    {
        var style = new GUIStyle(ItemStyle);
        style.normal.textColor = BackgroundStyle.normal.textColor;
        GUI.Label(item.Position.Scale(Scale), item.Label, style);


    }

    protected virtual void DrawSelectedItem(IDiagramNodeItem nodeItem, ElementsDiagram diagram)
    {
        DrawSelectedItemLabel(nodeItem);


        if (GUILayout.Button(string.Empty, UBStyles.RemoveButtonStyle.Scale(Scale)))
        {
            //this.ExecuteCommand(new SimpleEditorCommand<DiagramNodeItem>(p => nodeItem.Rename(Data, newName)));
            uFrameEditor.ExecuteCommand(RemoveItemCommand);
        }
    }

    protected virtual void DrawSelectedItemLabel(IDiagramNodeItem nodeItem)
    {
        GUI.SetNextControlName(nodeItem.Name);
        var newName = EditorGUILayout.TextField(nodeItem.Name, ElementDesignerStyles.ClearItemStyle);
        if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(newName))
        {
            if (ViewModel.Items.All(p => p.Name != newName))
            {
                //Undo.RecordObject(diagram.Data, "Rename");
                //Diagram.ExecuteCommand(RemoveItemCommand);
                uFrameEditor.ExecuteCommand(p => nodeItem.Rename(ViewModel.GraphItemObject, newName));
                //EditorUtility.SetDirty(diagram.Data);
            }
        }
    }

    public virtual IEditorCommand RemoveItemCommand
    {
        get { return uFrameEditor.Container.Resolve<IEditorCommand>("RemoveNodeItem"); }
    }

    public virtual void Refresh(Vector2 position)
    {
        var startY = ViewModel.Position.y;

        // Get our content drawers
        var drawers = new List<IDrawer>();
        GetContentDrawers(drawers);

        ViewModel.ContentItems.Clear();
        foreach (var drawer in drawers)
        {
            ViewModel.ContentItems.Add(drawer.ViewModelObject);
        }
        // Cache them as we only want to pre-calculate their positions
        if (ViewModel.IsCollapsed)
        {
            Children = drawers.Take(1).ToArray();
        }
        else
        {
            Children = drawers.ToArray();
        }
      

        foreach (var child in Children)
        {
            child.Refresh(new Vector2(ViewModel.Position.x,startY));
            startY += child.Bounds.height;
        }
        // Now lets stretch all the content drawers to the maximum width
        var maxWidth = 0f;
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
        //ViewModel.HeaderPosition = new Rect(ViewModel.Position.x, ViewModel.Position.y, maxWidth, ViewModel.HeaderSize);
    }

    public bool IsSelected
    {
        get { return ViewModel.IsSelected; }
        set { ViewModel.IsSelected = value; }
    }

    public GraphItemViewModel ViewModelObject
    {
        get { return ViewModel; }
   
    }


    public bool IsExternal { get; set; }

    public IEnumerable<IDrawer> SelectedChildren
    {
        get { return Children.Where(p => p.IsSelected); }
    }
    public IDrawer[] Children { get; set; }

    public virtual void DoubleClicked()
    {

    }

    public void OnDeselecting()
    {
        
    }

    public void OnSelecting()
    {
        
    }

    public void OnDeselected()
    {
        
    }

    public void OnSelected()
    {
        
    }

    public void OnMouseExit()
    {
        
    }

    public void OnMouseEnter()
    {
        
    }

    public void OnMouseMove()
    {
        
    }

    public void OnDrag()
    {
        
    }

    public void OnMouseUp()
    {
        
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

 
}