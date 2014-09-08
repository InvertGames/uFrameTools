using Invert.Common;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

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
        var typeSize = TextStyle.CalcSize(new GUIContent(ElementItemViewModel.TypeLabel));

        Bounds = new Rect(position.x, position.y, 5 + nameSize.x + 5 + typeSize.x + 10, 18);
    }

    public override void DrawOption()
    {
        base.DrawOption();

        if (GUILayout.Button(ElementItemViewModel.TypeLabel,ElementDesignerStyles.SelectedItemStyle))
        {
            ElementItemViewModel.NodeViewModel.IsSelected = true;
            OptionClicked();
        }
    }

    public virtual void OptionClicked()
    {
        var commandName = ViewModelObject.DataObject.GetType().Name.Replace("Data","") + "TypeSelection";
        Debug.Log(commandName);
        var command = uFrameEditor.Container.Resolve<IEditorCommand>(commandName);
        ElementItemViewModel.Select();
        Debug.Log(command.GetType().Name);
        uFrameEditor.ExecuteCommand(command);
    }

    public override void Draw(float scale)
    {

        base.Draw(scale);
       
    }
}