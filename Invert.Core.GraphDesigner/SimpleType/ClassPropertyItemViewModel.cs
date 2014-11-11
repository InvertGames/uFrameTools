using Invert.uFrame.Editor.ViewModels;

public class ClassPropertyItemViewModel : ElementItemViewModel<ClassPropertyData>
{

    public ClassPropertyItemViewModel(ClassPropertyData viewModelItem, DiagramNodeViewModel nodeViewModel) : base(viewModelItem, nodeViewModel)
    {
    }

    public override string TypeLabel
    {
        get { return Data.RelatedTypeName; }
    }
}