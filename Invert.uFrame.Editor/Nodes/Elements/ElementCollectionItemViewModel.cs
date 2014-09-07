namespace Invert.uFrame.Editor.ViewModels
{
    public class ElementCollectionItemViewModel : ElementItemViewModel<ViewModelCollectionData>
    {
        public ElementCollectionItemViewModel(ViewModelCollectionData data, DiagramNodeViewModel nodeViewModel)
            : base(data, nodeViewModel)
        {
            DataObject = data;
        }
    }
}