namespace Invert.uFrame.Editor.ViewModels
{
    public abstract class ElementItemViewModel<TElementItem> : TypedItemViewModel
    {
        public TElementItem ElementItem
        {
            get { return (TElementItem)DataObject; }
        }

        protected ElementItemViewModel(ITypeDiagramItem viewModelItem, DiagramNodeViewModel nodeViewModel)
            : base(viewModelItem, nodeViewModel)
        {

        }

  
        public override bool IsEditable
        {
            get { return base.IsEditable; }
        }

        public override ConnectorViewModel InputConnector
        {
            get { return null; }
        }
    }
}