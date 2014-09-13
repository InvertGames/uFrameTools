namespace Invert.uFrame.Editor.ViewModels
{
    public abstract class ElementItemViewModel<TElementItem> : TypedItemViewModel
    {
        public TElementItem ElementItem
        {
            get { return (TElementItem)DataObject; }
        }

        public ElementItemViewModel(ITypeDiagramItem viewModelItem, DiagramNodeViewModel nodeViewModel)
            : base(viewModelItem, nodeViewModel)
        {
        }
    }
}