using Invert.Common;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

public class DiagramSubItemGroup : Drawer
{
    public NodeItemHeader Header { get; set; }
    public IDiagramNodeItem[] Items { get; set; }

    //public GraphItemViewModel ViewModelObject { get; set; }

    //public void Draw(ElementsDiagram elementsDiagram)
    //{
        
    //}
}

public class EnumItemDrawer : ItemDrawer
{
    public EnumItemDrawer(EnumItem item)
    {
        this.ViewModelObject = new EnumItemViewModel(item);
    }
    
    public override void Draw(float scale)
    {
        base.Draw(scale);
    }
}

public class ItemDrawer : Drawer
{
    private GUIStyle _textStyle;
    private GUIStyle _backgroundStyle;
    private GUIStyle _selectedItemStyle;

    public ItemViewModel ItemViewModel
    {
        get { return this.ViewModelObject as ItemViewModel; }
    }

    public virtual int Padding
    {
        get { return 1; }
    }

    public GUIStyle BackgroundStyle
    {
        get { return _backgroundStyle ?? (_backgroundStyle = ElementDesignerStyles.Item4); }
        set { _backgroundStyle = value; }
    }

    public GUIStyle SelectedItemStyle
    {
        get { return _selectedItemStyle ?? (_selectedItemStyle = ElementDesignerStyles.SelectedItemStyle); }
        set { _selectedItemStyle = value; }
    }
    public GUIStyle TextStyle
    {
        get { return _textStyle ?? (_textStyle = ElementDesignerStyles.Item4); }
        set { _textStyle = value; }
    }

    public override void Refresh(Vector2 position)
    {
        base.Refresh(position);
        // Calculate the size of the label and add the padding * 2 for left and right
        var textSize = TextStyle.CalcSize(new GUIContent(ItemViewModel.Name));
        var width = textSize.x + (Padding * 2);
        var height = textSize.y + (Padding * 2);

        this.Bounds = new Rect(position.x,position.y, width, height);
        

    }

    public override void Draw(float scale)
    {
        base.Draw(scale);
        if (ItemViewModel.IsSelected && ItemViewModel.IsSelectable)
        {
           
            GUI.Box(Bounds.Scale(scale), string.Empty, SelectedItemStyle);
            GUILayout.BeginArea(Bounds);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();

            GUI.SetNextControlName(ItemViewModel.Name);
            var newName = EditorGUILayout.TextField(ItemViewModel.Name, ElementDesignerStyles.ClearItemStyle);
            if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(newName))
            {
                uFrameEditor.ExecuteCommand(p => ItemViewModel.Rename(newName));
            }

            if (GUILayout.Button(string.Empty, UBStyles.RemoveButtonStyle.Scale(scale)))
            {
                uFrameEditor.ExecuteCommand(ItemViewModel.RemoveItemCommand);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        else
        {

            GUI.Box(Bounds.Scale(scale), string.Empty, ItemViewModel.IsSelected ? SelectedItemStyle : TextStyle);

            var style = new GUIStyle(TextStyle);
            style.normal.textColor = BackgroundStyle.normal.textColor;
            GUI.Label(Bounds.Scale(scale), ItemViewModel.Name, style);

        }
        if (!string.IsNullOrEmpty(ItemViewModel.Highlighter))
        {
            var highlighterPosition = new Rect(Bounds);
            highlighterPosition.width = 4;
            highlighterPosition.y += 2;
            highlighterPosition.x += 2;
            highlighterPosition.height = Bounds.height - 6;
            GUI.Box(highlighterPosition.Scale(scale), string.Empty, ElementDesignerStyles.GetHighlighter(ItemViewModel.Highlighter));
        }
    }
    protected virtual void DrawItemLabel(IDiagramNodeItem item)
    {
    }

    protected virtual void DrawSelectedItem(IDiagramNodeItem nodeItem)
    {
       
    }

    protected virtual void DrawSelectedItemLabel(IDiagramNodeItem nodeItem)
    {
      
    }
}

public class ElementItemDrawer : ItemDrawer
{
    public ElementItemViewModel ElementItemViewModel
    {
        get
        {
            return ViewModelObject as ElementItemViewModel;
        }
    }
    public ElementItemDrawer(ElementItemViewModel viewModel)
    {
        ViewModelObject = viewModel;
    }

    public override void Refresh(Vector2 position)
    {
        base.Refresh(position);
         var nameSize = TextStyle.CalcSize(new GUIContent(ElementItemViewModel.Name));
         var typeSize = TextStyle.CalcSize(new GUIContent(ElementItemViewModel.RelatedType));

         Bounds = new Rect(position.x, position.y, 5 + nameSize.x + 5 + typeSize.x + 5, 25);
    }

    public override void Draw(float scale)
    {

        base.Draw(scale);
    }
}

public class HeaderDrawer : Drawer
{
    private GUIStyle _textStyle;
    private GUIStyle _backgroundStyle;
    public virtual int Padding
    {
        get { return 12; }
    }
    public GUIStyle BackgroundStyle
    {
        get { return _backgroundStyle ?? (_backgroundStyle = ElementDesignerStyles.ItemStyle); }
        set { _backgroundStyle = value; }
    }

    public GUIStyle TextStyle
    {
        get { return _textStyle ?? (_textStyle = ElementDesignerStyles.NodeBackground); }
        set { _textStyle = value; }
    }

    public DiagramNodeViewModel NodeViewModel
    {
        get { return ViewModelObject as DiagramNodeViewModel; }
    }

    public override void Refresh(Vector2 position)
    {
        base.Refresh(position);
        TextSize = TextStyle.CalcSize(new GUIContent(NodeViewModel.Label));
        var width = Mathf.Max(TextSize.x + (Padding * 2),Bounds.width);
        
        if (NodeViewModel.IsCollapsed)
        {
            this.Bounds = new Rect(position.x, position.y, width, TextSize.y + (Padding * 2));
         
        }
        else
        {
            this.Bounds = new Rect(position.x, position.y, width, 32);
        }
    }

    public Vector2 TextSize { get; set; }

    public Rect AdjustedBounds { get; set; }

    public override void Draw(float scale)
    {
        base.Draw(scale);
        if (NodeViewModel.IsCollapsed)
        {
            AdjustedBounds = new Rect(Bounds.x - 9, Bounds.y + 1, Bounds.width + 19, Bounds.height + 9);
        }
        else
        {
            AdjustedBounds = new Rect(Bounds.x - 9, Bounds.y + 1, Bounds.width + 19, 27);
        }
        if (NodeViewModel.IsCollapsed)
        {
            ElementDesignerStyles.DrawExpandableBox(AdjustedBounds.Scale(scale), BackgroundStyle, string.Empty, 20);
        }
        else
        {
            ElementDesignerStyles.DrawExpandableBox(AdjustedBounds.Scale(scale), BackgroundStyle, string.Empty, new RectOffset(20, 20, 27, 0));
        }

        var style = new GUIStyle(TextStyle)
        {
            normal = {textColor = BackgroundStyle.normal.textColor},
            alignment = TextAnchor.MiddleCenter
        };
        // The bounds for the main text
        var textBounds = new Rect(Bounds.x, Bounds.y + ((Bounds.height / 2f) - (TextSize.y / 2f)), Bounds.width,
               Bounds.height);

        if (NodeViewModel.IsEditing)
        {
            GUI.SetNextControlName(NodeViewModel.Name);

            EditorGUI.BeginChangeCheck();
            var newText = GUI.TextField(textBounds.Scale(scale), NodeViewModel.Name, style);

            if (EditorGUI.EndChangeCheck())
            {
                NodeViewModel.Rename(newText);
                Dirty = true;
            }


          
            style = new GUIStyle(EditorStyles.miniLabel);
            style.fontSize = Mathf.RoundToInt(10 * scale);
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(Bounds.Scale(scale), NodeViewModel.SubTitle, TextStyle);

        }
        else
        {
            var titleStyle = new GUIStyle(TextStyle);
            titleStyle.normal.textColor = BackgroundStyle.normal.textColor;
            titleStyle.alignment = TextAnchor.MiddleCenter;
           

            GUI.Label(textBounds, NodeViewModel.Label ?? string.Empty, titleStyle);
            textBounds.x+=TextSize.y / 2f;

            GUI.Label(Bounds.Scale(scale), NodeViewModel.SubTitle, ElementDesignerStyles.ViewModelHeaderStyle);
        }
    }
}
