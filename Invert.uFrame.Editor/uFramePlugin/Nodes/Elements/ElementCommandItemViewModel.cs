using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor.ViewModels
{
    public class ElementCommandItemViewModel : TypedItemViewModel<ViewModelCommandData>
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
                return Data.Name;
            }
        }

        public override ConnectorViewModel InputConnector
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