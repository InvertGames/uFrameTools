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

    protected DiagramNodeDrawer(ElementsDiagram diagram) :base(diagram)
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

    protected DiagramNodeDrawer(ElementsDiagram diagram)
    {
        Diagram = diagram;
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

    public Rect CalculateItemBounds(float width, float localY)
    {
        if (ViewModel.IsCollapsed) 
            return ViewModel.HeaderPosition;

        var location = ViewModel.Position;

        var itemRect = new Rect
        {
            x = location.x + Padding,
            width = width - (Padding * 2),
            height = ItemHeight,
            y = location.y + localY
        };
        return itemRect;
    }
    public  Rect Bounds { get; set; }
    //protected virtual void DrawHeader(ElementsDiagram diagram, bool importOnly)
    //{
    //    if (ViewModel.AllowCollapsing)
    //    {
    //        var rect = new Rect((Bounds.x + (Bounds.width / 2f)) - (ElementDesignerStyles.NodeCollapse.fixedWidth / 2f),
    //            Bounds.y + Bounds.height - 9, 42f, 18f);

    //        if (GUI.Button(rect.Scale(Scale), string.Empty,
    //            ViewModel.IsCollapsed  ? ElementDesignerStyles.NodeExpand : ElementDesignerStyles.NodeCollapse))
    //        {
    //            Diagram.ExecuteCommand((item) =>
    //            {
    //                ViewModel.IsCollapsed = !ViewModel.IsCollapsed;
    //                Dirty = true;
    //                diagram.Dirty = true;
    //                //CalculateBounds();
    //            });
    //        }

    //    }


    //    var style = new GUIStyle(ElementDesignerStyles.ViewModelHeaderStyle);
    //    style.normal.textColor = BackgroundStyle.normal.textColor;
    //    style.alignment = TextAnchor.MiddleCenter;
    //    var position = new Rect(ViewModel.HeaderPosition);
    //    position.y += (ViewModel.IsCollapsed || !ViewModel.AllowCollapsing ? (Bounds.height - 7) * ViewModel.Scale : ViewModel.HeaderSize) / 2f;
    //    position.y -= (12.5f * Scale);

    //    if (ViewModel.IsEditing && !importOnly)
    //    {
    //        GUI.SetNextControlName(ViewModel.Name);

    //        EditorGUI.BeginChangeCheck();
    //        var newText = GUI.TextField(position.Scale(Scale), ViewModel.Name, style);

    //        if (EditorGUI.EndChangeCheck())
    //        {

    //            //Undo.RecordObject(diagram.Data, "Set Element Name");
    //            ViewModel.Rename(newText);
    //            Refresh(Vector2.zero);
    //            //EditorUtility.SetDirty(diagram.Data);
    //        }

         
    //        position.y += (8f * Scale);
    //        style = new GUIStyle(EditorStyles.miniLabel);
    //        style.fontSize = Mathf.RoundToInt(10 * Scale);
    //        style.alignment = TextAnchor.MiddleCenter;
    //        GUI.Label(position.Scale(Scale), ViewModel.SubTitle, style);

    //    }
    //    else
    //    {
    //        //if (!AllowCollapsing)
    //        //{
    //        //    style.alignment = TextAnchor.MiddleCenter;
    //        //}

    //        GUI.Label(position.Scale(Scale), _cachedLabel ?? string.Empty, style);
    //        position.y += (8f * Scale);
    //        style = new GUIStyle(EditorStyles.miniLabel);
    //        style.fontSize = Mathf.RoundToInt(8 * Scale);
    //        style.alignment = TextAnchor.MiddleCenter;
    //        GUI.Label(position.Scale(Scale), ViewModel.SubTitle, style);
    //    }
    //}

    public ElementsDiagram Diagram { get; set; }

    public virtual Type CommandsType
    {
        get { return typeof(IDiagramNode); }
    }

    protected virtual void GetContentDrawers(List<IDrawer> drawers)
    {
        drawers.Add(new HeaderDrawer()
        {
            BackgroundStyle = HeaderStyle,
            TextStyle = ElementDesignerStyles.ViewModelHeaderStyle,
            ViewModelObject = ViewModelObject
        });
    }

    protected virtual void DrawContent(ElementsDiagram diagram, bool importOnly)
    {
        
        //foreach (var diagramSubItemGroup in CachedItemGroups)
        //{
        //    diagramSubItemGroup.Draw(Diagram);
        //    foreach (var item in diagramSubItemGroup.Items)
        //    {
        //        DrawItem(item, diagram, importOnly);
        //    }
        //}

        //PropertiesHeader.Draw();
        //foreach (var viewModelPropertyData in Data.Properties.ToArray())
        //{
        //    DrawItem(viewModelPropertyData, diagram, importOnly);
        //}
        //CollectionsHeader.Draw();
        //var collections = Data.Collections.ToArray();
        //foreach (var viewModelCollectionData in collections)
        //{
        //    DrawItem(viewModelCollectionData, diagram, importOnly);
        //}
        //CommandsHeader.Draw();
        //foreach (var viewModelCommandData in Data.Commands.ToArray())
        //{
        //    DrawItem(viewModelCommandData, diagram, importOnly);
        //}
        //if (Data.CurrentViewModelType != null)
        //{
        //    BehavioursHeader.Draw();
        //    foreach (var behaviour in Behaviours)
        //    {
        //        DrawItem(behaviour, diagram, importOnly);
        //    }
        //}
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
                Diagram.ExecuteCommand((item) =>
                {
                    ViewModel.IsCollapsed = !ViewModel.IsCollapsed;
                    Dirty = true;
                    Diagram.Dirty = true;
                    //CalculateBounds();
                });
            }

        }

        //if (ViewModel.IsCollapsed || !ViewModel.AllowCollapsing)
        //{
        //    ElementDesignerStyles.DrawExpandableBox(Bounds.Scale(Scale), ElementDesignerStyles.NodeBackground, string.Empty, 20);
        //    ElementDesignerStyles.DrawExpandableBox(Bounds.Scale(Scale), HeaderStyle, string.Empty, 20);    
        //}
        //else
        //{
        //    var rect = new Rect(Bounds);
        //    rect.height = ViewModel.HeaderSize;
        //    ElementDesignerStyles.DrawExpandableBox(Bounds.Scale(Scale), ElementDesignerStyles.NodeBackground, string.Empty, 20);
        //    ElementDesignerStyles.DrawExpandableBox(rect.Scale(Scale), HeaderStyle, string.Empty, new RectOffset(20,20,27,0));
        //}
        
        //DrawHeader(diagram, false);

      
        //else if (Data.Equals(diagram.CurrentMouseOverNode))
        //    ElementDesignerStyles.DrawExpandableBox(Bounds.Scale(Scale), ElementDesignerStyles.BoxHighlighter1, string.Empty);

        foreach (var item in Children)
        {
            item.Draw(scale);
        }
        if (ViewModel.IsSelected)
        {
            ElementDesignerStyles.DrawExpandableBox(adjustedBounds.Scale(Scale), ElementDesignerStyles.BoxHighlighter2, string.Empty, 20);
        }

        //if (!ViewModel.IsCollapsed)
        //    DrawContent(diagram, false);
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
            Diagram.ExecuteCommand(RemoveItemCommand);
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
                Diagram.ExecuteCommand(p => nodeItem.Rename(ViewModel.GraphItemObject, newName));
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

        //var location = ViewModel.Position;
        //var width = Width;
        var startY = ViewModel.Position.y;

        // Get our content drawers
        var drawers = new List<IDrawer>();
        GetContentDrawers(drawers);
        // Cache them as we only want to pre-calculate their positions
        if (ViewModel.IsCollapsed)
        {
            Children = drawers.Take(1).ToArray();
        }
        else
        {
            Children = drawers.ToArray();
        }
      

        foreach (var cachedDrawer in Children)
        {
            cachedDrawer.Refresh(new Vector2(ViewModel.Position.x,startY));
            startY += cachedDrawer.Bounds.height;
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

    //public IEnumerable<IDiagramNodeItem> Items
    //{
    //    get { return CachedItemGroups.SelectMany(p => p.Items); }
    //}

    public virtual void DoubleClicked()
    {

    }

    public void OnDeselecting(InputManager inputManager)
    {
        
    }

    public void OnSelecting(InputManager inputManager)
    {
        
    }

    public void OnDeselected(InputManager inputManager)
    {
        
    }

    public void OnSelected(InputManager inputManager)
    {
        
    }

    public void OnMouseExit(InputManager inputManager)
    {
        
    }

    public void OnMouseEnter(InputManager inputManager)
    {
        
    }

    public void OnMouseMove(InputManager inputManager)
    {
        
    }

    public void OnDrag(InputManager inputManager)
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

    public void CommandExecuted(IEditorCommand command)
    {
        Diagram.CommandExecuting(command);
    }

    public void CommandExecuting(IEditorCommand command)
    {
        Diagram.CommandExecuting(command);
    }
}