namespace Invert.uFrame.Editor.ViewModels
{
    public class ElementPropertyItemViewModel : ElementItemViewModel<ViewModelPropertyData>
    {
        public ElementPropertyItemViewModel(ViewModelPropertyData data, DiagramNodeViewModel nodeViewModel)
            : base(data, nodeViewModel)
        {
            DataObject = data;
        }

        public bool IsComputed
        {
            get { return ElementItem.IsComputed; }
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