namespace Invert.uFrame.Editor.ViewModels
{
    public abstract class TypedItemViewModel : ItemViewModel<ITypeDiagramItem>
    {
        protected TypedItemViewModel(ITypeDiagramItem viewModelItem, DiagramNodeViewModel nodeViewModel)
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

    public class RegisterInstanceItemViewModel : TypedItemViewModel
    {
        public override ConnectorViewModel InputConnector
        {
            get { return null; }
        }

        public override ConnectorViewModel OutputConnector
        {
            get { return null; }
        }

        public RegisterInstanceItemViewModel(RegisteredInstanceData viewModelItem, DiagramNodeViewModel nodeViewModel) : base(viewModelItem, nodeViewModel)
        {
        }

        public override string TypeLabel
        {
            get
            {
                return Data.RelatedTypeName ?? string.Empty;
            }
        }
    }
}