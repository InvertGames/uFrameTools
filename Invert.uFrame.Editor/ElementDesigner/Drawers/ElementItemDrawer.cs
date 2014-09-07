using Invert.uFrame.Editor.ViewModels;
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
        var typeSize = TextStyle.CalcSize(new GUIContent(ElementItemViewModel.RelatedType));

        Bounds = new Rect(position.x, position.y, 5 + nameSize.x + 5 , 18);
    }

    public override void Draw(float scale)
    {

        base.Draw(scale);
    }
}