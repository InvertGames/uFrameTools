namespace Invert.uFrame.Editor.ViewModels
{
    public class ElementPropertyItemViewModel : TypedItemViewModel<ViewModelPropertyData>
    {
        public ElementPropertyItemViewModel(ViewModelPropertyData data, DiagramNodeViewModel nodeViewModel)
            : base(data, nodeViewModel)
        {
            DataObject = data;
        }

        //public bool IsComputed
        //{
        //    get { return ElementItem.IsComputed; }
        //}

        public override string TypeLabel
        {
            get
            {
                return ElementDataBase.TypeAlias(Data.RelatedTypeName);
            }
        }
    }
    public class ElementViewPropertyItemViewModel : TypedItemViewModel<ViewPropertyData>
    {
        public ElementViewPropertyItemViewModel(ViewPropertyData data, DiagramNodeViewModel nodeViewModel)
            : base(data, nodeViewModel)
        {
            DataObject = data;
        }

        public override bool IsEditable
        {
            get { return false; }
        }

        public override bool AllowRemoving
        {
            get { return false; }
        }

        public override ConnectorViewModel OutputConnector
        {
            get { return null; }
        }

        public override string TypeLabel
        {
            get
            {
                return ElementDataBase.TypeAlias(Data.RelatedTypeName);
            }
        }
    }
}