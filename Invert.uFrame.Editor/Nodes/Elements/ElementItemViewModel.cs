namespace Invert.uFrame.Editor.ViewModels
{
    public abstract class ElementItemViewModel<TElementItem> : ElementItemViewModel
    {
        public TElementItem ElementItem
        {
            get { return (TElementItem)DataObject; }
        }

        public ElementItemViewModel(IViewModelItem viewModelItem, DiagramNodeViewModel nodeViewModel)
            : base(viewModelItem, nodeViewModel)
        {
        }
    }
}