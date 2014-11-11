namespace Invert.uFrame.Editor.ViewModels
{
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