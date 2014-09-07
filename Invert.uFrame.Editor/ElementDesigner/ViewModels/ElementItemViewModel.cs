namespace Invert.uFrame.Editor.ViewModels
{
    public class ElementItemViewModel : ItemViewModel<IViewModelItem>
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
    }
}