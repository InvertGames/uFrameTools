namespace Invert.uFrame.Editor.ViewModels
{
    public abstract class ElementItemViewModel : ItemViewModel<IViewModelItem>
    {

        public ElementItemViewModel(IViewModelItem viewModelItem, DiagramNodeViewModel nodeViewModel)
            : base(nodeViewModel)
        {
            DataObject = viewModelItem;
        }
        
        public string RelatedType
        {
            get
            {
                return ElementDataBase.TypeAlias(Data.RelatedType);
            }
            set
            {
                Data.RelatedType = value;
            }
        }

        public abstract string TypeLabel { get; }
    }
}