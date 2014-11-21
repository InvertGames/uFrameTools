using Invert.Common;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

public class TypedItemDrawer : ItemDrawer
{
    public TypedItemViewModel TypedItemViewModel
    {
        get
        {
            return ViewModelObject as TypedItemViewModel;
        }
    }
    public TypedItemDrawer(TypedItemViewModel viewModel)
    {
        ViewModelObject = viewModel;
    }

    public override void Refresh(Vector2 position)
    {
        base.Refresh(position);
        var nameSize = TextStyle.CalcSize(new GUIContent(TypedItemViewModel.Name));
        var typeSize = TextStyle.CalcSize(new GUIContent(TypedItemViewModel.TypeLabel));

        Bounds = new Rect(position.x, position.y, 5 + nameSize.x + 5 + typeSize.x + 10, 18);
    }

    public override void DrawOption()
    {
        base.DrawOption();

        if (GUILayout.Button(TypedItemViewModel.TypeLabel + (TypedItemViewModel.IsMouseOver ? "..." : string.Empty),ElementDesignerStyles.ClearItemStyle))
        {
            TypedItemViewModel.NodeViewModel.IsSelected = true;
            OptionClicked();
        }
    }

    public virtual void OptionClicked()
    {
        var commandName = ViewModelObject.DataObject.GetType().Name.Replace("Data","") + "TypeSelection";
        
        var command = InvertGraphEditor.Container.Resolve<IEditorCommand>(commandName);
        TypedItemViewModel.Select();
        
        InvertGraphEditor.ExecuteCommand(command);
    }

    public override void Draw(float scale)
    {

        base.Draw(scale);
       
    }
}