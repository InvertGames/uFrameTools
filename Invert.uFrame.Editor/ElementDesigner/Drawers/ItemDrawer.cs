using Invert.Common;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

public class ItemDrawer : Drawer
{
    public ItemDrawer(GraphItemViewModel viewModelObject) : base(viewModelObject)
    {
    }

    public override Rect Bounds
    {
        get { return ViewModelObject.Bounds; }
        set { ViewModelObject.Bounds = value; }
    }

    private GUIStyle _textStyle;
    private GUIStyle _backgroundStyle;
    private GUIStyle _selectedItemStyle;

    public ItemViewModel ItemViewModel
    {
        get { return this.ViewModelObject as ItemViewModel; }
    }

    public ItemDrawer()
    {
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
        get
        {
            if (ViewModelObject.IsMouseOver)
                return ElementDesignerStyles.Item5;
            if (ViewModelObject.IsSelected)
                return ElementDesignerStyles.Item1;

            return ElementDesignerStyles.ClearItemStyle;
        }
        set { _selectedItemStyle = value; }
    }
    public GUIStyle TextStyle
    {
        get { return _textStyle ?? (_textStyle = ElementDesignerStyles.Item4); }
        set { _textStyle = value; }
    }

    public override void OnMouseEnter(MouseEvent e)
    {
        base.OnMouseEnter(e);
        ViewModelObject.IsMouseOver = true;
        //Debug.Log("Mouse Enter Item");
    }
    public override void OnMouseExit(MouseEvent e)
    {
        base.OnMouseExit(e);
        ViewModelObject.IsMouseOver = false;
    }

    public override void OnMouseDown(MouseEvent mouseEvent)
    {
        base.OnMouseDown(mouseEvent);
        ViewModelObject.Select();
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

    public virtual void DrawOption()
    {
        
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
            DrawOption();
            GUI.SetNextControlName(ItemViewModel.Name);
            var newName = EditorGUILayout.TextField(ItemViewModel.Name, ElementDesignerStyles.ClearItemStyle);
            if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(newName))
            {
                ItemViewModel.Rename(newName);
                //uFrameEditor.ExecuteCommand(p => );
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
          
            GUI.Box(Bounds.Scale(scale), string.Empty, SelectedItemStyle);
            GUILayout.BeginArea(Bounds);
            EditorGUILayout.BeginHorizontal();
            DrawOption();
            var style = new GUIStyle(TextStyle);
            style.normal.textColor = BackgroundStyle.normal.textColor;
            GUILayout.Label(ItemViewModel.Name, style);
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        if (!string.IsNullOrEmpty(ItemViewModel.Highlighter))
        {
            var highlighterPosition = new Rect(Bounds) {width = 4};
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