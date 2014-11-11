using Invert.uFrame.Editor.ViewModels;

public class ClassCollectionItemViewModel : ElementItemViewModel<ClassCollectionData>
{
    public ClassCollectionItemViewModel(ClassCollectionData viewModelItem, DiagramNodeViewModel nodeViewModel)
        : base(viewModelItem, nodeViewModel)
    {
    }

    public override string TypeLabel
    {
        get { return Data.RelatedTypeName; }
    }
}