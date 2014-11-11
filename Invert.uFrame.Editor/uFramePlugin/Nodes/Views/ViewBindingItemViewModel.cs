using Invert.uFrame.Code.Bindings;

namespace Invert.uFrame.Editor.ViewModels
{
    public class ViewBindingItemViewModel : ItemViewModel<ViewBindingData>
    {
        public ViewBindingItemViewModel(ViewBindingData data, DiagramNodeViewModel nodeViewModel)
            : base(nodeViewModel)
        {
            DataObject = data;
        }

        public override ConnectorViewModel InputConnector
        {
            get { return null; }
        }

        public override ConnectorViewModel OutputConnector
        {
            get { return null; }
        }

        public override bool IsEditable
        {
            get { return false; }
        }
    }
}