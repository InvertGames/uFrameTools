namespace Invert.uFrame.Editor.ViewModels
{
    public class EnumItemViewModel : ItemViewModel<EnumItem>
    {
        public EnumItemViewModel(EnumItem item, DiagramNodeViewModel nodeViewModel)
            : base(nodeViewModel)
        {
            DataObject = item;
        }

        public override ConnectorViewModel InputConnector
        {
            get { return null; }
        }

        public override ConnectorViewModel OutputConnector
        {
            get { return null; }
        }

        public override string Name
        {
            get { return Data.Name; }
            set { Data.Name = value; }
        }
    }
}