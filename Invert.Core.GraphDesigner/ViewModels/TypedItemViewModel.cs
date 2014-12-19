namespace Invert.Core.GraphDesigner
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
                var rt = Data.RelatedType;
                if (string.IsNullOrEmpty(rt))
                {
                    return "[None]";
                }
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