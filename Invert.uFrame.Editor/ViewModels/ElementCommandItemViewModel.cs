namespace Invert.uFrame.Editor.ViewModels
{
    public class ElementCommandItemViewModel : ElementItemViewModel<ViewModelCommandData>
    {
        public ElementCommandItemViewModel(ViewModelCommandData data, DiagramNodeViewModel nodeViewModel)
            : base(data, nodeViewModel)
        {
            DataObject = data;
        }
        public override Invert.uFrame.Editor.ViewModels.ConnectorViewModel InputConnector
        {
            get { return null; }
        }
    }
}