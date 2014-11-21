namespace Invert.uFrame.Editor.ViewModels
{
    public abstract class TypedItemViewModel : ItemViewModel<ITypedItem>
    {
        protected TypedItemViewModel(ITypedItem viewModelItem, DiagramNodeViewModel nodeViewModel)
            : base(nodeViewModel)
        {
            DataObject = viewModelItem;
        }

 
        public string RelatedType
        {
            get
            {
                return Data.RelatedType;//ElementDataBase.TypeAlias(Data.RelatedType);
            }
            set
            {
                Data.RelatedType = value;
            }
        }

        public virtual string TypeLabel
        {
            get { return Data.RelatedTypeName; }
        }
    }
}