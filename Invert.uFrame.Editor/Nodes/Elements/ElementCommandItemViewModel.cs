namespace Invert.uFrame.Editor.ViewModels
{
    public class ElementCommandItemViewModel : ElementItemViewModel<ViewModelCommandData>
    {
        public ElementCommandItemViewModel(ViewModelCommandData data, DiagramNodeViewModel nodeViewModel)
            : base(data, nodeViewModel)
        {
            DataObject = data;
        }

        public override string Name
        {
            get
            {
                return string.Format("{0} ({1})", Data.Name,
                    ElementDataBase.TypeAlias(Data.RelatedTypeName) ?? string.Empty);
            }
        }

        public override Invert.uFrame.Editor.ViewModels.ConnectorViewModel InputConnector
        {
            get { return null; }
        }
    }
}