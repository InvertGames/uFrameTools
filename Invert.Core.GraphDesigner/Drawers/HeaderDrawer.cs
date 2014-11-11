using Invert.Common;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

public class HeaderDrawer : Drawer
{
    private GUIStyle _textStyle;
    private GUIStyle _backgroundStyle;
    private float _padding = 12;

    public virtual float Padding
    {
        get { return _padding; }
        set { _padding = value; }
    }

    public GUIStyle BackgroundStyle
    {
        get { return _backgroundStyle ?? (_backgroundStyle = ElementDesignerStyles.ItemStyle); }
        set { _backgroundStyle = value; }
    }

    public GUIStyle TextStyle
    {
        get { return _textStyle ?? (_textStyle = ElementDesignerStyles.ViewModelHeaderStyle); }
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
        var width = TextSize.x + (Padding*2);
        
        if (NodeViewModel.IsCollapsed)
        {
            this.Bounds = new Rect(position.x, position.y, width + 12, TextSize.y + (Padding * 2));
         
        }
        else
        {
            this.Bounds = new Rect(position.x, position.y, width + 12, 32);
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
            AdjustedBounds = new Rect(Bounds.x - 9, Bounds.y + 1, Bounds.width + 19, 27 * scale);
        }
        if (NodeViewModel.IsCollapsed )
        {
            ElementDesignerStyles.DrawExpandableBox(AdjustedBounds.Scale(scale), BackgroundStyle, string.Empty, 20 * scale);
        }
        else
        {
            ElementDesignerStyles.DrawExpandableBox(AdjustedBounds.Scale(scale), BackgroundStyle, string.Empty, new RectOffset(Mathf.RoundToInt(20 * scale), Mathf.RoundToInt(20 * scale),Mathf.RoundToInt( 27 * scale), 0));
        }

        //var style = new GUIStyle(TextStyle)
        //{
        //    normal = {textColor = BackgroundStyle.normal.textColor},
        //    alignment = TextAnchor.UpperLeft
        //};
        // The bounds for the main text
        var textBounds = new Rect(Bounds.x, Bounds.y + ((Bounds.height / 2f) - (TextSize.y / 2f)), Bounds.width,
            Bounds.height);

        if (NodeViewModel.IsEditing)
        {
            GUI.SetNextControlName("EditingField");
            ElementsDiagram.IsEditingField = true;
            EditorGUI.BeginChangeCheck();

            var newText = GUI.TextField(textBounds.Scale(scale), NodeViewModel.Name, ElementDesignerStyles.ViewModelHeaderEditingStyle);

            if (EditorGUI.EndChangeCheck())
            {
                NodeViewModel.Rename(newText);
                Dirty = true;
            }
          

            textBounds.y += TextSize.y / 2f;
        }
        else
        {
            var titleStyle = new GUIStyle(TextStyle);
            titleStyle.normal.textColor = BackgroundStyle.normal.textColor;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.fontSize = Mathf.RoundToInt(12 * scale);

            GUI.Label(textBounds.Scale(scale), NodeViewModel.Label  ?? string.Empty, titleStyle);
            if (NodeViewModel.IsCollapsed)
            {
                textBounds.y += TextSize.y / 2f;
                titleStyle.fontSize = Mathf.RoundToInt(10 * scale);
                titleStyle.fontStyle = FontStyle.Italic;
                GUI.Label(textBounds.Scale(scale), NodeViewModel.SubTitle, titleStyle);    
            }
            
        }
    }
}